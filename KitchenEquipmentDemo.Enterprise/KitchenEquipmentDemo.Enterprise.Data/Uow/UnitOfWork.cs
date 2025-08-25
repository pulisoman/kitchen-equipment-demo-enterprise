using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Data.Context;

namespace KitchenEquipmentDemo.Enterprise.Data.Uow
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;
        private bool _disposed;

        public UnitOfWork(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public int SaveChanges() => _db.SaveChanges();

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var details = ex.Message;
                throw new Exception("Validation failed: " + details, ex);
            }
            //catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            //{
            //    var details = string.Join("; ",
            //        ex.EntityValidationErrors.SelectMany(e =>
            //            e.ValidationErrors.Select(v =>
            //                $"{e.Entry.Entity.GetType().Name}.{v.PropertyName}: {v.ErrorMessage} (Value: {e.Entry.CurrentValues[v.PropertyName]})")));
            //    throw new System.Data.Entity.Validation.DbEntityValidationException("Validation failed: " + details, ex);
            //}
        }


        public DbContextTransaction BeginTransaction()
            => _db.Database.BeginTransaction();

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            using (var tx = _db.Database.BeginTransaction())
            {
                try
                {
                    await action();
                    await _db.SaveChangesAsync();
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _db.Dispose();
            _disposed = true;
        }
    }
}
