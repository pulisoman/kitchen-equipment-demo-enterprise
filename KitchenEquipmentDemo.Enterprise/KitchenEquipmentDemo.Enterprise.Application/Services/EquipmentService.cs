using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Equipments;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.Data.Repositories;
using KitchenEquipmentDemo.Enterprise.Data.Uow;
using KitchenEquipmentDemo.Enterprise.Data.Models;
using System;

namespace KitchenEquipmentDemo.Enterprise.Application.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly EquipmentRepository _equipmentRepo;
        private readonly UserRepository _userRepo;
        private readonly SiteRepository _siteRepo;
        private readonly SiteEquipmentHistoryRepository _historyRepo;
        private readonly IUnitOfWork _uow;

        public EquipmentService(
            EquipmentRepository equipmentRepo,
            UserRepository userRepo,
            SiteRepository siteRepo,
            SiteEquipmentHistoryRepository historyRepo,
            IUnitOfWork uow)
        {
            _equipmentRepo = equipmentRepo;
            _userRepo = userRepo;
            _siteRepo = siteRepo;
            _historyRepo = historyRepo;
            _uow = uow;
        }

        public async Task<PagedResult<EquipmentDto>> ListAsync(int userId, int page, int pageSize, string search = null)
        {
            // Expected repo: GetForUserAsync(userId) returning IsDeleted=false
            var all = await _equipmentRepo.GetForUserAsync(userId);
            IEnumerable<Equipment> filtered = all;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                filtered = filtered.Where(x =>
                    (x.SerialNumber ?? "").IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (x.Description ?? "").IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var total = filtered.Count();
            var items = filtered
                .OrderBy(x => x.EquipmentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(Map)
                .ToList();

            return new PagedResult<EquipmentDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }

        public async Task<OperationResult<EquipmentDto>> GetAsync(int equipmentId)
        {
            var e = await _equipmentRepo.GetByIdAsync(equipmentId);
            if (e == null || e.IsDeleted)
                return OperationResult<EquipmentDto>.Fail("Equipment not found.");
            return OperationResult<EquipmentDto>.Success(Map(e));
        }

        public async Task<OperationResult<EquipmentDto>> CreateAsync(EquipmentCreateDto dto, int actorUserId)
        {
            if (string.IsNullOrWhiteSpace(dto.SerialNumber))
                return OperationResult<EquipmentDto>.Fail("Serial number is required.");

            // Recommended uniqueness: (UserId, SerialNumber) filtered on IsDeleted=0
            if (await _equipmentRepo.SerialExistsAsync(dto.SerialNumber.Trim()))
                return OperationResult<EquipmentDto>.Fail("Serial number already exists.");

            // If SiteId provided, ensure site exists and belongs to same user
            int? siteId = dto.SiteId;
            if (siteId.HasValue)
            {
                var site = await _siteRepo.GetByIdAsync(siteId.Value);
                if (site == null || site.IsDeleted) return OperationResult<EquipmentDto>.Fail("Site not found.");
                if (site.UserId != dto.UserId) return OperationResult<EquipmentDto>.Fail("Site belongs to a different user.");
            }

            var entity = new Equipment
            {
                UserId = dto.UserId,
                SiteId = dto.SiteId,
                SerialNumber = dto.SerialNumber.Trim(),
                Description = dto.Description ?? string.Empty?.Trim(),
                Condition = dto.Condition == EquipmentCondition.Working ? "Working" : "Not Working",
                IsDeleted = false,
                CreatedBy = actorUserId
            };

            await _equipmentRepo.AddAsync(entity);

            // Write history if assigned to a site
            if (entity.SiteId.HasValue)
            {
                await _historyRepo.AddAsync(new SiteEquipmentHistory
                {
                    EquipmentId = entity.EquipmentId, // EF will populate after SaveChanges
                    SiteId = entity.SiteId.Value,
                    Action = "Register",
                ActionAt = DateTime.UtcNow,
                    ActionBy = actorUserId
                });
            }

            await _uow.SaveChangesAsync();
            return OperationResult<EquipmentDto>.Success(Map(entity), "Equipment created.");
        }

        public async Task<OperationResult<EquipmentDto>> UpdateAsync(EquipmentUpdateDto dto, int actorUserId)
        {
            var e = await _equipmentRepo.GetByIdAsync(dto.EquipmentId);
            if (e == null || e.IsDeleted)
                return OperationResult<EquipmentDto>.Fail("Equipment not found.");

            // If moving between sites, record history
            int? oldSiteId = e.SiteId;
            int? newSiteId = dto.SiteId;

            Site site = null;
            if (newSiteId.HasValue)
            {
                site = await _siteRepo.GetByIdAsync(newSiteId.Value);
                if (site == null || site.IsDeleted)
                    return OperationResult<EquipmentDto>.Fail("Site not found.");

                if (site.UserId != e.UserId)
                {
                    // Cross-owner move → only SuperAdmin can do this
                    var actor = await _userRepo.GetByIdAsync(actorUserId);
                    if (!string.Equals(actor?.UserType, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                        return OperationResult<EquipmentDto>.Fail("Site belongs to a different user.");

                    // Ensure serial remains unique for the TARGET user
                    var serial = (dto.SerialNumber ?? string.Empty).Trim();
                    var conflict = await _equipmentRepo.SerialExistsForUserAsync(
                        site.UserId, serial, excludeEquipmentId: e.EquipmentId);
                    if (conflict)
                        return OperationResult<EquipmentDto>.Fail("Serial number already exists for the target user.");

                    // Transfer ownership to the target site's user
                    e.UserId = site.UserId;
                }
            }

            e.SiteId = newSiteId;
            e.SerialNumber = dto.SerialNumber?.Trim();
            e.Description = dto.Description ?? string.Empty?.Trim();
            e.Condition = dto.Condition == EquipmentCondition.Working ? "Working" : "Not Working";
            e.UpdatedBy = actorUserId;
            e.UpdatedAt = DateTime.UtcNow;
            _equipmentRepo.Update(e);

            // History entries
            if (oldSiteId != newSiteId)
            {
                if (oldSiteId.HasValue)
                {
                    await _historyRepo.AddAsync(new SiteEquipmentHistory
                    {
                        EquipmentId = e.EquipmentId,
                        SiteId = oldSiteId,
                        Action = "Unregister",
                ActionAt = DateTime.UtcNow,
                        ActionBy = actorUserId
                    });
                }
                if (newSiteId.HasValue)
                {
                    await _historyRepo.AddAsync(new SiteEquipmentHistory
                    {
                        EquipmentId = e.EquipmentId,
                        SiteId = newSiteId.Value,
                        Action = "Register",
                ActionAt = DateTime.UtcNow,
                        ActionBy = actorUserId
                    });
                }
            }

            await _uow.SaveChangesAsync();
            return OperationResult<EquipmentDto>.Success(Map(e), "Equipment updated.");
        }

        public async Task<OperationResult> DeleteAsync(int equipmentId, int actorUserId)
        {
            var e = await _equipmentRepo.GetByIdAsync(equipmentId);
            if (e == null || e.IsDeleted) return OperationResult.Success("Already deleted.");

            e.IsDeleted = true;
            e.UpdatedBy = actorUserId;
            e.UpdatedAt = DateTime.UtcNow;
            _equipmentRepo.Update(e);

            await _uow.SaveChangesAsync();
            return OperationResult.Success("Equipment deleted.");
        }

        // ---- helpers ----
        private static EquipmentDto Map(Equipment e) => new EquipmentDto
        {
            EquipmentId = e.EquipmentId,
            UserId = e.UserId,
            SiteId = e.SiteId,
            SerialNumber = e.SerialNumber,
            Description = e.Description,
            Condition = e.Condition == "Working" ? EquipmentCondition.Working : EquipmentCondition.NotWorking
        };
    }
}