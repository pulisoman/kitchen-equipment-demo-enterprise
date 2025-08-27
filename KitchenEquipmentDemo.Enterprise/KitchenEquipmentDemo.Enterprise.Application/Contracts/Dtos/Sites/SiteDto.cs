using System;

namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Sites
{
    public class SiteDto
    {
        public int SiteId { get; set; }
        public int? UserId { get; set; }
        public string Code { get; set; }          // globally unique (per our rule)
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public DateTime? CreatedAt { get; set; }  // for UI
        public DateTime? UpdatedAt { get; set; }  
    }
}
