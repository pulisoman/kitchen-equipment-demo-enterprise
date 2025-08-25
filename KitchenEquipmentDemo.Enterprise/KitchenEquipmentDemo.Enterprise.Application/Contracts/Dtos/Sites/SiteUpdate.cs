namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Sites
{
    public class SiteUpdateDto
    {
        public int SiteId { get; set; }
        public string Code { get; set; }        // keep if you allow code edits; else ignore in service
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        // Remove if you skip concurrency
        public byte[] RowVersion { get; set; }
    }
}
