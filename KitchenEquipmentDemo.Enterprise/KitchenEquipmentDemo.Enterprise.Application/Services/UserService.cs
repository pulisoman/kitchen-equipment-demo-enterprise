using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Users;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.Data.Repositories;
using KitchenEquipmentDemo.Enterprise.Data.Uow;
using KitchenEquipmentDemo.Enterprise.Data.Models;
using KitchenEquipmentDemo.Enterprise.Application.Helpers;

namespace KitchenEquipmentDemo.Enterprise.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepo;
        private readonly UnitOfWork _uow;

        public UserService(UserRepository userRepo, UnitOfWork uow)
        {
            _userRepo = userRepo;
            _uow = uow;
        }

        public async Task<PagedResult<UserDto>> ListAsync(int page, int pageSize, string search = null, int actorUserId = 0)
        {
            // Ensure SuperAdmin
            var actor = await _userRepo.GetByIdAsync(actorUserId);
            if (actor == null || actor.IsDeleted || !IsSuperAdmin(actor))
                return new PagedResult<UserDto> { Page = page, PageSize = pageSize, Total = 0 };

            // Expected repo: GetAllActiveAsync() or similar; fallback: get all and filter
            var all = await _userRepo.GetAllAsync(); // add if missing
            var filtered = all.Where(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                filtered = filtered.Where(u =>
                    (u.UserName ?? "").IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (u.EmailAddress ?? "").IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (u.FirstName ?? "").IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (u.LastName ?? "").IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var total = filtered.Count();
            var items = filtered
                .OrderBy(u => u.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(Map)
                .ToList();

            return new PagedResult<UserDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }

        public async Task<OperationResult<UserDto>> GetAsync(int userId, int actorUserId = 0)
        {
            var actor = await _userRepo.GetByIdAsync(actorUserId);
            if (actor == null || actor.IsDeleted || (!IsSuperAdmin(actor) && userId != actorUserId))
                return OperationResult<UserDto>.Fail("Forbidden.");

            var u = await _userRepo.GetByIdAsync(userId);
            if (u == null || u.IsDeleted) return OperationResult<UserDto>.Fail("User not found.");
            return OperationResult<UserDto>.Success(Map(u));
        }

        public async Task<OperationResult<UserDto>> UpdateAsync(UserDto dto, int actorUserId)
        {
            var actor = await _userRepo.GetByIdAsync(actorUserId);
            if (actor == null || actor.IsDeleted || !IsSuperAdmin(actor))
                return OperationResult<UserDto>.Fail("Forbidden.");

            var u = await _userRepo.GetByIdAsync(dto.UserId);
            if (u == null || u.IsDeleted) return OperationResult<UserDto>.Fail("User not found.");

            // Simple updates (no username/email changes here; add uniqueness checks if you allow)
            u.FirstName = dto.FirstName?.Trim();
            u.LastName = dto.LastName?.Trim();
            u.UserType = dto.UserType == UserType.SuperAdmin ? "SuperAdmin" : "Admin";
            u.UpdatedBy = actorUserId;
            u.UpdatedAt = DateTime.UtcNow;

            _userRepo.Update(u);
            await _uow.SaveChangesAsync();
            return OperationResult<UserDto>.Success(Map(u), "User updated.");
        }

        public async Task<OperationResult> DeleteAsync(int userId, int actorUserId)
        {
            var actor = await _userRepo.GetByIdAsync(actorUserId);
            if (actor == null || actor.IsDeleted || !IsSuperAdmin(actor))
                return OperationResult.Fail("Forbidden.");

            if (userId == actorUserId)
                return OperationResult.Fail("You cannot delete your own account.");

            var u = await _userRepo.GetByIdAsync(userId);
            if (u == null || u.IsDeleted) return OperationResult.Success("Already deleted.");
            if (IsSuperAdmin(u)) return OperationResult.Fail("Cannot delete a SuperAdmin.");

            u.IsDeleted = true;
            u.UpdatedBy = actorUserId;
            u.UpdatedAt = DateTime.UtcNow;
            _userRepo.Update(u);

            await _uow.SaveChangesAsync();
            return OperationResult.Success("User deleted.");
        }

        public async Task<PagedResult<UserDto>> GetPagedAsync(
        int page,
        int pageSize,
        string searchString,
        bool showDeletedOnly,
        int actorUserId,
        string orderBy = "UserId",
        bool orderByDescending = false)
        {
            var actor = await _userRepo.GetByIdAsync(actorUserId);
            if (actor == null || actor.IsDeleted || !IsSuperAdmin(actor))
                return new PagedResult<UserDto> { Page = page, PageSize = pageSize, Total = 0, Items = new List<UserDto>() };

            var all = await _userRepo.GetAllAsync();

            IEnumerable<User> query = showDeletedOnly
                ? all.Where(u => u.IsDeleted)
                : all.Where(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var s = searchString.Trim();

                query = query.Where(u =>
                    u.UserId.ToString().Contains(s) ||
                    (u.UserName ?? "").Contains(s) ||
                    (u.FirstName + " " + u.LastName).Contains(s));
            }

            var orderedQuery = query;
            switch (orderBy)
            {
                case nameof(User.UserName):
                    orderedQuery = orderByDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName);
                    break;
                case nameof(User.FirstName):
                    orderedQuery = orderByDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName);
                    break;
                case nameof(User.LastName):
                    orderedQuery = orderByDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName);
                    break;
                case nameof(User.EmailAddress):
                    orderedQuery = orderByDescending ? query.OrderByDescending(u => u.EmailAddress) : query.OrderBy(u => u.EmailAddress);
                    break;
                case nameof(User.UserId):
                    orderedQuery = orderByDescending ? query.OrderByDescending(u => u.UserId) : query.OrderBy(u => u.UserId);
                    break;
                case nameof(User.UserType):
                    orderedQuery = orderByDescending ? query.OrderByDescending(u => u.UserType) : query.OrderBy(u => u.UserType);
                    break;
                case nameof(User.CreatedAt):
                    orderedQuery = orderByDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt);
                    break;
                default:
                    orderedQuery = query.OrderBy(u => u.UserId);
                    break;
            }


            var total = orderedQuery.Count();

            //compute for totalpages and adjust page if necessary (though UI should handle this) handle if there are no results at all
            if (total == 0)
            {
                return new PagedResult<UserDto>
                {
                    Page = page,
                    PageSize = pageSize,
                    Total = total,
                    TotalPages = 1,
                    Items = new List<UserDto>()
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

            return new PagedResult<UserDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                TotalPages = totalPages,
                Items = items
            };
        }


        // ---- helpers ----
        private static bool IsSuperAdmin(User u) => string.Equals(u.UserType, "SuperAdmin", StringComparison.Ordinal);

        private static UserDto Map(User u) => new UserDto
        {
            UserId = u.UserId,
            UserType = string.Equals(u.UserType, "SuperAdmin", StringComparison.Ordinal)
                ? UserType.SuperAdmin
                : UserType.Admin,
            FirstName = u.FirstName,
            LastName = u.LastName,
            EmailAddress = u.EmailAddress,
            UserName = u.UserName,
            IsDeleted = u.IsDeleted,
            CreatedAt = u.CreatedAt
        };


        public async Task<OperationResult> UpdatePasswordAsync(UserPasswordUpdateDto dto, int actorUserId)
        {
            // Basic validation of input if empty for each field
            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return OperationResult.Fail("New password is required.");

            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                return OperationResult.Fail("Confirm password is required.");

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                return OperationResult.Fail("Current password is required.");

            if (dto.NewPassword != dto.ConfirmPassword)
                return OperationResult.Fail("New password and confirm password do not match.");

            var actor = await _userRepo.GetByIdAsync(actorUserId);
            if (actor == null || actor.IsDeleted)
                return OperationResult.Fail("Unauthorized User.");
            // Ensure the user making the request is the same as the user trying to change the password
            if (dto.UserId != actorUserId && actor.UserType != nameof(UserType.SuperAdmin))
                return OperationResult.Fail("Unauthorized password change attempt.");

            // Fetch the user from repository or DB
            var user = await _userRepo.GetByIdAsync(dto.UserId);
            if (user == null || user.IsDeleted)
                return OperationResult.Fail("User not found.");

            // Validate the current password
            bool passwordValid = PasswordHelper.Verify(actor.PasswordHash, actor.PasswordSalt, dto.CurrentPassword);
            if (!passwordValid)
                return OperationResult.Fail("Current password is incorrect.");

            // Hash and update the new password
            user.PasswordHash = PasswordHelper.Hash(dto.NewPassword, user.PasswordSalt);
            user.UpdatedBy = actorUserId;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepo.Update(user);

            await _uow.SaveChangesAsync();
            return OperationResult.Success("Password updated successfully.");
        }

        public async Task<OperationResult> UpdateUserInfoAsync(UserInfoUpdateDto dto, int actorUserId)
        {
            // Basic validation of input if empty for each field
            if (string.IsNullOrWhiteSpace(dto.FirstName))
                return OperationResult.Fail("First name is required.");

            if (string.IsNullOrWhiteSpace(dto.LastName))
                return OperationResult.Fail("Last name is required.");

            if (string.IsNullOrWhiteSpace(dto.EmailAddress))
                return OperationResult.Fail("Email address is required.");

            if (string.IsNullOrWhiteSpace(dto.UserName))
                return OperationResult.Fail("User name is required.");

            var actor = await _userRepo.GetByIdAsync(actorUserId);
            if (actor == null || actor.IsDeleted)
                return OperationResult.Fail("Unauthorized User.");
            // Ensure the user making the request is the same as the user trying to change the password
            if (dto.UserId != actorUserId && actor.UserType != nameof(UserType.SuperAdmin))
                return OperationResult.Fail("Unauthorized password change attempt.");

            //check if the username or email address is already taken by another user
            var isUserNameTaken = await _userRepo.UserNameExistsAsync(dto.UserName,  dto.UserId);
            if (isUserNameTaken)
                return OperationResult.Fail("User name is already taken.");

            var isEmailTaken = await _userRepo.EmailExistsAsync(dto.EmailAddress, dto.UserId);
            if (isEmailTaken)
                return OperationResult.Fail("Email address is already taken.");

            if(IsValidEmail(dto.EmailAddress) == false)
                return OperationResult.Fail("Email address is not valid.");

            // Fetch the user from repository or DB
            var user = await _userRepo.GetByIdAsync(dto.UserId);
            if (user == null || user.IsDeleted)
                return OperationResult.Fail("User not found.");

            //pass the dto values to user object
            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.EmailAddress = dto.EmailAddress.Trim();
            user.UserName = dto.UserName.Trim();
            user.UserType = dto.UserType.ToString();
            user.UpdatedBy = actorUserId;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepo.Update(user);

            await _uow.SaveChangesAsync();
            return OperationResult.Success("User info updated successfully.");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}
