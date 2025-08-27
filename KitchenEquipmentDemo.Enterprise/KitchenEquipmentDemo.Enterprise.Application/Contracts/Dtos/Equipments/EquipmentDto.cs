using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;

namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Equipments
{
    public class EquipmentDto
    {
        public int EquipmentId { get; set; }
        public int? UserId { get; set; }
        public int? SiteId { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string SerialNumber { get; set; }
        public string Description { get; set; }
        public EquipmentCondition Condition { get; set; }
        public string Action { get; set; }
        public string ScreenName { get; set; }
    }
}
