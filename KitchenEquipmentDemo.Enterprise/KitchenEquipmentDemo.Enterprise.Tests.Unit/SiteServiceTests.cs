using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Sites;

namespace KitchenEquipmentDemo.Enterprise.Tests.Unit
{
    [TestClass]
    public class SiteServiceTests : TestBase
    {
        [TestMethod]
        public async Task Site_Create_Update_Delete_Works_In_Transaction()
        {
            var userId = CreateTestUser();
            var code = "SITE_" + RandToken("C");
            var name = "Name_" + RandToken("N");

            var createDto = new SiteCreateDto { UserId = userId, Code = code, Name = name, Description = "Desc", Active = true };
            var created = await SiteSvc.CreateAsync(createDto, actorUserId: userId);
            Assert.IsTrue(created.Succeeded, string.Join("; ", created.Errors));

            var upd = await SiteSvc.UpdateAsync(new SiteUpdateDto
            {
                SiteId = created.Data.SiteId,
                Name = name + "_upd",
                Description = "Updated desc",
                Active = false
            }, actorUserId: userId);
            Assert.IsTrue(upd.Succeeded, string.Join("; ", upd.Errors));
            Assert.AreEqual(false, upd.Data.Active);

            var del = await SiteSvc.DeleteAsync(created.Data.SiteId, actorUserId: userId);
            Assert.IsTrue(del.Succeeded, string.Join("; ", del.Errors));

            var get = await SiteSvc.GetAsync(userId, created.Data.SiteId);
            Assert.IsFalse(get.Succeeded);
        }

        [TestMethod]
        public async Task Site_List_Returns_Items()
        {
            var userId = CreateTestUser();
            var res = await SiteSvc.CreateAsync(new SiteCreateDto
            {
                UserId = userId,
                Code = "SITE_" + RandToken("L"),
                Name = "Seed " + RandToken("S"),
                Description = "Test Site",
                Active = true
            }, actorUserId: userId);
            Assert.IsTrue(res.Succeeded, string.Join("; ", res.Errors));

            var list = await SiteSvc.ListAsync(userId, page: 1, pageSize: 20);
            Assert.IsTrue(list.Total >= 1);
            Assert.IsTrue(list.Items.Count >= 1);
        }
    }
}
