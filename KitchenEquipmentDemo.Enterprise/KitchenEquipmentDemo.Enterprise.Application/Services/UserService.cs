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
            if (actor == null || actor.IsDeleted || !IsSuperAdmin(actor))
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
            IsDeleted = u.IsDeleted
        };
    }
}
