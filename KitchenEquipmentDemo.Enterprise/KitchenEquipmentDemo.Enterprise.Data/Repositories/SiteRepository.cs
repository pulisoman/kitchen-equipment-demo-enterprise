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

        public Task<Site> GetByIdAsync(int id)
        {
            var q =
                from s in _set
                where s.SiteId == id
                select s;
            return q.FirstOrDefaultAsync();
        }

        /// <summary>All non-deleted sites for a user.</summary>
        public Task<List<Site>> GetForUserAsync(int userId)
        {
            var q =
                from s in _set.AsNoTracking()
                where s.IsDeleted == false && s.UserId == userId
                orderby s.Name
                select s;
            return q.ToListAsync();
        }

        /// <summary>Find one by Code (global), excluding deleted.</summary>
        public Task<Site> FindByCodeAsync(string code)
        {
            var q =
                from s in _set
                where s.IsDeleted == false
                   && s.Code == code
                select s;
            return q.FirstOrDefaultAsync();
        }

        /// <summary>Check if Code exists globally (IsDeleted=false).</summary>
        public Task<bool> CodeExistsGlobalAsync(string code)
        {
            var q =
                from s in _set
                where s.IsDeleted == false
                   && s.Code == code
                select s.SiteId;
            return q.AnyAsync();
        }

        /// <summary>Check if Code exists globally, excluding a specific site.</summary>
        public Task<bool> CodeExistsGlobalExceptAsync(string code, int excludeSiteId)
        {
            var q =
                from s in _set
                where s.IsDeleted == false
                   && s.Code == code
                   && s.SiteId != excludeSiteId
                select s.SiteId;
            return q.AnyAsync();
        }

        /// <summary>Name unique per user (IsDeleted=false).</summary>
        public Task<bool> NameExistsAsync(int userId, string name)
        {
            var q =
                from s in _set
                where s.IsDeleted == false
                   && s.UserId == userId
                   && s.Name == name
                select s.SiteId;
            return q.AnyAsync();
        }

        /// <summary>Name unique per user, excluding a specific site.</summary>
        public Task<bool> NameExistsExceptAsync(int? userId, string name, int excludeSiteId)
        {
            var q =
                from s in _set
                where s.IsDeleted == false
                   && s.UserId == userId
                   && s.Name == name
                   && s.SiteId != excludeSiteId
                select s.SiteId;
            return q.AnyAsync();
        }

        public override Task AddAsync(Site entity)
        {
            _set.Add(entity);
            return Task.CompletedTask;
        }

        public override void Update(Site entity)
        {
            _db.Entry(entity).State = EntityState.Modified;
        }
    }
}
