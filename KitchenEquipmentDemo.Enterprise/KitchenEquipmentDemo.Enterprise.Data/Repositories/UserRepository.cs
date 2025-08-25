using System.Collections.Generic;
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

        public Task<User> GetByIdAsync(int id)
        {
            var q =
                from u in _set
                where u.UserId == id
                select u;
            return q.FirstOrDefaultAsync();
        }

        public Task<List<User>> GetAllAsync()
        {
            var q =
                from u in _set
                select u;
            return q.ToListAsync();
        }

        public Task<User> FindByUserNameAsync(string userName)
        {
            var q =
                from u in _set
                where u.UserName == userName
                select u;
            return q.FirstOrDefaultAsync();
        }

        public Task<bool> UserNameExistsAsync(string userName)
        {
            var q =
                from u in _set
                where u.IsDeleted == false
                   && u.UserName == userName
                select u.UserId;
            return q.AnyAsync();
        }

        public Task<bool> EmailExistsAsync(string email)
        {
            var q =
                from u in _set
                where u.IsDeleted == false
                   && u.EmailAddress == email
                select u.UserId;
            return q.AnyAsync();
        }

        public Task AddAsync(User entity)
        {
            _set.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(User entity)
        {
            _db.Entry(entity).State = EntityState.Modified;
        }
    }
}
