using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Equipments;

namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Services
{
    public interface IEquipmentService
    {
        Task<PagedResult<EquipmentDto>> ListAsync(int userId, int page, int pageSize, string search = null);
        Task<OperationResult<EquipmentDto>> GetAsync(int equipmentId);
        Task<OperationResult<EquipmentDto>> CreateAsync(EquipmentCreateDto dto, int actorUserId);
        Task<OperationResult<EquipmentDto>> UpdateAsync(EquipmentUpdateDto dto, int actorUserId);
        Task<OperationResult> DeleteAsync(int equipmentId, int actorUserId); // null if skipping concurrency

        Task<PagedResult<EquipmentDto>> GetPagedAsync(
    int page,
    int pageSize,
    string searchString,
    int? ownerId, // New parameter for owner filtering
    int actorUserId,
    string orderBy = "EquipmentId",
    bool orderByDescending = false);
    }
}
