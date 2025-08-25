using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Users;

namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Services
{
    public interface IUserService
    {
        // SuperAdmin-only operations (enforced in implementation)
        Task<PagedResult<UserDto>> ListAsync(int page, int pageSize, string search = null, int actorUserId = 0);
        Task<OperationResult<UserDto>> GetAsync(int userId, int actorUserId = 0);
        Task<OperationResult<UserDto>> UpdateAsync(UserDto dto, int actorUserId);
        Task<OperationResult> DeleteAsync(int userId, int actorUserId); // null if skipping concurrency
    }
}
