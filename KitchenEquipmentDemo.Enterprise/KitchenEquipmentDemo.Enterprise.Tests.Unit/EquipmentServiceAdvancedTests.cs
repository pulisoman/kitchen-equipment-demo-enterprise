using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Equipments;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Sites;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using System.Linq;

namespace KitchenEquipmentDemo.Enterprise.Tests.Unit
{
    [TestClass]
    public class EquipmentServiceAdvancedTests : TestBase
    {
        [TestMethod]
        public async Task Serial_Is_Unique_Per_User()
        {
            var userA = CreateTestUser();
            var userB = CreateTestUser();

            var serial = "SN_" + RandToken("U");

            var a1 = await EquipmentSvc.CreateAsync(new EquipmentCreateDto { UserId = userA, SerialNumber = serial, Name = "Equip Name",
                Description = "A", Condition = EquipmentCondition.Working }, actorUserId: userA);
            Assert.IsTrue(a1.Succeeded, string.Join("; ", a1.Errors));

            var a2 = await EquipmentSvc.CreateAsync(new EquipmentCreateDto { UserId = userA, SerialNumber = serial, Name = "Equip Name",
                Description = "A2", Condition = EquipmentCondition.NotWorking }, actorUserId: userA);
            Assert.IsFalse(a2.Succeeded, "Duplicate serial for same user must fail.");
        }

        [TestMethod]
        public async Task Moving_Equipment_Between_Sites_Writes_History()
        {
            var userId = CreateTestUser();              // owns site s1
            var userId2 = CreateTestUser();              // owns site s2
            var super = CreateTestUser(role: "SuperAdmin"); // actor with privilege

            var s1 = await SiteSvc.CreateAsync(new SiteCreateDto
            {
                UserId = userId,
                Code = "SITE_" + RandToken("S1"),
                Name = "S1_" + RandToken("N"),
                Description = "Site 1 " + RandToken("D"),
                Active = true
            }, actorUserId: userId);
            Assert.IsTrue(s1.Succeeded);

            var s2 = await SiteSvc.CreateAsync(new SiteCreateDto
            {
                UserId = userId2,
                Code = "SITE_" + RandToken("S2"),
                Name = "S2_" + RandToken("N"),
                Description = "Site 2 " + RandToken("D"),
                Active = true
            }, actorUserId: userId2);
            Assert.IsTrue(s2.Succeeded);

            var e = await EquipmentSvc.CreateAsync(new EquipmentCreateDto
            {
                UserId = userId,
                SiteId = s1.Data.SiteId,
                SerialNumber = "SN_" + RandToken("M"),
                Name = "Equip Name",
                Description = "Move",
                Condition = EquipmentCondition.Working
            }, actorUserId: userId);
            Assert.IsTrue(e.Succeeded);

            // Cross-owner move: only SuperAdmin may do this
            var upd = await EquipmentSvc.UpdateAsync(new EquipmentUpdateDto
            {
                EquipmentId = e.Data.EquipmentId,
                SiteId = s2.Data.SiteId,           // this belongs to userId2
                SerialNumber = e.Data.SerialNumber,
                Description = e.Data.Description,
                Condition = EquipmentCondition.Working
            }, actorUserId: super);
            Assert.IsTrue(upd.Succeeded, string.Join("; ", upd.Errors));

            var regs = Db.SiteEquipmentHistory.Count(h => h.EquipmentId == e.Data.EquipmentId && h.Action == "Register");
            var unregs = Db.SiteEquipmentHistory.Count(h => h.EquipmentId == e.Data.EquipmentId && h.Action == "Unregister");
            Assert.AreEqual(2, regs);   // one at create, one at move
            Assert.AreEqual(1, unregs); // one when moving off s1
        }

    }
}
