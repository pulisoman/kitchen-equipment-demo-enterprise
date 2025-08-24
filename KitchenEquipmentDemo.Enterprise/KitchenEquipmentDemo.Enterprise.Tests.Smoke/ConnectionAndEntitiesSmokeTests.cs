using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using KitchenEquipmentDemo.Enterprise.Data;
namespace KitchenEquipmentDemo.Enterprise.Tests.Smoke
{
    [TestFixture]
    [Category("Smoke")]
    public class ConnectionAndEntitiesSmokeTests
    {
      
        [Test]
        public void CanQueryAllDbSets_AndTestConnection()
        {
            var failures = new List<string>();

            using (var db = new AppDbContext())
            {
                try
                {
                    // Force EF6 model to initialize
                    db.Database.Initialize(false);

                    // Try opening the connection
                    db.Database.Connection.Open();

                    if (db.Database.Connection.State == ConnectionState.Open)
                        TestContext.WriteLine("✓ Database connection opened successfully.");
                    else
                    {
                        failures.Add("✗ Database connection not in Open state.");
                        TestContext.WriteLine(failures.Last());
                    }
                }
                catch (Exception ex)
                {
                    failures.Add("✗ Connection failed — " + ex.GetBaseException().Message);
                    TestContext.WriteLine(failures.Last());
                }

                // Find all DbSet<T> properties
                var dbSets = db.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.PropertyType.IsGenericType &&
                                p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    .ToList();

                if (!dbSets.Any())
                {
                    failures.Add("✗ No DbSet<> properties found on AppDbContext.");
                }

                foreach (var prop in dbSets)
                {
                    var entityType = prop.PropertyType.GetGenericArguments()[0];
                    var label = $"{prop.Name} (DbSet<{entityType.Name}>)";

                    try
                    {
                        // Get DbSet<T> instance
                        var dbSetObj = prop.GetValue(db, null);

                        // Build: ((IQueryable<T>)dbSetObj).Any()
                        var anyMethod = typeof(Queryable)
                            .GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .First(m => m.Name == "Any" && m.GetParameters().Length == 1)
                            .MakeGenericMethod(entityType);

                        bool hasRows = (bool)anyMethod.Invoke(null, new object[] { dbSetObj });

                        TestContext.WriteLine($"✓ {label} — {(hasRows ? "has data" : "empty")}");
                    }
                    catch (Exception ex)
                    {
                        var msg = $"✗ {label} — {ex.GetBaseException().Message}";
                        failures.Add(msg);
                        TestContext.WriteLine(msg);
                    }
                }

            }

            if (failures.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Smoke Test FAILURES:");
                foreach (var f in failures) sb.AppendLine("  " + f);
                Assert.Fail(sb.ToString());  // single fail with consolidated details
            }
        }
    }
}
