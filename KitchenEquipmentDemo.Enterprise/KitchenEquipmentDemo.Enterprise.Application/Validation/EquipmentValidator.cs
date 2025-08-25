using System.Collections.Generic;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Equipments;

namespace KitchenEquipmentDemo.Enterprise.Application.Validation
{
    public class EquipmentCreateValidator : IValidator<EquipmentCreateDto>
    {
        public List<string> Validate(EquipmentCreateDto instance)
        {
            var errors = new List<string>();
            if (instance == null)
            {
                errors.Add("Request is required.");
                return errors;
            }

            errors.PositiveId(instance.UserId, "UserId");
            errors.Required(instance.SerialNumber, "Serial number");
            errors.MaxLen(instance.SerialNumber, 100, "Serial number");
            errors.MaxLen(instance.Description, 200, "Description");

            return errors;
        }
    }

    public class EquipmentUpdateValidator : IValidator<EquipmentUpdateDto>
    {
        public List<string> Validate(EquipmentUpdateDto instance)
        {
            var errors = new List<string>();
            if (instance == null)
            {
                errors.Add("Request is required.");
                return errors;
            }

            errors.PositiveId(instance.EquipmentId, "EquipmentId");
            errors.MaxLen(instance.SerialNumber, 100, "Serial number");
            errors.MaxLen(instance.Description, 200, "Description");

            return errors;
        }
    }
}
