using System.Threading.Tasks;
using KitchenEquipmentDemo.Enterprise.Data.Context;
using KitchenEquipmentDemo.Enterprise.Data.Models;

namespace KitchenEquipmentDemo.Enterprise.Data.Repositories
{
    public class SiteEquipmentHistoryRepository : Repository<SiteEquipmentHistory>
    {
        public SiteEquipmentHistoryRepository(AppDbContext db) : base(db) { }

        public Task AddAsync(SiteEquipmentHistory entity)
        {
            _set.Add(entity);
            return Task.CompletedTask;
        }
    }
}
