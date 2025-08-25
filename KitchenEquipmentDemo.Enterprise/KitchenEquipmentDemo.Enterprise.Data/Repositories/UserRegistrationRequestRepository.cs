using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Data.Context;
using KitchenEquipmentDemo.Enterprise.Data.Models;

namespace KitchenEquipmentDemo.Enterprise.Data.Repositories
{
    /// <summary>
    /// Repository for UserRegistrationRequest with readable, non-lambda query methods.
    /// </summary>
    public class UserRegistrationRequestRepository : Repository<UserRegistrationRequest>
    {
        public UserRegistrationRequestRepository(AppDbContext db) : base(db) { }

        public Task<UserRegistrationRequest> GetByIdAsync(int id)
        {
            var q =
                from r in _set
                where r.RequestId == id
                select r;
            return q.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns true if there is a Pending request for either the username or email.
        /// </summary>
        public Task<bool> PendingExistsForAsync(string userName, string email)
        {
            var q =
                from r in _set
                where r.Status == "Pending"
                  && (r.UserName == userName || r.EmailAddress == email)
                select r.RequestId;
            return q.AnyAsync();
        }

        /// <summary>
        /// Get the Pending request for a specific username, if any.
        /// </summary>
        public Task<UserRegistrationRequest> GetPendingByUserNameAsync(string userName)
        {
            var q =
                from r in _set
                where r.Status == "Pending"
                  && r.UserName == userName
                select r;
            return q.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get the Pending request for a specific email, if any.
        /// </summary>
        public Task<UserRegistrationRequest> GetPendingByEmailAsync(string email)
        {
            var q =
                from r in _set
                where r.Status == "Pending"
                  && r.EmailAddress == email
                select r;
            return q.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get the first Pending request found (ordered by RequestId).
        /// </summary>
        public Task<UserRegistrationRequest> GetFirstPendingAsync()
        {
            var q =
                from r in _set
                where r.Status == "Pending"
                orderby r.RequestId
                select r;
            return q.FirstOrDefaultAsync();
        }

        public Task AddAsync(UserRegistrationRequest entity)
        {
            _set.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(UserRegistrationRequest entity)
        {
            _db.Entry(entity).State = System.Data.Entity.EntityState.Modified;
        }
    }
}
