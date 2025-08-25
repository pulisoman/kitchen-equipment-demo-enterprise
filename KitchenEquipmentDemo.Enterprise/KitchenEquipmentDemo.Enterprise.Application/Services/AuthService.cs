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

            var ok = VerifyPassword(user, request.PasswordPlain);
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
            if (user.PasswordHash == null || user.PasswordSalt == null) return false;

            // Basic PBKDF2 verify. If you store algo/iterations, branch by user.PasswordAlgo/Version.
            // Assume 100,000 iterations, HMACSHA256 by default; adjust to your seeding.
            const int iterations = 100000;
            using (var pbkdf2 = new Rfc2898DeriveBytes(passwordPlain, user.PasswordSalt, iterations, HashAlgorithmName.SHA256))
            {
                var candidate = pbkdf2.GetBytes(user.PasswordHash.Length);
                // constant-time compare
                if (candidate.Length != user.PasswordHash.Length) return false;
                int diff = 0;
                for (int i = 0; i < candidate.Length; i++) diff |= candidate[i] ^ user.PasswordHash[i];
                return diff == 0;
            }
        }
    }
}
