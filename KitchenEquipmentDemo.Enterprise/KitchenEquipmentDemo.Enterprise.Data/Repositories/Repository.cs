using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Data.Context;

namespace KitchenEquipmentDemo.Enterprise.Data.Repositories
{
    public class Repository<T> where T : class
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<T> _set;

        public Repository(AppDbContext db)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            _db = db;
            _set = _db.Set<T>();
        }

        // READ (single)
        public virtual async Task<T> GetByIdAsync(int id)
        {
            var entity = await _set.FindAsync(id);
            return entity;
        }

        // READ (all)
        public virtual async Task<List<T>> GetAllAsync(bool asNoTracking = true)
        {
            IQueryable<T> query;
            if (asNoTracking)
            {
                query = _set.AsNoTracking();
            }
            else
            {
                query = _set;
            }

            var list = await query.ToListAsync();
            return list;
        }

        // Expose queryable (still no lambdas here; consumers may project if needed)
        public virtual IQueryable<T> QueryAll(bool asNoTracking = true)
        {
            if (asNoTracking)
            {
                return _set.AsNoTracking();
            }

            return _set;
        }

        // CREATE
        public virtual Task AddAsync(T entity)
        {
            _set.Add(entity);
            return Task.CompletedTask;
        }

        // UPDATE
        public virtual void Update(T entity)
        {
            _db.Entry(entity).State = EntityState.Modified;
        }

        // DELETE (physical)
        public virtual void Remove(T entity)
        {
            _set.Remove(entity);
        }

    }
}
