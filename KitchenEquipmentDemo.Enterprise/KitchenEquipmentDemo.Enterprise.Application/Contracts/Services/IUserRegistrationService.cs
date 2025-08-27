using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Authorization;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.UserRegistrations.KitchenEquipmentDemo.Enterprise.WPF.Dtos;

namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Services
{
    public interface IUserRegistrationService
    {
        Task<OperationResult> RequestSignupAsync(SignupRequestDto request);
        Task<OperationResult> ApproveAsync(int requestId, int reviewerUserId);
        Task<OperationResult> DenyAsync(int requestId, int reviewerUserId, string note);
        Task<PagedResult<UserRegistrationDto>> GetPagedAsync(
        int page,
        int pageSize,
        string searchString,
        string status,
        int actorUserId,
        string orderBy = "UserId",
        bool orderByDescending = false);
    }
}
