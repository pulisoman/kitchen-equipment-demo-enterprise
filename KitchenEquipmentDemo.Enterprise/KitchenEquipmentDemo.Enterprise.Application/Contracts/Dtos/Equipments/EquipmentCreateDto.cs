using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;

namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Equipments
{
    public class EquipmentCreateDto
    {
        public int? UserId { get; set; }
        public int? SiteId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public string Description { get; set; }
        public EquipmentCondition Condition { get; set; }
        public int EquipmentId { get; set; }
        public bool Active { get; set; }
    }
}
