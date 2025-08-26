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
    public class AuthService : IAuthService
    {
        private readonly UserRepository _userRepo;
        private readonly UnitOfWork _uow;

        public AuthService(UserRepository userRepo, UnitOfWork uow)
        {
            _userRepo = userRepo;
            _uow = uow;
        }

        public async Task<OperationResult<LoginResultDto>> LoginAsync(LoginRequestDto request)
        {
            // Expected repo method (add if missing): FindByUserNameAsync(string userName)
            var user = await _userRepo.FindByUserNameAsync(request.UserName);
            if (user == null || user.IsDeleted)
                return OperationResult<LoginResultDto>.Success(new LoginResultDto
                {
                    Success = false,
                    FailureReason = "Invalid username or password."
                });

            var ok = VerifyPassword(user, request.Password);
            if (!ok)
                return OperationResult<LoginResultDto>.Success(new LoginResultDto
                {
                    Success = false,
                    FailureReason = "Invalid username or password."
                });

            return OperationResult<LoginResultDto>.Success(new LoginResultDto
            {
                Success = true,
                UserId = user.UserId,
                UserType = user.UserType == "SuperAdmin"
                    ? UserType.SuperAdmin
                    : UserType.Admin,
                FullName = $"{user.FirstName} {user.LastName}".Trim()
            });
        }

        public Task LogoutAsync()
        {
            // WPF clears local session/state; nothing to persist server-side.
            return Task.CompletedTask;
        }

        // ---- helpers ----

        private static bool VerifyPassword(User user, string passwordPlain)
        {
            if (string.IsNullOrEmpty(passwordPlain)) return false;

            var hash = user?.PasswordHash;
            var salt = user?.PasswordSalt;

            // Reject null/empty/incorrect sizes up front
            if (hash == null || salt == null) return false;
            if (hash.Length == 0 || salt.Length == 0) return false;

            // If you know your sizes, enforce them (recommended):
            // PBKDF2-SHA256 32-byte hash, 16-byte salt (change if you use different)
            if (hash.Length != 32) return false;
            if (salt.Length != 16) return false;

            // Make sure these match whatever you used when creating the hash
            const int iterations = 100_000;

            byte[] candidate;
            using (var pbkdf2 = new Rfc2898DeriveBytes(passwordPlain, salt, iterations, HashAlgorithmName.SHA256))
            {
                candidate = pbkdf2.GetBytes(hash.Length);
            }

            // Constant-time compare (Framework 4.8 doesn’t have CryptographicOperations.FixedTimeEquals)
            if (candidate.Length != hash.Length) return false;
            int diff = 0;
            for (int i = 0; i < hash.Length; i++) diff |= candidate[i] ^ hash[i];
            return diff == 0;
        }
    }
}
