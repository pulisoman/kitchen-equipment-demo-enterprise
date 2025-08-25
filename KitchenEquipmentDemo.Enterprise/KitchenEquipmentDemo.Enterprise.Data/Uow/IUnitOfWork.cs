using System;
using System.Threading.Tasks;
using System.Data.Entity;

namespace KitchenEquipmentDemo.Enterprise.Data.Uow
{
    public interface IUnitOfWork : IDisposable
    {
        int SaveChanges();
        Task<int> SaveChangesAsync();
        DbContextTransaction BeginTransaction();
        Task ExecuteInTransactionAsync(Func<Task> action);
    }
}
