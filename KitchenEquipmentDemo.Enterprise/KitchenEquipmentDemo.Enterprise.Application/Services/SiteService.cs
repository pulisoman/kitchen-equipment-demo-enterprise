using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Sites;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.Data.Repositories;
using KitchenEquipmentDemo.Enterprise.Data.Uow;
using KitchenEquipmentDemo.Enterprise.Data.Models;

namespace KitchenEquipmentDemo.Enterprise.Application.Services
{
    public class SiteService : ISiteService
    {
        private readonly SiteRepository _siteRepo;
        private readonly EquipmentRepository _equipmentRepo;
        private readonly SiteEquipmentHistoryRepository _historyRepo;
        private readonly UnitOfWork _uow;

        public SiteService(
            SiteRepository siteRepo,
            EquipmentRepository equipmentRepo,
            SiteEquipmentHistoryRepository historyRepo,
            UnitOfWork uow)
        {
            _siteRepo = siteRepo;
            _equipmentRepo = equipmentRepo;
            _historyRepo = historyRepo;
            _uow = uow;
        }

        public async Task<PagedResult<SiteDto>> ListAsync(int userId, int page, int pageSize, string search = null, bool includeInactive = false)
        {
            var all = await _siteRepo.GetForUserAsync(userId); // returns IsDeleted=false
            IEnumerable<Site> filtered = all;

            if (!includeInactive)
                filtered = filtered.Where(s => s.Active);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                filtered = filtered.Where(x =>
                    (x.Name ?? "").IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (x.Code ?? "").IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (x.Description ?? "").IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var total = filtered.Count();
            var items = filtered
                .OrderBy(x => x.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(Map)
                .ToList();

            return new PagedResult<SiteDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }

        public async Task<OperationResult<SiteDto>> GetAsync(int userId, int siteId)
        {
            var site = await _siteRepo.GetByIdAsync(siteId);
            if (site == null || site.IsDeleted || site.UserId != userId)
                return OperationResult<SiteDto>.Fail("Site not found.");

            return OperationResult<SiteDto>.Success(Map(site));
        }

        public async Task<OperationResult<SiteDto>> FindByCodeAsync(string code)
        {
            var norm = NormalizeCode(code);
            var site = await _siteRepo.FindByCodeAsync(norm); // global unique
            if (site == null || site.IsDeleted)
                return OperationResult<SiteDto>.Fail("Site not found.");

            return OperationResult<SiteDto>.Success(Map(site));
        }

        public async Task<OperationResult<SiteDto>> CreateAsync(SiteCreateDto dto, int actorUserId)
        {
            if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
                return OperationResult<SiteDto>.Fail("Code and Name are required.");

            var code = NormalizeCode(dto.Code);

            // Global unique Code; Name unique per user (UX)
            if (await _siteRepo.CodeExistsGlobalAsync(code))
                return OperationResult<SiteDto>.Fail("Code already exists.");
            if (await _siteRepo.NameExistsAsync(dto.UserId, dto.Name.Trim()))
                return OperationResult<SiteDto>.Fail("Name already exists for this user.");

            var entity = new Site
            {
                UserId = dto.UserId,
                Code = code,
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                Active = dto.Active,
                IsDeleted = false,
                CreatedBy = actorUserId
            };

            await _siteRepo.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return OperationResult<SiteDto>.Success(Map(entity), "Site created.");
        }

        public async Task<OperationResult<SiteDto>> UpdateAsync(SiteUpdateDto dto, int actorUserId)
        {
            var site = await _siteRepo.GetByIdAsync(dto.SiteId);
            if (site == null || site.IsDeleted)
                return OperationResult<SiteDto>.Fail("Site not found.");

            // Optional: keep Code immutable (recommended). If you allow change, check global uniqueness.
            if (!string.IsNullOrWhiteSpace(dto.Code) && !string.Equals(site.Code, NormalizeCode(dto.Code), StringComparison.Ordinal))
            {
                var newCode = NormalizeCode(dto.Code);
                var exists = await _siteRepo.CodeExistsGlobalExceptAsync(newCode, site.SiteId);
                if (exists) return OperationResult<SiteDto>.Fail("Code already exists.");
                site.Code = newCode;
            }

            if (!string.Equals(site.Name, dto.Name?.Trim(), StringComparison.Ordinal))
            {
                if (await _siteRepo.NameExistsExceptAsync(site.UserId, dto.Name.Trim(), site.SiteId))
                    return OperationResult<SiteDto>.Fail("Name already exists for this user.");
                site.Name = dto.Name.Trim();
            }

            site.Description = dto.Description?.Trim();
            site.Active = dto.Active;
            site.UpdatedBy = actorUserId;
            site.UpdatedAt = DateTime.UtcNow;

            _siteRepo.Update(site);
            await _uow.SaveChangesAsync();
            return OperationResult<SiteDto>.Success(Map(site), "Site updated.");
        }

        public async Task<OperationResult> DeleteAsync(int siteId, int actorUserId)
        {
            var site = await _siteRepo.GetByIdAsync(siteId);
            if (site == null || site.IsDeleted)
                return OperationResult.Success("Already deleted.");

            // Unregister all equipment under this site, write history
            // Expected repo method: GetBySiteAsync(int siteId) returning non-deleted equipment
            var equipments = await _equipmentRepo.GetBySiteAsync(siteId);

            foreach (var e in equipments)
            {
                // add history: Unregister
                var hist = new SiteEquipmentHistory
                {
                    EquipmentId = e.EquipmentId,
                    SiteId = site.SiteId,
                    Action = "Unregister",
                    ActionBy = actorUserId,
                    // action_at has default SYSUTCDATETIME()
                };
                await _historyRepo.AddAsync(hist);

                e.SiteId = null;
                e.UpdatedBy = actorUserId;
                e.UpdatedAt = DateTime.UtcNow;
                _equipmentRepo.Update(e);
            }

            site.IsDeleted = true;
            site.UpdatedBy = actorUserId;
            site.UpdatedAt = DateTime.UtcNow;
            _siteRepo.Update(site);

            await _uow.SaveChangesAsync();
            return OperationResult.Success("Site deleted and equipment unregistered.");
        }

        public async Task<OperationResult> EditSiteEquipmentAsync(int siteId, int[] addEquipmentIds, int[] removeEquipmentIds, int actorUserId)
        {
            var site = await _siteRepo.GetByIdAsync(siteId);
            if (site == null || site.IsDeleted)
                return OperationResult.Fail("Site not found.");

            addEquipmentIds = addEquipmentIds ?? Array.Empty<int>();
            removeEquipmentIds = removeEquipmentIds ?? Array.Empty<int>();

            // ADD: register equipment to this site
            foreach (var id in addEquipmentIds.Distinct())
            {
                var eq = await _equipmentRepo.GetByIdAsync(id);
                if (eq == null || eq.IsDeleted) continue;
                if (eq.UserId != site.UserId) continue; // enforce ownership rule

                if (eq.SiteId != site.SiteId)
                {
                    eq.SiteId = site.SiteId;
                    eq.UpdatedBy = actorUserId;
                    eq.UpdatedAt = DateTime.UtcNow;
                    _equipmentRepo.Update(eq);

                    await _historyRepo.AddAsync(new SiteEquipmentHistory
                    {
                        EquipmentId = eq.EquipmentId,
                        SiteId = site.SiteId,
                        Action = "Register",
                        ActionBy = actorUserId
                    });
                }
            }

            // REMOVE: unregister equipment from this site
            foreach (var id in removeEquipmentIds.Distinct())
            {
                var eq = await _equipmentRepo.GetByIdAsync(id);
                if (eq == null || eq.IsDeleted) continue;
                if (eq.SiteId == site.SiteId)
                {
                    eq.SiteId = null;
                    eq.UpdatedBy = actorUserId;
                    eq.UpdatedAt = DateTime.UtcNow;
                    _equipmentRepo.Update(eq);

                    await _historyRepo.AddAsync(new SiteEquipmentHistory
                    {
                        EquipmentId = eq.EquipmentId,
                        SiteId = site.SiteId,
                        Action = "Unregister",
                        ActionBy = actorUserId
                    });
                }
            }

            await _uow.SaveChangesAsync();
            return OperationResult.Success("Site equipment updated.");
        }

        // ---- helpers ----
        private static string NormalizeCode(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            var s = raw.Trim().ToUpperInvariant();
            s = s.Replace(' ', '-').Replace('_', '-');
            return s;
        }

        private static SiteDto Map(Site s) => new SiteDto
        {
            SiteId = s.SiteId,
            UserId = s.UserId,
            Code = s.Code,
            Name = s.Name,
            Description = s.Description,
            Active = s.Active,
            UpdatedAt = s.UpdatedAt
        };
    }
}
