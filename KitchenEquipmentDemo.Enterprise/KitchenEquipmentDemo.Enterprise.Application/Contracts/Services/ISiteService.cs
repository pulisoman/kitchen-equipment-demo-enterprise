using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Sites;

namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Services
{
    public interface ISiteService
    {
        Task<PagedResult<SiteDto>> ListAsync(int userId, int page, int pageSize, string search = null, bool includeInactive = false);
        Task<OperationResult<SiteDto>> GetAsync(int userId, int siteId);
        Task<OperationResult<SiteDto>> FindByCodeAsync(string code); // code is global unique as per rule
        Task<OperationResult<SiteDto>> CreateAsync(SiteCreateDto dto, int actorUserId);
        Task<OperationResult<SiteDto>> UpdateAsync(SiteUpdateDto dto, int actorUserId);
        Task<OperationResult> DeleteAsync(int siteId, int actorUserId); // pass null if skipping concurrency
        Task<OperationResult> EditSiteEquipmentAsync(int siteId, int[] addEquipmentIds, int[] removeEquipmentIds, int actorUserId);
    }
}
