using System.Collections.Generic;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Sites;

namespace KitchenEquipmentDemo.Enterprise.Application.Validation
{
    public class SiteCreateValidator : IValidator<SiteCreateDto>
    {
        public List<string> Validate(SiteCreateDto instance)
        {
            var errors = new List<string>();
            if (instance == null)
            {
                errors.Add("Request is required.");
                return errors;
            }

            errors.PositiveId(instance.UserId, "UserId");
            errors.Required(instance.Code, "Code");
            errors.Required(instance.Name, "Name");

            errors.MaxLen(instance.Code, 100, "Code");
            errors.MaxLen(instance.Name, 200, "Name");
            errors.MaxLen(instance.Description, 200, "Description");

            return errors;
        }
    }

    public class SiteUpdateValidator : IValidator<SiteUpdateDto>
    {
        public List<string> Validate(SiteUpdateDto instance)
        {
            var errors = new List<string>();
            if (instance == null)
            {
                errors.Add("Request is required.");
                return errors;
            }

            errors.PositiveId(instance.SiteId, "SiteId");
            errors.MaxLen(instance.Code, 100, "Code");
            errors.MaxLen(instance.Name, 200, "Name");
            errors.MaxLen(instance.Description, 200, "Description");

            return errors;
        }
    }
}
