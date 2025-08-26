using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Authorization; // UserType, Login* DTOs
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.WPF.Commands;
using KitchenEquipmentDemo.Enterprise.WPF.Models;
using KitchenEquipmentDemo.Enterprise.WPF.Services.Navigation;
using KitchenEquipmentDemo.Enterprise.WPF.ViewModels.Base;

namespace KitchenEquipmentDemo.Enterprise.WPF.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _auth;
        private readonly AuthSession _session;   // DI Singleton
        private readonly INavigationService _nav;

        private string _userName;
        private string _password;
        private string _message;
        private bool _isBusy;

        public string UserName { get => _userName; set => Set(ref _userName, value); }
        public string Password { get => _password; set => Set(ref _password, value); }
        public string Message { get => _message; set => Set(ref _message, value); }
        public bool IsBusy { get => _isBusy; set { if (Set(ref _isBusy, value)) ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged(); } }

        public ICommand LoginCommand { get; }

        public LoginViewModel(IAuthService auth, AuthSession session, INavigationService nav)
        {
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _nav = nav ?? throw new ArgumentNullException(nameof(nav));

            LoginCommand = new AsyncRelayCommand(LoginAsync, () => !IsBusy);

            //Testing
            UserName ="norman.super";
            Password = "Duke!N0rm@n#2025$KE";
        }

        private async Task LoginAsync()
        {
            Message = string.Empty;

            var user = (UserName ?? string.Empty).Trim();
            var pass = Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                Message = "Enter username and password.";
                return;
            }

            try
            {
                IsBusy = true;

                var request = new LoginRequestDto { UserName = user, Password = pass };
                // IAuthService: Task<OperationResult<LoginResultDto>>
                var op = await _auth.LoginAsync(request).ConfigureAwait(false);

                await DispatchToUiAsync(() =>
                {
                    if (op == null || !op.Succeeded || op.Data == null || !op.Data.Success)
                    {
                        Message = (op?.Errors != null && op.Errors.Any())
                                  ? string.Join("\n", op.Errors)
                                  : "Invalid username or password.";
                        return;
                    }

                    var data = op.Data; // LoginResultDto

                    _session.Reset();
                    _session.UserId = data.UserId;
                    _session.UserName = string.IsNullOrWhiteSpace(data.FullName) ? user : data.FullName;
                    _session.UserType = data.UserType.ToString(); // "SuperAdmin" | "Staff"
                    _session.IsAuthenticated = true;

                    // Permissions from role
                    _session.Permissions.Clear();
                    if (data.UserType == UserType.SuperAdmin)
                    {
                        _session.Permissions.Add(AppPermission.ManageUsers);
                        _session.Permissions.Add(AppPermission.ManageSignUpRequests);
                        _session.Permissions.Add(AppPermission.ManageEquipmentActivity);
                    }
                    _session.Permissions.Add(AppPermission.ManageProfile);
                    _session.Permissions.Add(AppPermission.ManageDashboard);
                    _session.Permissions.Add(AppPermission.ManageSites);
                    _session.Permissions.Add(AppPermission.ManageEquipments);
                    _session.SelectedNavItem = NavItem.Dashboard;
                    

                    Password = string.Empty;
                    _nav.Navigate<DashboardViewModel>();
                    _session.Refresh();
                });
            }
            catch (Exception ex)
            {
                Message = "Login failed. Please try again.";
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private Task DispatchToUiAsync(Action action)
        {
            action();
            return Task.CompletedTask;
        }
    }
}
