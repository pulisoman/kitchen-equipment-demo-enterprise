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

        public async Task<List<Equipment>> GetForUserAsync(int userId)
        {
            IQueryable<Equipment> baseQuery = _set.AsNoTracking();

            var query =
                from e in baseQuery
                where e.IsDeleted == false && e.UserId == userId
                orderby e.EquipmentId
                select e;

            var list = await query.ToListAsync();
            return list;
        }

        public async Task<bool> SerialExistsForUserAsync(int userId, string serial)
        {
            IQueryable<Equipment> baseQuery = _set;

            var query =
                from e in baseQuery
                where e.IsDeleted == false
                   && e.UserId == userId
                   && e.SerialNumber == serial
                select e.EquipmentId;

            var exists = await query.AnyAsync();
            return exists;
        }

        public async Task<Equipment> GetWithSiteAsync(int id)
        {
            IQueryable<Equipment> baseQuery = _set.AsNoTracking().Include("Site");

            var query =
                from e in baseQuery
                where e.EquipmentId == id && e.IsDeleted == false
                select e;

            var entity = await query.FirstOrDefaultAsync();
            return entity;
        }
    }
}
