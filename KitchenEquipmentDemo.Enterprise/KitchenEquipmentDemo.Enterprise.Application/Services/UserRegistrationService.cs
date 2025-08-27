using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Authorization;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.UserRegistrations.KitchenEquipmentDemo.Enterprise.WPF.Dtos;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Users;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.Data.Models;
using KitchenEquipmentDemo.Enterprise.Data.Repositories;
using KitchenEquipmentDemo.Enterprise.Data.Uow;

namespace KitchenEquipmentDemo.Enterprise.Application.Services
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly UserRegistrationRequestRepository _reqRepo;
        private readonly UserRepository _userRepo;
        private readonly UnitOfWork _uow;

        public UserRegistrationService(
            UserRegistrationRequestRepository reqRepo,
            UserRepository userRepo,
            UnitOfWork uow)
        {
            _reqRepo = reqRepo;
            _userRepo = userRepo;
            _uow = uow;
        }

        public async Task<OperationResult> RequestSignupAsync(SignupRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.EmailAddress) ||
                string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName))
                return OperationResult.Fail("All fields are required.");

            // Expected repo helpers: UserNameExistsAsync, EmailExistsAsync, PendingRequestExistsAsync(userName/email)
            if (await _userRepo.UserNameExistsAsync(request.UserName))
                return OperationResult.Fail("Username already exists.");
            if (await _userRepo.EmailExistsAsync(request.EmailAddress))
                return OperationResult.Fail("Email already exists.");
            if (await _reqRepo.PendingExistsForAsync(request.UserName, request.EmailAddress))
                return OperationResult.Fail("A pending request already exists for this username or email.");

            var urr = new UserRegistrationRequest
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                EmailAddress = request.EmailAddress.Trim(),
                UserName = request.UserName.Trim(),
                // We don’t store plain password; hash now so approver doesn’t see it
                // PBKDF2 w/ random salt:
                PasswordSalt = RandomSalt(16),
                UserType = request.UserType.ToString(),
                Status = "Pending",
            };
            urr.PasswordHash = Pbkdf2(request.Password, urr.PasswordSalt);

            await _reqRepo.AddAsync(urr);
            await _uow.SaveChangesAsync();
            return OperationResult.Success("Signup request submitted.");
        }

        public async Task<OperationResult> ApproveAsync(int requestId, int reviewerUserId)
        {
            var request = await _reqRepo.GetByIdAsync(requestId);
            if (await _userRepo.UserNameExistsAsync(request.UserName))
                return OperationResult.Fail("Username already exists.");
            if (await _userRepo.EmailExistsAsync(request.EmailAddress))
                return OperationResult.Fail("Email already exists.");
            // Create user from request
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailAddress = request.EmailAddress,
                UserName = request.UserName,
                PasswordSalt = request.PasswordSalt,
                PasswordHash = request.PasswordHash,
                UserType = request.UserType, // "Admin" or "SuperAdmin" depending on your rule
                IsDeleted = false,
                CreatedBy = reviewerUserId
            };
            await _userRepo.AddAsync(user);

            // Mark request approved
            request.Status = "Approved";
            request.ReviewedBy = reviewerUserId;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewNote = "Approved";

            _reqRepo.Update(request);
            await _uow.SaveChangesAsync();
            return OperationResult.Success("Request approved and user created.");
        }

        public async Task<OperationResult> DenyAsync(int requestId, int reviewerUserId, string note)
        {
            var req = await _reqRepo.GetByIdAsync(requestId);
            if (req == null || req.Status != "Pending")
                return OperationResult.Fail("Request not found or already processed.");

            req.Status = "Denied";
            req.ReviewedBy = reviewerUserId;
            req.ReviewedAt = DateTime.UtcNow;
            req.ReviewNote = string.IsNullOrWhiteSpace(note) ? "Denied" : note.Trim();

            _reqRepo.Update(req);
            await _uow.SaveChangesAsync();
            return OperationResult.Success("Request denied.");
        }

        // ---- helpers ----
        private static byte[] RandomSalt(int len)
        {
            var salt = new byte[len];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            return salt;
        }

        private static byte[] Pbkdf2(string password, byte[] salt)
        {
            const int iterations = 100000;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                return pbkdf2.GetBytes(32); // 256-bit hash
        }

        public async Task<PagedResult<UserRegistrationDto>> GetPagedAsync(
        int page,
        int pageSize,
        string searchString,
        string status,
        int actorUserId,
        string orderBy = "RequestId",
        bool orderByDescending = false)
        {
            var actor = await _userRepo.GetByIdAsync(actorUserId);
            if (actor == null || actor.IsDeleted || !IsSuperAdmin(actor))
                return new PagedResult<UserRegistrationDto> { Page = page, PageSize = pageSize, Total = 0, Items = new List<UserRegistrationDto>() };

            var all = await _reqRepo.GetAllAsync();

            IEnumerable<UserRegistrationRequest> query = string.IsNullOrWhiteSpace(status) || status.Equals("All", StringComparison.OrdinalIgnoreCase) ? all : all.Where(u => u.Status == status);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var s = searchString.Trim();

                query = query.Where(u =>
                    u.RequestId.ToString().Contains(s) ||
                    (u.UserName ?? "").Contains(s) ||
                    (u.FirstName + " " + u.LastName).Contains(s));
            }

            var orderedQuery = query;
            switch (orderBy)
            {
                case nameof(UserRegistrationRequest.UserName):
                    orderedQuery = orderByDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName);
                    break;
                case nameof(UserRegistrationRequest.FirstName):
                    orderedQuery = orderByDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName);
                    break;
                case nameof(UserRegistrationRequest.LastName):
                    orderedQuery = orderByDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName);
                    break;
                case nameof(UserRegistrationRequest.EmailAddress):
                    orderedQuery = orderByDescending ? query.OrderByDescending(u => u.EmailAddress) : query.OrderBy(u => u.EmailAddress);
                    break;
                case nameof(UserRegistrationRequest.RequestId):
                    orderedQuery = orderByDescending ? query.OrderByDescending(u => u.RequestId) : query.OrderBy(u => u.RequestId);
                    break;
                case nameof(UserRegistrationRequest.UserType):
                    orderedQuery = orderByDescending ? query.OrderByDescending(u => u.UserType) : query.OrderBy(u => u.UserType);
                    break;
                case nameof(UserRegistrationRequest.CreatedAt):
                    orderedQuery = orderByDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt);
                    break;
                default:
                    orderedQuery = query.OrderBy(u => u.RequestId);
                    break;
            }


            var total = orderedQuery.Count();

            //compute for totalpages and adjust page if necessary (though UI should handle this) handle if there are no results at all
            if (total == 0)
            {
                return new PagedResult<UserRegistrationDto>
                {
                    Page = page,
                    PageSize = pageSize,
                    Total = total,
                    TotalPages = 1,
                    Items = new List<UserRegistrationDto>()
                };
            }

            var totalPages = total;

            if (total == 0)
                totalPages = 1;
            else
                totalPages = (int)Math.Ceiling(total / (double)pageSize);

            if (totalPages == 0)
                totalPages = 1;

            if (page > totalPages) page = totalPages;

            var items = orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(Map)
                .ToList();

            return new PagedResult<UserRegistrationDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                TotalPages = totalPages,
                Items = items
            };
        }

        private static bool IsSuperAdmin(User u) => string.Equals(u.UserType, "SuperAdmin", StringComparison.Ordinal);

        private static UserRegistrationDto Map(UserRegistrationRequest u) => new UserRegistrationDto
        {
            RequestId = u.RequestId,
            FirstName = u.FirstName,
            LastName = u.LastName,
            EmailAddress = u.EmailAddress,
            UserName = u.UserName,
            UserType = u.UserType,
            PasswordHash = u.PasswordHash,
            PasswordSalt = u.PasswordSalt,
            Status = u.Status,
            CreatedAt = u.CreatedAt,
            ReviewedBy = u.ReviewedBy,
            ReviewedAt = u.ReviewedAt,
            ReviewNote = u.ReviewNote
        };

    }
}
