using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Data.Context;
using KitchenEquipmentDemo.Enterprise.Data.Models;

namespace KitchenEquipmentDemo.Enterprise.Data.Repositories
{
    public class UserRegistrationRequestRepository : Repository<UserRegistrationRequest>
    {
        public UserRegistrationRequestRepository(AppDbContext db) : base(db) { }

        public async Task<List<UserRegistrationRequest>> GetPendingAsync()
        {
            IQueryable<UserRegistrationRequest> baseQuery = _set.AsNoTracking();

            var query =
                from r in baseQuery
                where r.Status == "Pending"
                orderby r.RequestId
                select r;

            var list = await query.ToListAsync();
            return list;
        }

        public async Task<bool> ExistsByUserNameOrEmailAsync(string userName, string email)
        {
            IQueryable<UserRegistrationRequest> baseQuery = _set;

            var query =
                from r in baseQuery
                where r.UserName == userName || r.EmailAddress == email
                select r.RequestId;

            var exists = await query.AnyAsync();
            return exists;
        }
    }
}
