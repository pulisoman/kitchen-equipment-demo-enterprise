using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Authorization;

namespace KitchenEquipmentDemo.Enterprise.Tests.Unit
{
    [TestClass]
    public class AuthServiceTests : TestBase
    {
        [TestMethod]
        public async Task Login_Invalid_Returns_False()
        {
            var res = await AuthSvc.LoginAsync(new LoginRequestDto { UserName = "nope", Password = "nope" });
            Assert.IsTrue(res.Succeeded);
            Assert.IsFalse(res.Data.Success);
        }
    }
}
