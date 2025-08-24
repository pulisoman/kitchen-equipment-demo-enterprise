using System;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core;
using KitchenEquipmentDemo.Enterprise.Data.Context;

namespace KitchenEquipmentDemo.Enterprise.Data.Uow
{
    /// <summary>
    /// Coordinates a set of repository operations against a single AppDbContext.
    /// </summary>
    public class UnitOfWork : IDisposable
    {
        private readonly AppDbContext _db;
        private bool _disposed;

        public UnitOfWork(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public int SaveChanges() => _db.SaveChanges();

        public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

        /// <summary>
        /// Start a database transaction. Use with try/catch and Commit/Rollback.
        /// </summary>
        public DbContextTransaction BeginTransaction()
            => _db.Database.BeginTransaction();

        /// <summary>
        /// Helper to execute a function inside a transaction (auto commit/rollback).
        /// </summary>
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
                catch (Exception)
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
