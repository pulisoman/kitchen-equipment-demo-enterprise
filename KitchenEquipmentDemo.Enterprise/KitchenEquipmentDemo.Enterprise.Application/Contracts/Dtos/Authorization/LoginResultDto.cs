using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;

namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Authorization
{
    public class LoginResultDto
    {
        public int UserId { get; set; }
        public UserType UserType { get; set; }
        public string FullName { get; set; }
        public bool Success { get; set; }
        public string FailureReason { get; set; }
    }
}
