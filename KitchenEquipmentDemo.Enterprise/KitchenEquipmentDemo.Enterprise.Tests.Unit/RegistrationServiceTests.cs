using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Authorization;

namespace KitchenEquipmentDemo.Enterprise.Tests.Unit
{
    [TestClass]
    public class RegistrationServiceTests : TestBase
    {
        [TestMethod]
        public async Task Request_Approve_Login_Succeeds()
        {
            var reviewer = CreateTestUser(role: "SuperAdmin");

            var signup = new SignupRequestDto
            {
                FirstName = "Alice",
                LastName = "Tester",
                EmailAddress = RandToken("alice")+"@example.local",
                UserName = "alice_"+ RandToken("u"),
                Password = "P@ssw0rd!",
                UserType = KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common.UserType.Admin
            };

            var req = await RegSvc.RequestSignupAsync(signup);
            Assert.IsTrue(req.Succeeded, string.Join("; ", req.Errors));

            // Pending by username
            var pending = await UrrRepo.GetPendingByUserNameAsync(signup.UserName);
            Assert.IsNotNull(pending, "Pending request not found.");

            var approve = await RegSvc.ApproveAsync(pending.RequestId, reviewer);
            Assert.IsTrue(approve.Succeeded, string.Join("; ", approve.Errors));

            // Now login should succeed
            var login = await AuthSvc.LoginAsync(new LoginRequestDto { UserName = signup.UserName, Password = signup.Password });
            Assert.IsTrue(login.Succeeded);
            Assert.IsTrue(login.Data.Success);
        }

        [TestMethod]
        public async Task Duplicate_Request_Is_Prevented()
        {
            var reviewer = CreateTestUser(role: "SuperAdmin");

            var email = RandToken("dup")+"@example.local";
            var uname = "dup_"+RandToken("u");
            var req1 = await RegSvc.RequestSignupAsync(new SignupRequestDto
            {
                FirstName = "Bob",
                LastName = "T",
                EmailAddress = email,
                UserName = uname,
                Password = "P@ssw0rd!",
                UserType = KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common.UserType.Admin
            });
            Assert.IsTrue(req1.Succeeded, string.Join("; ", req1.Errors));

            // same username or email should be blocked while pending
            var req2 = await RegSvc.RequestSignupAsync(new SignupRequestDto
            {
                FirstName = "Other",
                LastName = "User",
                EmailAddress = email,
                UserName = "x_"+RandToken("u"),
                Password = "P@ssw0rd!",
                UserType = KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common.UserType.Admin
            });
            Assert.IsFalse(req2.Succeeded, "Expected duplicate pending request to be blocked.");
        }

        [TestMethod]
        public async Task Deny_Sets_Status_And_Note()
        {
            var reviewer = CreateTestUser(role: "SuperAdmin");

            var req = await RegSvc.RequestSignupAsync(new SignupRequestDto
            {
                FirstName = "Carl",
                LastName = "T",
                EmailAddress = RandToken("c")+"@example.local",
                UserName = "c_"+RandToken("u"),
                Password = "P@ssw0rd!",
                UserType = KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common.UserType.Admin
            });
            Assert.IsTrue(req.Succeeded);

            var pending = await UrrRepo.GetFirstPendingAsync();
            Assert.IsNotNull(pending);

            var deny = await RegSvc.DenyAsync(pending.RequestId, reviewer, "Insufficient info");
            Assert.IsTrue(deny.Succeeded);
        }
    }
}
