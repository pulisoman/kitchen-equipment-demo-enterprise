using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Sites;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Equipments;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;

namespace KitchenEquipmentDemo.Enterprise.Tests.Unit
{
    [TestClass]
    public class SiteServiceAdvancedTests : TestBase
    {
        [TestMethod]
        public async Task Site_Code_Is_Global_Unique_Across_Users()
        {
            var userA = CreateTestUser();
            var userB = CreateTestUser();

            var code = "SITE_" + RandToken("G");
            var createA = await SiteSvc.CreateAsync(new SiteCreateDto { UserId = userA, Code = code, Name = "A_" + RandToken("N"), Description = "Test Site",
                Active = true }, actorUserId: userA);
            Assert.IsTrue(createA.Succeeded, string.Join("; ", createA.Errors));

            var createB = await SiteSvc.CreateAsync(new SiteCreateDto { UserId = userB, Code = code, Name = "B_" + RandToken("N"), Description = "Test Site",
                Active = true }, actorUserId: userB);
            Assert.IsFalse(createB.Succeeded, "Expected failure due to global code uniqueness.");
        }

        [TestMethod]
        public async Task Site_Name_Is_Unique_Per_User_Only()
        {
            var userA = CreateTestUser();
            var userB = CreateTestUser();

            var name = "SameName_" + RandToken("NM");

            var s1 = await SiteSvc.CreateAsync(new SiteCreateDto { UserId = userA, Code = "SITE_" + RandToken("A"), Name = name, Description = "Test Site",
                Active = true }, actorUserId: userA);
            Assert.IsTrue(s1.Succeeded, string.Join("; ", s1.Errors));

            var s2 = await SiteSvc.CreateAsync(new SiteCreateDto { UserId = userA, Code = "SITE_" + RandToken("B"), Name = name, Description = "Test Site",
                Active = true }, actorUserId: userA);
            Assert.IsFalse(s2.Succeeded, "Same user should not reuse site name.");

            var s3 = await SiteSvc.CreateAsync(new SiteCreateDto { UserId = userB, Code = "SITE_" + RandToken("C"), Name = name, Description = "Test Site",
                Active = true }, actorUserId: userB);
            Assert.IsTrue(s3.Succeeded, "Different user can use the same site name.");
        }

        [TestMethod]
        public async Task Site_FindByCode_Works()
        {
            var userId = CreateTestUser();
            var code = "SITE_" + RandToken("F");
            var name = "Name_" + RandToken("N");

            var created = await SiteSvc.CreateAsync(new SiteCreateDto { UserId = userId, Code = code, Name = name, Description = "Test Site",
                Active = true }, actorUserId: userId);
            Assert.IsTrue(created.Succeeded);

            var found = await SiteSvc.FindByCodeAsync(code);
            Assert.IsTrue(found.Succeeded);
            Assert.AreEqual(created.Data.SiteId, found.Data.SiteId);
        }

        [TestMethod]
        public async Task EditSiteEquipment_Adds_And_Removes_With_History()
        {
            var userId = CreateTestUser();
            var site = await SiteSvc.CreateAsync(new SiteCreateDto { UserId = userId, Code = "SITE_" + RandToken("E"), Name = "E_" + RandToken("N"), Description = "Test Site",
                Active = true }, actorUserId: userId);
            Assert.IsTrue(site.Succeeded);

            var e1 = await EquipmentSvc.CreateAsync(new EquipmentCreateDto { UserId = userId, SerialNumber = "SN_" + RandToken("1"), Description = "E1", Condition = EquipmentCondition.Working }, actorUserId: userId);
            var e2 = await EquipmentSvc.CreateAsync(new EquipmentCreateDto { UserId = userId, SerialNumber = "SN_" + RandToken("2"), Description = "E2", Condition = EquipmentCondition.Working }, actorUserId: userId);
            Assert.IsTrue(e1.Succeeded && e2.Succeeded);

            var addRes = await SiteSvc.EditSiteEquipmentAsync(site.Data.SiteId, new[] { e1.Data.EquipmentId, e2.Data.EquipmentId }, new int[0], actorUserId: userId);
            Assert.IsTrue(addRes.Succeeded);

            var remRes = await SiteSvc.EditSiteEquipmentAsync(site.Data.SiteId, new int[0], new[] { e1.Data.EquipmentId }, actorUserId: userId);
            Assert.IsTrue(remRes.Succeeded);

            // Check history written (register for 2 items, then unregister for 1)
            var regs = Db.SiteEquipmentHistory.Where(h => h.SiteId == site.Data.SiteId && h.Action == "Register").Count();
            var unregs = Db.SiteEquipmentHistory.Where(h => h.SiteId == site.Data.SiteId && h.Action == "Unregister").Count();
            Assert.AreEqual(2, regs);
            Assert.AreEqual(1, unregs);
        }
    }
}
