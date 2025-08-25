using System.Collections.Generic;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Authorization;

namespace KitchenEquipmentDemo.Enterprise.Application.Validation
{
    public class LoginRequestValidator : IValidator<LoginRequestDto>
    {
        public List<string> Validate(LoginRequestDto instance)
        {
            var errors = new List<string>();
            errors.Required(instance?.UserName, "Username");
            errors.Required(instance?.PasswordPlain, "Password");
            errors.MaxLen(instance?.UserName, 100, "Username");
            errors.MaxLen(instance?.PasswordPlain, 256, "Password");
            return errors;
        }
    }
}
