using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Data.Context;
using KitchenEquipmentDemo.Enterprise.Data.Models;

namespace KitchenEquipmentDemo.Enterprise.Data.Repositories
{
    public class EquipmentRepository : Repository<Equipment>
    {
        public EquipmentRepository(AppDbContext db) : base(db) { }

        public Task<Equipment> GetByIdAsync(int id)
        {
            var q =
                from e in _set
                where e.EquipmentId == id
                select e;
            return q.FirstOrDefaultAsync();
        }

        public Task<List<Equipment>> GetForUserAsync(int userId)
        {
            var q =
                from e in _set.AsNoTracking()
                where e.IsDeleted == false && e.UserId == userId
                orderby e.EquipmentId
                select e;
            return q.ToListAsync();
        }

        public Task<List<Equipment>> GetBySiteAsync(int siteId)
        {
            var q =
                from e in _set
                where e.IsDeleted == false && e.SiteId == siteId
                orderby e.EquipmentId
                select e;
            return q.ToListAsync();
        }

        /// <summary>
        /// Check if a serial number exists for a given user among non-deleted equipment.
        /// </summary>
        public Task<bool> SerialExistsAsync(string serialNumber)
        {
            var q =
                from e in _set
                where e.IsDeleted == false
                   && e.SerialNumber == serialNumber
                select e.EquipmentId;
            return q.AnyAsync();
        }

        public Task<bool> SerialExistsForUserAsync(int userId, string serial, int? excludeEquipmentId = null)
        {
            if (excludeEquipmentId.HasValue)
            {
                var excludeId = excludeEquipmentId.Value;

                var q =
                    from x in _set
                    where x.IsDeleted == false
                       && x.UserId == userId
                       && x.SerialNumber == serial
                       && x.EquipmentId != excludeId
                    select x.EquipmentId;

                return q.AnyAsync();
            }
            else
            {
                var q =
                    from x in _set
                    where x.IsDeleted == false
                       && x.UserId == userId
                       && x.SerialNumber == serial
                    select x.EquipmentId;

                return q.AnyAsync();
            }
        }

        public Task AddAsync(Equipment entity)
        {
            _set.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(Equipment entity)
        {
            _db.Entry(entity).State = EntityState.Modified;
        }
    }
}
