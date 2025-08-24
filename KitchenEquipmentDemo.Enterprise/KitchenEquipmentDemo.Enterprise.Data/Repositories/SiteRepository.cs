// KitchenEquipmentDemo.Enterprise.Data/Repositories/SiteRepository.cs
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Data.Context;
using KitchenEquipmentDemo.Enterprise.Data.Models;

namespace KitchenEquipmentDemo.Enterprise.Data.Repositories
{
    public class SiteRepository : Repository<Site>
    {
        public SiteRepository(AppDbContext db) : base(db) { }

        /// <summary>All non-deleted sites for a user.</summary>
        public async Task<List<Site>> GetForUserAsync(int userId)
        {
            IQueryable<Site> baseQuery = _set.AsNoTracking();

            var query =
                from s in baseQuery
                where s.IsDeleted == false
                   && s.UserId == userId
                orderby s.SiteId
                select s;

            var list = await query.ToListAsync();
            return list;
        }

        /// <summary>Selectable sites (Active=true, not deleted) for a user.</summary>
        public async Task<List<Site>> GetSelectableForUserAsync(int userId)
        {
            IQueryable<Site> baseQuery = _set.AsNoTracking();

            var query =
                from s in baseQuery
                where s.IsDeleted == false
                   && s.Active == true
                   && s.UserId == userId
                orderby s.Name
                select s;

            var list = await query.ToListAsync();
            return list;
        }

        /// <summary>Active, non-deleted sites for a user.</summary>
        public async Task<List<Site>> GetActiveForUserAsync(int userId)
        {
            IQueryable<Site> baseQuery = _set.AsNoTracking();

            var query =
                from s in baseQuery
                where s.IsDeleted == false
                   && s.Active == true
                   && s.UserId == userId
                orderby s.SiteId
                select s;

            var list = await query.ToListAsync();
            return list;
        }

        /// <summary>Find one by Code (per user), excluding deleted.</summary>
        public async Task<Site> FindByCodeAsync(int userId, string code)
        {
            IQueryable<Site> baseQuery = _set.AsNoTracking();

            var query =
                from s in baseQuery
                where s.IsDeleted == false
                   && s.UserId == userId
                   && s.Code == code
                select s;

            var entity = await query.FirstOrDefaultAsync();
            return entity;
        }

        // ---------- Uniqueness pre-checks (per user, IsDeleted = false) ----------

        public async Task<bool> CodeExistsAsync(int userId, string code)
        {
            IQueryable<Site> baseQuery = _set;

            var query =
                from s in baseQuery
                where s.IsDeleted == false
                   && s.UserId == userId
                   && s.Code == code
                select s.SiteId;

            var exists = await query.AnyAsync();
            return exists;
        }

        public async Task<bool> CodeExistsExceptAsync(int userId, string code, int excludeSiteId)
        {
            IQueryable<Site> baseQuery = _set;

            var query =
                from s in baseQuery
                where s.IsDeleted == false
                   && s.UserId == userId
                   && s.Code == code
                   && s.SiteId != excludeSiteId
                select s.SiteId;

            var exists = await query.AnyAsync();
            return exists;
        }

        public async Task<bool> NameExistsAsync(int userId, string name)
        {
            IQueryable<Site> baseQuery = _set;

            var query =
                from s in baseQuery
                where s.IsDeleted == false
                   && s.UserId == userId
                   && s.Name == name
                select s.SiteId;

            var exists = await query.AnyAsync();
            return exists;
        }

        public async Task<bool> NameExistsExceptAsync(int userId, string name, int excludeSiteId)
        {
            IQueryable<Site> baseQuery = _set;

            var query =
                from s in baseQuery
                where s.IsDeleted == false
                   && s.UserId == userId
                   && s.Name == name
                   && s.SiteId != excludeSiteId
                select s.SiteId;

            var exists = await query.AnyAsync();
            return exists;
        }
    }
}
