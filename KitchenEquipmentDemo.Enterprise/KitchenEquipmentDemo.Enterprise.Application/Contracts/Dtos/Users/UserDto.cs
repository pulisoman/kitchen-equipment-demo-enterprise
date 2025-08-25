using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;

namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Users
{
    public class UserDto
    {
        public int UserId { get; set; }
        public UserType UserType { get; set; } // SuperAdmin/Admin
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string UserName { get; set; }
        public bool IsDeleted { get; set; }
    }
}
