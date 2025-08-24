using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Data.Context;
using KitchenEquipmentDemo.Enterprise.Data.Models;

namespace KitchenEquipmentDemo.Enterprise.Data.Repositories
{
    public class SiteEquipmentHistoryRepository : Repository<SiteEquipmentHistory>
    {
        public SiteEquipmentHistoryRepository(AppDbContext db) : base(db) { }

        public async Task<List<SiteEquipmentHistory>> GetForEquipmentAsync(int equipmentId)
        {
            IQueryable<SiteEquipmentHistory> baseQuery = _set.AsNoTracking();

            var query =
                from h in baseQuery
                where h.EquipmentId == equipmentId
                orderby h.ActionAt descending
                select h;

            var list = await query.ToListAsync();
            return list;
        }

        public async Task<List<SiteEquipmentHistory>> GetForEquipmentWithSiteAsync(int equipmentId)
        {
            IQueryable<SiteEquipmentHistory> baseQuery = _set.AsNoTracking().Include("Site");

            var query =
                from h in baseQuery
                where h.EquipmentId == equipmentId
                orderby h.ActionAt descending
                select h;

            var list = await query.ToListAsync();
            return list;
        }

        public async Task<int> CountForEquipmentAsync(int equipmentId)
        {
            IQueryable<SiteEquipmentHistory> baseQuery = _set;

            var query =
                from h in baseQuery
                where h.EquipmentId == equipmentId
                select h.Id;

            var count = await query.CountAsync();
            return count;
        }

        public Task AddRegisterAsync(int equipmentId, int? siteId, int? actorUserId)
        {
            var row = new SiteEquipmentHistory
            {
                EquipmentId = equipmentId,
                SiteId = siteId,
                Action = "Register",
                ActionAt = DateTime.UtcNow,
                ActionBy = actorUserId
            };

            return AddAsync(row);
        }

        public Task AddUnregisterAsync(int equipmentId, int? siteId, int? actorUserId)
        {
            var row = new SiteEquipmentHistory
            {
                EquipmentId = equipmentId,
                SiteId = siteId,
                Action = "Unregister",
                ActionAt = DateTime.UtcNow,
                ActionBy = actorUserId
            };

            return AddAsync(row);
        }
    }
}
