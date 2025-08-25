using System.Collections.Generic;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Users;

namespace KitchenEquipmentDemo.Enterprise.Application.Validation
{
    public class UserUpdateValidator : IValidator<UserDto>
    {
        public List<string> Validate(UserDto instance)
        {
            var errors = new List<string>();
            if (instance == null)
            {
                errors.Add("Request is required.");
                return errors;
            }

            errors.PositiveId(instance.UserId, "UserId");
            errors.MaxLen(instance.FirstName, 100, "First name");
            errors.MaxLen(instance.LastName, 100, "Last name");
            errors.MaxLen(instance.EmailAddress, 256, "Email");
            errors.MaxLen(instance.UserName, 100, "Username");
            errors.Email(instance.EmailAddress, "Email");

            return errors;
        }
    }
}
