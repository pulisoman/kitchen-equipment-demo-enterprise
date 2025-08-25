namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Sites
{
    public class SiteCreateDto
    {
        public int UserId { get; set; }
        public string Code { get; set; }     // required
        public string Name { get; set; }     // required
        public string Description { get; set; }
        public bool Active { get; set; } = true;
    }
}
