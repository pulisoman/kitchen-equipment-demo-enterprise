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


        //create UserInfoExistsAsync method that will check uniqueness of username and email excluding a specific userId

        public Task<bool> UserInfoExistsAsync(string userName, string email, int excludeUserId)
        {
            var q =
                from u in _set
                where u.IsDeleted == false
                   && (u.UserName == userName || u.EmailAddress == email)
                   && u.UserId != excludeUserId
                select u.UserId;
            return q.AnyAsync();
        }

        //create for each email and username exists method exclyuding a specific userId

        public Task<bool> UserNameExistsAsync(string userName, int excludeUserId)
        {
            var q =
                from u in _set
                where u.IsDeleted == false
                   && u.UserName == userName
                   && u.UserId != excludeUserId
                select u.UserId;
            return q.AnyAsync();
        }

        public Task<bool> EmailExistsAsync(string email, int excludeUserId)
        {
            var q =
                from u in _set
                where u.IsDeleted == false
                   && u.EmailAddress == email
                   && u.UserId != excludeUserId
                select u.UserId;
            return q.AnyAsync();
        }

        public override Task AddAsync(User entity)
        {
            _set.Add(entity);
            return Task.CompletedTask;
        }

        public override void Update(User entity)
        {
            _db.Entry(entity).State = EntityState.Modified;
        }
    }
}
