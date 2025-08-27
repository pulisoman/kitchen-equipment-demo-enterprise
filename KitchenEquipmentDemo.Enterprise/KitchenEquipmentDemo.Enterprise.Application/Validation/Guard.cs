using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KitchenEquipmentDemo.Enterprise.Application.Validation
{
    public static class Guard
    {
        public static void Required(this List<string> errors, string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                errors.Add($"{fieldName} is required.");
        }

        public static void MaxLen(this List<string> errors, string value, int max, string fieldName)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > max)
                errors.Add($"{fieldName} must be at most {max} characters.");
        }

        public static void PositiveId(this List<string> errors, int? id, string fieldName)
        {
            if (id <= 0) errors.Add($"{fieldName} must be a positive id.");
        }

        public static void Email(this List<string> errors, string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            try
            {
                var addr = new System.Net.Mail.MailAddress(value);
                if (!string.Equals(addr.Address, value, StringComparison.OrdinalIgnoreCase))
                    errors.Add("Invalid email address.");
            }
            catch
            {
                errors.Add("Invalid email address.");
            }
        }

        public static void OneOf(this List<string> errors, string value, string fieldName, params string[] allowed)
        {
            if (value == null) return;
            foreach (var a in allowed)
                if (string.Equals(a, value, StringComparison.Ordinal)) return;
            errors.Add($"{fieldName} is invalid.");
        }
    }
}
