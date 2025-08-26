using Microsoft.VisualStudio.TestTools.UnitTesting;
using KitchenEquipmentDemo.Enterprise.Application.Validation;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Authorization;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Sites;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Equipments;

namespace KitchenEquipmentDemo.Enterprise.Tests.Unit
{
    [TestClass]
    public class ValidationTests
    {
        [TestMethod]
        public void Login_Validator_Flags_Missing()
        {
            var v = new LoginRequestValidator();
            var errs = v.Validate(new LoginRequestDto { UserName = "", Password = "" });
            Assert.IsTrue(errs.Count >= 2);
        }

        [TestMethod]
        public void Signup_Validator_Flags_Missing()
        {
            var v = new SignupRequestValidator();
            var errs = v.Validate(new SignupRequestDto { FirstName = "", LastName = "", EmailAddress = "x", UserName = "", PasswordPlain = "" });
            Assert.IsTrue(errs.Count >= 4);
        }

        [TestMethod]
        public void SiteCreate_Validator_Flags_Missing()
        {
            var v = new SiteCreateValidator();
            var errs = v.Validate(new SiteCreateDto { UserId = 0, Code = "", Name = "" });
            Assert.IsTrue(errs.Count >= 3);
        }

        [TestMethod]
        public void EquipmentCreate_Validator_Flags_Missing()
        {
            var v = new EquipmentCreateValidator();
            var errs = v.Validate(new EquipmentCreateDto { UserId = 0, SerialNumber = "" });
            Assert.IsTrue(errs.Count >= 2);
        }
    }
}
