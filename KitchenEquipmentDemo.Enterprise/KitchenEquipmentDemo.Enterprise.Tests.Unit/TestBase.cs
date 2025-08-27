using System;
using System.Data.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KitchenEquipmentDemo.Enterprise.Data.Context;
using KitchenEquipmentDemo.Enterprise.Data.Uow;
using KitchenEquipmentDemo.Enterprise.Data.Repositories;
using KitchenEquipmentDemo.Enterprise.Data.Models;
using KitchenEquipmentDemo.Enterprise.Application.Services;

namespace KitchenEquipmentDemo.Enterprise.Tests.Unit
{
    /// <summary>
    /// Opens a real AppDbContext and starts a transaction per test.
    /// All changes are rolled back in TestCleanup so the database is unchanged.
    /// </summary>
    public abstract class TestBase
    {
        protected AppDbContext Db;
        protected UnitOfWork Uow;
        protected DbContextTransaction Tx;

        // Repositories
        protected UserRepository UserRepo;
        protected SiteRepository SiteRepo;
        protected EquipmentRepository EquipmentRepo;
        protected SiteEquipmentHistoryRepository HistoryRepo;
        protected UserRegistrationRequestRepository UrrRepo;

        // Services
        protected AuthService AuthSvc;
        protected UserRegistrationService RegSvc;
        protected SiteService SiteSvc;
        protected EquipmentService EquipmentSvc;
        protected UserService UserSvc;

        [TestInitialize]
        public void Init()
        {
            // Prefer a test DB connection string if available:
            // Db = new AppDbContext("name=KitchenEquipmentDemo_Test");
            Db = new AppDbContext();

            Uow = new UnitOfWork(Db);
            Tx = Db.Database.BeginTransaction();

            UserRepo = new UserRepository(Db);
            SiteRepo = new SiteRepository(Db);
            EquipmentRepo = new EquipmentRepository(Db);
            HistoryRepo = new SiteEquipmentHistoryRepository(Db);
            UrrRepo = new UserRegistrationRequestRepository(Db);

            AuthSvc = new AuthService(UserRepo, Uow);
            RegSvc = new UserRegistrationService(UrrRepo, UserRepo, Uow);
            SiteSvc = new SiteService(SiteRepo, EquipmentRepo, HistoryRepo, Uow);
            EquipmentSvc = new EquipmentService(EquipmentRepo, UserRepo, SiteRepo, HistoryRepo, Uow);
            UserSvc = new UserService(UserRepo, Uow);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                Tx?.Rollback(); // discard all changes
            }
            finally
            {
                Tx?.Dispose();
                Uow?.Dispose();
                Db?.Dispose();
            }
        }

        protected string RandToken(string prefix) => $"{prefix}_{Guid.NewGuid():N}".Substring(0, 24);
        // Alias for any stray calls
        protected string Rand(string prefix) => RandToken(prefix);

        protected int CreateTestUser(string role = "Admin")
        {
            var u = new User
            {
                FirstName = "Test",
                LastName = "User",
                UserName = RandToken("u"),
                EmailAddress = $"{RandToken("u")}@example.local",
                PasswordSalt = new byte[16], // not used in these tests
                PasswordHash = new byte[32],
                UserType = role,
                IsDeleted = false,
                CreatedBy = null
            };
            UserRepo.AddAsync(u).Wait();
            Uow.SaveChanges();
            return u.UserId;
        }
    }
}
