using System;
using System.ComponentModel;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.Data.Models;
using KitchenEquipmentDemo.Enterprise.WPF.Commands;
using KitchenEquipmentDemo.Enterprise.WPF.Models;
using KitchenEquipmentDemo.Enterprise.WPF.Services.Navigation;
using KitchenEquipmentDemo.Enterprise.WPF.ViewModels.Base;
using KitchenEquipmentDemo.Enterprise.WPF.Views;

namespace KitchenEquipmentDemo.Enterprise.WPF.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        public AuthSession Session { get; }
        public INavigationService Navigation { get; }
        public ICommand GoDashboardCommand { get; }
        public ICommand GoUsersCommand { get; }
        public ICommand GoSignUpRequestsCommand { get; }
        public ICommand GoEquipmentActivityCommand { get; }
        public ICommand GoProfileCommand { get; }
        public ICommand GoSitesCommand { get; }
        public ICommand GoEquipmentsCommand { get; }
        public ICommand LogoutCommand { get; }

        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
            }
        }

        public MainWindowViewModel(IUserService userService, AuthSession session, INavigationService navigation)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            Session = session;
            Navigation = navigation;
            
            GoDashboardCommand = new RelayCommand(() => { Navigation.Navigate<DashboardViewModel>(); Session.SelectedNavItem = NavItem.Dashboard; });
            GoUsersCommand = new RelayCommand(() => { Navigation.Navigate<UsersViewModel>(); Session.SelectedNavItem = NavItem.Users; });
            GoProfileCommand = new AsyncRelayCommand( () => EditProfileAsync(Session.UserId), () => !IsBusy);
            GoSitesCommand = new RelayCommand(() => { Navigation.Navigate<SitesViewModel>(); Session.SelectedNavItem = NavItem.Sites; });
            GoEquipmentsCommand = new RelayCommand(() => { Navigation.Navigate<EquipmentsViewModel>(); Session.SelectedNavItem = NavItem.Equipments; });
            GoSignUpRequestsCommand = new RelayCommand(() => { Navigation.Navigate<UserRegistrationsViewModel>(); Session.SelectedNavItem = NavItem.SignUpRequests; });
            LogoutCommand = new RelayCommand(Logout);
        }

        private async Task EditProfileAsync(int userId)
        {
            if (IsBusy) return;

            IsBusy = true;

            try
            {
                var result = await _userService.GetAsync(userId, Session.UserId);

                if (result?.Data != null)
                {
                    Session.SelectedNavItem = NavItem.Profile;
                    Navigation.Navigate(null);
                    result.Data.Action = "Edit";
                    result.Data.ScreenName = "Edit Profile";
                    Navigation.Navigate<UserFormViewModel>(result.Data);
                }
                else
                {
                    MessageBox.Show($"User data not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Logout()
        {
            Session.Reset();
            Navigation.Navigate<LoginViewModel>();
        }
    }
}
