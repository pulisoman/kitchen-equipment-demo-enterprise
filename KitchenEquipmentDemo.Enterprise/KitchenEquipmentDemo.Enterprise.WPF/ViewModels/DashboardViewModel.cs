using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.WPF.Commands;
using KitchenEquipmentDemo.Enterprise.WPF.Models;
using KitchenEquipmentDemo.Enterprise.WPF.Services.Navigation;
using KitchenEquipmentDemo.Enterprise.WPF.ViewModels.Base;

namespace KitchenEquipmentDemo.Enterprise.WPF.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly AuthSession _session;
        private readonly INavigationService _navigation;

        public bool IsBusy { get; set; }
        public AuthSession Session => _session;
        public ObservableCollection<DashboardTile> Tiles { get; } = new ObservableCollection<DashboardTile>();

        public DashboardViewModel(IUserService userService, AuthSession session, INavigationService navigation)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            InitializeTiles();
        }

        private void InitializeTiles()
        {
            AddTileIfVisible("&#xE77B;", "Profile", "Manage your account",
                            NavItem.Profile, _session.CanManageProfile);

            AddTileIfVisible("&#xE716;", "Users", "Administer users",
                            NavItem.Users, _session.CanManageUsers);

            AddTileIfVisible("&#xE707;", "Sites", "Manage locations",
                            NavItem.Sites, _session.CanManageSites);

            AddTileIfVisible("&#xE7F8;", "Equipments", "Maintain inventory",
                            NavItem.Equipments, _session.CanManageEquipments);

            AddTileIfVisible("&#xE8D4;", "Sign-Up Requests", "Approve or reject requests",
                            NavItem.SignUpRequests, _session.CanManageSignUpRequests);

            AddTileIfVisible("&#xE8D4;", "Equipment Activity", "View equipment activity logs",
                            NavItem.EquipmentActivity, _session.CanManageEquipmentActivity);
        }

        private void AddTileIfVisible(string icon, string title, string subtitle,
                                     NavItem navItem, bool isVisible)
        {
            if (!isVisible) return;

            Tiles.Add(new DashboardTile
            {
                Icon = ConvertHtmlEntityToUnicode(icon),
                Title = title,
                Subtitle = subtitle,
                Command = new RelayCommand(() => Select(navItem)),
                IsVisible = true
            });
        }

        private string ConvertHtmlEntityToUnicode(string htmlEntity)
        {
            if (string.IsNullOrEmpty(htmlEntity) || !htmlEntity.StartsWith("&#x") || !htmlEntity.EndsWith(";"))
                return htmlEntity;

            // Extract the hex code (remove "&#x" and ";")
            string hexCode = htmlEntity.Substring(3, htmlEntity.Length - 4);

            if (int.TryParse(hexCode, System.Globalization.NumberStyles.HexNumber, null, out int codePoint))
            {
                return char.ConvertFromUtf32(codePoint);
            }

            return htmlEntity; // Return original if conversion fails
        }
        private async Task Select(NavItem target)
        {
            _session.SelectedNavItem = target;
            if(target == NavItem.Profile )
                 await EditProfileAsync(Session.UserId);
            else if (target == NavItem.Dashboard)
                _navigation.Navigate<DashboardViewModel>();
            else if (target == NavItem.Sites)
                _navigation.Navigate<SitesViewModel>();
            else if (target == NavItem.Equipments)
                _navigation.Navigate<EquipmentsViewModel>();
            else if (target == NavItem.SignUpRequests)
                _navigation.Navigate<UserRegistrationsViewModel>();
            else if (target == NavItem.Users)
                _navigation.Navigate<UsersViewModel>();
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
                    _navigation.Navigate(null);
                    result.Data.Action = "Edit";
                    result.Data.ScreenName = "Edit Profile";
                    _navigation.Navigate<UserFormViewModel>(result.Data);
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
    }
}