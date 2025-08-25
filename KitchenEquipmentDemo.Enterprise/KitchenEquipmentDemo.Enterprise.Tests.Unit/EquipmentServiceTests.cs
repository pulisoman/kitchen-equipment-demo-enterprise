using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Equipments;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Sites;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;

namespace KitchenEquipmentDemo.Enterprise.Tests.Unit
{
    [TestClass]
    public class EquipmentServiceTests : TestBase
    {
        [TestMethod]
        public async Task Equipment_Create_Update_Delete_Works_In_Transaction()
        {
            var userId = CreateTestUser();
            // Create a site to attach equipment (optional)
            var siteRes = await SiteSvc.CreateAsync(new SiteCreateDto
            {
                UserId = userId,
                Code = "SITE_" + RandToken("E"),
                Name = "Equip Site " + RandToken("N"),
                Description = "Test Site ",
                Active = true
            }, actorUserId: userId);
            Assert.IsTrue(siteRes.Succeeded, string.Join("; ", siteRes.Errors));

            // Create equipment
            var create = await EquipmentSvc.CreateAsync(new EquipmentCreateDto
            {
                UserId = userId,
                SiteId = siteRes.Data.SiteId,
                SerialNumber = "SN_" + RandToken("EQ"),
                Name = "Equip Name " + RandToken("N"),
                Description = "Test equipment",
                Condition = EquipmentCondition.Working
            }, actorUserId: userId);
            Assert.IsTrue(create.Succeeded, string.Join("; ", create.Errors));

            // Update: move off site
            var upd = await EquipmentSvc.UpdateAsync(new EquipmentUpdateDto
            {
                EquipmentId = create.Data.EquipmentId,
                SiteId = null,
                SerialNumber = create.Data.SerialNumber,
                Description = "Updated",
                Condition = EquipmentCondition.NotWorking
            }, actorUserId: userId);
            Assert.IsTrue(upd.Succeeded, string.Join("; ", upd.Errors));
            Assert.IsNull(upd.Data.SiteId);
            Assert.AreEqual(EquipmentCondition.NotWorking, upd.Data.Condition);

            // Delete (soft)
            var del = await EquipmentSvc.DeleteAsync(create.Data.EquipmentId, actorUserId: userId);
            Assert.IsTrue(del.Succeeded, string.Join("; ", del.Errors));
        }

        [TestMethod]
        public async Task Equipment_List_Returns_Items()
        {
            var userId = CreateTestUser();
            // seed 2 equipments
            for (int i = 0; i < 2; i++)
            {
                var res = await EquipmentSvc.CreateAsync(new EquipmentCreateDto
                {
                    UserId = userId,
                    SerialNumber = "SN_" + RandToken("S"),
                    Description = "Seed",
                    Condition = EquipmentCondition.Working
                }, actorUserId: userId);
                Assert.IsTrue(res.Succeeded, string.Join("; ", res.Errors));
            }

            var page = await EquipmentSvc.ListAsync(userId, page: 1, pageSize: 50);
            Assert.IsTrue(page.Total >= 2);
            Assert.IsTrue(page.Items.Count >= 2);
        }
    }
}
