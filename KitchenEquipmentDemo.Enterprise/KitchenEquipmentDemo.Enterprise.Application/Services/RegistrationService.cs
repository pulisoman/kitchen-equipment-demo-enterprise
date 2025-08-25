using System;
using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Authorization;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.Data.Repositories;
using KitchenEquipmentDemo.Enterprise.Data.Uow;
using KitchenEquipmentDemo.Enterprise.Data.Models;
using System.Security.Cryptography;

namespace KitchenEquipmentDemo.Enterprise.Application.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly UserRegistrationRequestRepository _reqRepo;
        private readonly UserRepository _userRepo;
        private readonly UnitOfWork _uow;

        public RegistrationService(
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
                string.IsNullOrWhiteSpace(request.PasswordPlain) ||
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
                RequestedRole = request.RequestedRole == UserType.SuperAdmin ? "SuperAdmin" : "Admin",
                Status = "Pending",
            };
            urr.PasswordHash = Pbkdf2(request.PasswordPlain, urr.PasswordSalt);

            await _reqRepo.AddAsync(urr);
            await _uow.SaveChangesAsync();
            return OperationResult.Success("Signup request submitted.");
        }

        public async Task<OperationResult> ApproveAsync(int requestId, int reviewerUserId)
        {
            var req = await _reqRepo.GetByIdAsync(requestId);
            if (req == null || req.Status != "Pending")
                return OperationResult.Fail("Request not found or already processed.");

            // Create user from request
            var user = new User
            {
                FirstName = req.FirstName,
                LastName = req.LastName,
                EmailAddress = req.EmailAddress,
                UserName = req.UserName,
                PasswordSalt = req.PasswordSalt,
                PasswordHash = req.PasswordHash,
                UserType = req.RequestedRole, // "Admin" or "SuperAdmin" depending on your rule
                IsDeleted = false,
                CreatedBy = reviewerUserId
            };
            await _userRepo.AddAsync(user);

            // Mark request approved
            req.Status = "Approved";
            req.ReviewedBy = reviewerUserId;
            req.ReviewedAt = DateTime.UtcNow;
            req.ReviewNote = "Approved";

            _reqRepo.Update(req);
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
    }
}
