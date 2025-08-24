using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Data.Context;
using KitchenEquipmentDemo.Enterprise.Data.Models;

namespace KitchenEquipmentDemo.Enterprise.Data.Repositories
{
    public class UserRepository : Repository<User>
    {
        public UserRepository(AppDbContext db) : base(db) { }

        public async Task<User> FindByUserNameAsync(string userName)
        {
            IQueryable<User> baseQuery = _set.AsNoTracking();

            var query =
                from u in baseQuery
                where u.IsDeleted == false
                   && u.UserName == userName
                select u;

            var entity = await query.FirstOrDefaultAsync();
            return entity;
        }

        public async Task<bool> IsSuperAdminAsync(int userId)
        {
            IQueryable<User> baseQuery = _set;

            var query =
                from u in baseQuery
                where u.IsDeleted == false
                   && u.UserId == userId
                   && u.UserType == "SuperAdmin"
                select u.UserId;

            var isSuper = await query.AnyAsync();
            return isSuper;
        }
    }
}
