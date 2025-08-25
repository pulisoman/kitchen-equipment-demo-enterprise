using System;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Data.Models;

namespace KitchenEquipmentDemo.Enterprise.Application.Authorization
{
    /// <summary>
    /// Tiny helper to centralize authorization checks.
    /// </summary>
    public static class AccessGuard
    {
        public static void EnsureSuperAdmin(User actor)
        {
            if (actor == null || actor.IsDeleted || !string.Equals(actor.UserType, "SuperAdmin", StringComparison.Ordinal))
                throw new UnauthorizedAccessException("SuperAdmin role required.");
        }

        public static bool IsSuperAdmin(User actor)
            => actor != null && !actor.IsDeleted && string.Equals(actor.UserType, "SuperAdmin", StringComparison.Ordinal);
    }
}
