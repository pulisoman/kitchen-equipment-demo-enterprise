using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;

namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Equipments
{
    public class EquipmentUpdateDto
    {
        public int EquipmentId { get; set; }
        public int? SiteId { get; set; }
        public string SerialNumber { get; set; }
        public string Description { get; set; }
        public EquipmentCondition Condition { get; set; }
        // Remove if you skip concurrency
        public byte[] RowVersion { get; set; }
    }
}
