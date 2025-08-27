using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Authorization;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.WPF.Commands;
using KitchenEquipmentDemo.Enterprise.WPF.Models;
using KitchenEquipmentDemo.Enterprise.WPF.Services.Navigation;
using KitchenEquipmentDemo.Enterprise.WPF.ViewModels.Base;

namespace KitchenEquipmentDemo.Enterprise.WPF.ViewModels
{
    public class UserRegistrationViewModel : ViewModelBase
    {
        private readonly AuthSession _session;   // DI Singleton
        private readonly INavigationService _nav;
        private readonly IUserRegistrationService _userReg;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public UserType UserType { get; set; }
        public ICommand CancelCommand { get; }
        public ICommand SignUpCommand { get; }

        public UserRegistrationViewModel(AuthSession session, INavigationService nav, IUserRegistrationService userReg)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _nav = nav ?? throw new ArgumentNullException(nameof(nav));
            _userReg = userReg ?? throw new ArgumentNullException(nameof(userReg));
            CancelCommand = new AsyncRelayCommand(CancelAsync);
            SignUpCommand = new AsyncRelayCommand(SignUpAsync);
            ////Testing
            //FirstName = "Norman";
            //LastName = "Super";
            //EmailAddress = "norman.super@example.com";
            //UserName = "norman.super";
            //Password = "Duke!N0rm@n#2025$KE";
            //ConfirmPassword = "Duke!N0rm@n#2025$KE";
            UserType = UserType.Admin;
        }

        private async Task CancelAsync()
        {
            _nav.Navigate<LoginViewModel>();
        }

        private async Task SignUpAsync()
        {
            var result = await _userReg.RequestSignupAsync(new SignupRequestDto()
            {
                FirstName = FirstName,
                LastName = LastName,
                EmailAddress = EmailAddress,
                UserName = UserName,
                Password = Password,
                UserType = UserType.Admin // Or whatever default you want
            });

            if (result != null && result.Succeeded)
            {
                MessageBox.Show("User Registration successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _nav.NavigateBack();
            }
            else
            {
                var errors = string.Join("\n", result.Errors);
                MessageBox.Show($"User Registration faild:\n{errors}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
          
        }
    }
}
