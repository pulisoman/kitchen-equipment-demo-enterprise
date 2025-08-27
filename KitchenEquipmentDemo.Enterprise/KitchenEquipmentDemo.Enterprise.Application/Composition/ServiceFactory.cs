using System;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.Application.Services;
using KitchenEquipmentDemo.Enterprise.Data.Context;
using KitchenEquipmentDemo.Enterprise.Data.Repositories;
using KitchenEquipmentDemo.Enterprise.Data.Uow;

namespace KitchenEquipmentDemo.Enterprise.Application.Composition
{
    /// <summary>
    /// Manual service factory for WPF. Creates DbContext, UnitOfWork, repositories, and services.
    /// Call ServiceFactory.Create() once on app startup and reuse for the app lifetime.
    /// </summary>
    public sealed class ServiceFactory : IDisposable
    {
        private readonly AppDbContext _db;
        private readonly UnitOfWork _uow;

        // Repositories
        private readonly UserRepository _userRepo;
        private readonly UserRegistrationRequestRepository _urrRepo;
        private readonly SiteRepository _siteRepo;
        private readonly EquipmentRepository _equipmentRepo;
        private readonly SiteEquipmentHistoryRepository _historyRepo;

        // Services (lazy)
        private IAuthService _authService;
        private IUserRegistrationService _registrationService;
        private ISiteService _siteService;
        private IEquipmentService _equipmentService;
        private IUserService _userService;

        private bool _disposed;

        private ServiceFactory(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _uow = new UnitOfWork(db);

            // Instantiate repositories
            _userRepo = new UserRepository(db);
            _urrRepo = new UserRegistrationRequestRepository(db);
            _siteRepo = new SiteRepository(db);
            _equipmentRepo = new EquipmentRepository(db);
            _historyRepo = new SiteEquipmentHistoryRepository(db);
        }

        /// <summary>
        /// Create a factory with the default AppDbContext (uses connection string from config).
        /// </summary>
        public static ServiceFactory Create() => new ServiceFactory(new AppDbContext());

        /// <summary>
        /// If your AppDbContext supports a named connection string constructor, you can use this.
        /// </summary>
        public static ServiceFactory Create(string nameOrConnectionString)
            => new ServiceFactory(new AppDbContext(nameOrConnectionString));

        // ---- Services ----
        public IAuthService Auth()
            => _authService ?? (_authService = new AuthService(_userRepo, _uow));

        public IUserRegistrationService Registration()
            => _registrationService ?? (_registrationService = new UserRegistrationService(_urrRepo, _userRepo, _uow));

        public ISiteService Sites()
            => _siteService ?? (_siteService = new SiteService(_userRepo, _siteRepo, _equipmentRepo, _historyRepo, _uow));

        public IEquipmentService Equipments()
            => _equipmentService ?? (_equipmentService = new EquipmentService(_equipmentRepo, _userRepo, _siteRepo, _historyRepo, _uow));

        public IUserService Users()
            => _userService ?? (_userService = new UserService(_userRepo, _siteRepo, _equipmentRepo, _uow));

        public void Dispose()
        {
            if (_disposed) return;
            _uow.Dispose();  // disposes DbContext
            _disposed = true;
        }
    }
}
