using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitchenEquipmentDemo.Enterprise.Seeder
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            try
            {
                SuperAdminSeed.EnsureSuperAdmin();
                StaffUsersSeed.SeedStaffUsers();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                Environment.ExitCode = 1;
            }
        }
    }
}
