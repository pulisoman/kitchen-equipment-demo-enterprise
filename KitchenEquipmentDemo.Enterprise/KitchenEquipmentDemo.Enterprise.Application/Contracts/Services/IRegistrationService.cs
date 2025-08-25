using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Authorization;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;

namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Services
{
    public interface IRegistrationService
    {
        Task<OperationResult> RequestSignupAsync(SignupRequestDto request);
        Task<OperationResult> ApproveAsync(int requestId, int reviewerUserId);
        Task<OperationResult> DenyAsync(int requestId, int reviewerUserId, string note);
    }
}
