using System.Collections.Generic;

namespace KitchenEquipmentDemo.Enterprise.Application.Validation
{
    public interface IValidator<T>
    {
        /// <summary>
        /// Returns a list of validation errors. Empty list means valid.
        /// </summary>
        List<string> Validate(T instance);
    }
}
