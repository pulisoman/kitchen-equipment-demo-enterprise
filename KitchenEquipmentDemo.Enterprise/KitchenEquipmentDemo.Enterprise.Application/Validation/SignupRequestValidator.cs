using System.Collections.Generic;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Authorization;

namespace KitchenEquipmentDemo.Enterprise.Application.Validation
{
    public class SignupRequestValidator : IValidator<SignupRequestDto>
    {
        public List<string> Validate(SignupRequestDto instance)
        {
            var errors = new List<string>();
            if (instance == null)
            {
                errors.Add("Request is required.");
                return errors;
            }

            errors.Required(instance.FirstName, "First name");
            errors.Required(instance.LastName, "Last name");
            errors.Required(instance.EmailAddress, "Email");
            errors.Required(instance.UserName, "Username");
            errors.Required(instance.PasswordPlain, "Password");

            errors.MaxLen(instance.FirstName, 100, "First name");
            errors.MaxLen(instance.LastName, 100, "Last name");
            errors.MaxLen(instance.UserName, 100, "Username");
            errors.MaxLen(instance.EmailAddress, 256, "Email");
            errors.Email(instance.EmailAddress, "Email");

            // roles are enum-backed in Contracts; no extra check needed here
            return errors;
        }
    }
}
