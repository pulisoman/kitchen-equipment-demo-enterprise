using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Users;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;

namespace KitchenEquipmentDemo.Enterprise.Tests.Unit
{
    [TestClass]
    public class UserServiceTests : TestBase
    {
        [TestMethod]
        public async Task Admin_Cannot_List_Get_Update_Delete_Users()
        {
            var adminId = CreateTestUser(role: "Admin");
            var list = await UserSvc.ListAsync(page: 1, pageSize: 10, actorUserId: adminId);
            Assert.AreEqual(0, list.Total, "Admin should not see user list.");

            var get = await UserSvc.GetAsync(adminId, actorUserId: adminId);
            Assert.IsFalse(get.Succeeded);

            var upd = await UserSvc.UpdateAsync(new UserDto { UserId = adminId, FirstName = "X" }, actorUserId: adminId);
            Assert.IsFalse(upd.Succeeded);

            var del = await UserSvc.DeleteAsync(adminId, actorUserId: adminId);
            Assert.IsFalse(del.Succeeded);
        }

        [TestMethod]
        public async Task SuperAdmin_Can_List_And_Update_User_But_Not_Delete_SuperAdmin_Or_Self()
        {
            var superId = CreateTestUser(role: "SuperAdmin");
            var adminId = CreateTestUser(role: "Admin");

            var list = await UserSvc.ListAsync(page: 1, pageSize: 50, actorUserId: superId);
            Assert.IsTrue(list.Total >= 2);

            var upd = await UserSvc.UpdateAsync(new UserDto
            {
                UserId = adminId,
                FirstName = "Updated",
                LastName = "User",
                EmailAddress = "updated@example.local",
                UserName = "updated_user",
                UserType = UserType.Admin
            }, actorUserId: superId);
            Assert.IsTrue(upd.Succeeded, string.Join("; ", upd.Errors));

            // cannot delete SuperAdmin
            var delSuper = await UserSvc.DeleteAsync(superId, actorUserId: superId);
            Assert.IsFalse(delSuper.Succeeded);

            // create another SuperAdmin and try deleting as superId
            var anotherSuper = CreateTestUser(role: "SuperAdmin");
            var delOtherSuper = await UserSvc.DeleteAsync(anotherSuper, actorUserId: superId);
            Assert.IsFalse(delOtherSuper.Succeeded, "Should not delete a SuperAdmin.");
        }
    }
}
