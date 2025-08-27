using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Users;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.Data.Models;
using KitchenEquipmentDemo.Enterprise.WPF.Commands;
using KitchenEquipmentDemo.Enterprise.WPF.Models;
using KitchenEquipmentDemo.Enterprise.WPF.Services.Navigation;
using KitchenEquipmentDemo.Enterprise.WPF.ViewModels.Base;

namespace KitchenEquipmentDemo.Enterprise.WPF.ViewModels
{
    public class UserFormViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly IUserService _mainService;
        private readonly AuthSession _session;
        private readonly INavigationService _nav;
        private readonly UserDto _userToUpdate;
        public string ScreenName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string CurrentPassword { get; set; }

        private UserType _SelectedUserType;

        private ObservableCollection<UserType> _userTypes;

        private bool _isBusy;

        public ICommand ChangePasswordCommand { get; }
        public ICommand UpdateInfoCommand { get; }

        public UserFormViewModel(IUserService mainService,
                                 AuthSession session,
                                 INavigationService nav)
        {
            _mainService = mainService ?? throw new ArgumentNullException(nameof(mainService));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _nav = nav ?? throw new ArgumentNullException(nameof(nav));
            ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync, () => !IsBusy);
            UpdateInfoCommand = new AsyncRelayCommand(UpdateUserInfoAsync, () => !IsBusy);
            _userToUpdate = (UserDto)_nav?.NavigationParameter;
            IsBusy = true;
            InitializeCollections();
            IsBusy = false;
            _ = LoadUserDataAsync(_userToUpdate.UserId);
            ScreenName = string.IsNullOrWhiteSpace(_userToUpdate.ScreenName) ? "User Form" : _userToUpdate.ScreenName;
        }

        private void InitializeCollections()
        {
            ScreenName = "Users Management";

            UserTypes = new ObservableCollection<UserType>()
            {
                UserType.Admin,
                UserType.SuperAdmin
            };

            if (!_session.UserType.Equals(UserType.SuperAdmin.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                UserTypes.Remove(UserType.SuperAdmin);
            }
        }

        //create LoadUserDataAsync method
        private async Task LoadUserDataAsync(int userId)
        {
            if (IsBusy) return; 
            IsBusy = true;

            var user = await _mainService.GetAsync(userId, _session.UserId);
            if (user?.Data != null)
            {
                UserName = user.Data.UserName;
                FirstName = user.Data.FirstName;
                LastName = user.Data.LastName;
                EmailAddress = user.Data.EmailAddress;
                SelectedUserType = user.Data.UserType;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
            IsBusy = false;
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        public UserType SelectedUserType
        {
            get => _SelectedUserType;
            set
            {
                _SelectedUserType = value;
                OnPropertyChanged(nameof(SelectedUserType));
            }
        }
        public ObservableCollection<UserType> UserTypes
        {
            get => _userTypes;
            set
            {
                _userTypes = value;
                OnPropertyChanged(nameof(UserTypes));
            }
        }

        private async Task UpdateUserInfoAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            var result = await _mainService.UpdateUserInfoAsync(new UserInfoUpdateDto
            {
                UserId = _userToUpdate.UserId,
                UserName = UserName,
                FirstName = FirstName,
                LastName = LastName,
                EmailAddress = EmailAddress,
                UserType = SelectedUserType

            }, _session.UserId);
            if (result.Succeeded)
            {
                MessageBox.Show("User information updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _nav.NavigateBack();
            }
            else
            {
                var errors = string.Join("\n", result.Errors);
                MessageBox.Show($"Failed to update user information:\n{errors}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            IsBusy = false;
        }

        private async Task ChangePasswordAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            var result = await _mainService.UpdatePasswordAsync(new UserPasswordUpdateDto
            {
                UserId = _userToUpdate.UserId,
                NewPassword = NewPassword,
                CurrentPassword = CurrentPassword,
                ConfirmPassword = ConfirmPassword
            }, _session.UserId);


            if (result.Succeeded)
            {
                MessageBox.Show("Password updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _nav.NavigateBack();
            }
            else
            {
                var errors = string.Join("\n", result.Errors);
                MessageBox.Show($"Failed to update password:\n{errors}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            IsBusy = false;
        }

        // INotifyPropertyChanged implementation
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
