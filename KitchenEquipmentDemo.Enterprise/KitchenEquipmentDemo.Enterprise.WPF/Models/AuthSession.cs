using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;


namespace KitchenEquipmentDemo.Enterprise.WPF.Models
{
    public enum AppPermission
    {
        ManageProfile,
        ManageDashboard,
        ManageUsers,
        ManageSignUpRequests,
        ManageEquipmentActivity,
        ManageSites,
        ManageEquipments
    }

    public class AuthSession: INotifyPropertyChanged
    {
        private NavItem _selectedNavItem = NavItem.None;
        public NavItem SelectedNavItem
        {
            get => _selectedNavItem;
            set
            {
                if (_selectedNavItem == value) return;
                _selectedNavItem = value;
                OnChanged(nameof(SelectedNavItem));
            }
        }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public bool IsAuthenticated { get; set; }

        public HashSet<AppPermission> Permissions { get; } = new HashSet<AppPermission>();

        public event PropertyChangedEventHandler PropertyChanged;

        public bool HasPermission(AppPermission permission) => Permissions.Contains(permission);

        public bool CanManageProfile => HasPermission(AppPermission.ManageProfile);
        public bool CanManageDashboard => HasPermission(AppPermission.ManageDashboard);
        public bool CanManageUsers => HasPermission(AppPermission.ManageUsers);
        public bool CanManageSites => HasPermission(AppPermission.ManageSites);
        public bool CanManageEquipments => HasPermission(AppPermission.ManageEquipments);
        public bool CanManageSignUpRequests => HasPermission(AppPermission.ManageSignUpRequests);
        public bool CanManageEquipmentActivity => HasPermission(AppPermission.ManageEquipmentActivity);

        public void Reset()
        {
            UserId = 0;
            UserName = null;
            UserType = null;
            IsAuthenticated = false;
            Permissions.Clear();
            Refresh();
        }

        private static readonly string[] _allPropNames =
            typeof(AuthSession)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetIndexParameters().Length == 0)
                .Select(p => p.Name)
                .ToArray();

        public void Refresh()
        {
            // Notify every bound property in one shot (includes computed ones)
            //foreach (var name in _allPropNames)
            //    OnChanged(name);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }

        private void OnChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
