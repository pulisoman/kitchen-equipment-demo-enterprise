using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

using KitchenEquipmentDemo.Enterprise.WPF.Models;
using KitchenEquipmentDemo.Enterprise.WPF.Services.Navigation;
using KitchenEquipmentDemo.Enterprise.WPF.ViewModels;
using KitchenEquipmentDemo.Enterprise.WPF.Shell;

// Application layer
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.Application.Services;   // AuthService

// Data layer
using KitchenEquipmentDemo.Enterprise.Data.Context;          // AppDbContext
using KitchenEquipmentDemo.Enterprise.Data.Uow;              // UnitOfWork
using KitchenEquipmentDemo.Enterprise.Data.Repositories;     // UserRepository

namespace KitchenEquipmentDemo.Enterprise.WPF
{
    public partial class App : System.Windows.Application
    {
        public IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var sc = new ServiceCollection();

            // ---------- Core singletons ----------
            sc.AddSingleton<AuthSession>();
            // Your NavigationService constructor takes IServiceProvider, DI will inject it
            sc.AddSingleton<INavigationService, NavigationService>();

            // ---------- Data layer (minimum needed by AuthService) ----------
            sc.AddScoped<AppDbContext>();   // uses connection string from App.config
            sc.AddScoped<UnitOfWork>();
            sc.AddScoped<UserRepository>();
            sc.AddScoped<UserRegistrationRequestRepository>();

            // ---------- Application services ----------
            sc.AddScoped<IAuthService, AuthService>();
            // ✅ Add this to Application services section
            sc.AddScoped<IUserService, UserService>();
            sc.AddScoped<IUserRegistrationService, UserRegistrationService>();


            // ---------- ViewModels in use now ----------
            sc.AddTransient<MainWindowViewModel>();
            sc.AddTransient<LoginViewModel>();
            sc.AddTransient<DashboardViewModel>();
            sc.AddTransient<UsersViewModel>();
            sc.AddTransient<UserFormViewModel>();
            sc.AddTransient<UserRegistrationViewModel>();
            sc.AddTransient<UserRegistrationsViewModel>();

            // ---------- Windows ----------
            sc.AddTransient<MainWindow>();

            Services = sc.BuildServiceProvider();

            // Show shell window
            var win = Services.GetRequiredService<MainWindow>();
            win.DataContext = Services.GetRequiredService<MainWindowViewModel>();
            var session = Services.GetRequiredService<AuthSession>();
            session.Reset();
            //session.IsAuthenticated = true;
            //session.Permissions.Add(AppPermission.ManageDashboard);

            win.Show();

            // Navigate to Login on startup
            var nav = Services.GetRequiredService<INavigationService>();
            nav.Navigate<LoginViewModel>();
        }
    }
}
