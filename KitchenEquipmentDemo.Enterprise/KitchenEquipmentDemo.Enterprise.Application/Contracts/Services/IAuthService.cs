using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Authorization;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;

namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Services
{
    public interface IAuthService
    {
        Task<OperationResult<LoginResultDto>> LoginAsync(LoginRequestDto request);
        Task LogoutAsync();
    }
}
