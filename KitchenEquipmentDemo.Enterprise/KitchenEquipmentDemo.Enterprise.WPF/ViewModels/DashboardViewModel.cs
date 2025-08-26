using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using KitchenEquipmentDemo.Enterprise.WPF.Commands;
using KitchenEquipmentDemo.Enterprise.WPF.Models;
using KitchenEquipmentDemo.Enterprise.WPF.Services.Navigation;
using KitchenEquipmentDemo.Enterprise.WPF.ViewModels.Base;

namespace KitchenEquipmentDemo.Enterprise.WPF.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly AuthSession _session;
        private readonly INavigationService _navigation;

        public AuthSession Session => _session;
        public ObservableCollection<DashboardTile> Tiles { get; } = new ObservableCollection<DashboardTile>();

        public DashboardViewModel(AuthSession session, INavigationService navigation)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));

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
        private void Select(NavItem target)
        {
            _session.SelectedNavItem = target;
            _navigation.Navigate<UsersViewModel>();
        }
    }
}