using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Equipments;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.WPF.Commands;
using KitchenEquipmentDemo.Enterprise.WPF.Models;
using KitchenEquipmentDemo.Enterprise.WPF.Services.Navigation;
using KitchenEquipmentDemo.Enterprise.WPF.ViewModels.Base;

namespace KitchenEquipmentDemo.Enterprise.WPF.ViewModels
{
    public class EquipmentFormViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly IEquipmentService _mainService;
        private readonly AuthSession _session;
        private readonly INavigationService _nav;
        private readonly EquipmentDto _equipmentToUpdate;

        public string ScreenName { get; set; }
        public int EquipmentId { get; set; }
        public string SerialNumber { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public EquipmentCondition Condition { get; set; }
        public int? SiteId { get; set; }
        public int? UserId { get; set; }

        private ObservableCollection<EquipmentCondition> _equipmentConditions;
        private bool _isBusy;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EquipmentFormViewModel(IEquipmentService mainService,
                                     AuthSession session,
                                     INavigationService nav)
        {
            _mainService = mainService ?? throw new ArgumentNullException(nameof(mainService));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _nav = nav ?? throw new ArgumentNullException(nameof(nav));

            SaveCommand = new AsyncRelayCommand(SaveEquipmentAsync, () => !IsBusy);
            CancelCommand = new RelayCommand(Cancel, () => !IsBusy);

            _equipmentToUpdate = (EquipmentDto)_nav?.NavigationParameter;
            IsBusy = true;
            InitializeCollections();
            IsBusy = false;

            UserId = _session.UserId;

            if (_equipmentToUpdate != null && _equipmentToUpdate.EquipmentId > 0)
            {
                _ = LoadEquipmentDataAsync(_equipmentToUpdate.EquipmentId);
                ScreenName = string.IsNullOrWhiteSpace(_equipmentToUpdate.ScreenName) ? "Edit Equipment" : _equipmentToUpdate.ScreenName;
            }
            else
            {
                ScreenName = "Add New Equipment";
            }
        }

        private void InitializeCollections()
        {
            EquipmentConditions = new ObservableCollection<EquipmentCondition>
            {
                EquipmentCondition.Working,
                EquipmentCondition.NotWorking,
            };
        }

        private async Task LoadEquipmentDataAsync(int equipmentId)
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var equipment = await _mainService.GetAsync(equipmentId);
                if (equipment?.Data != null)
                {
                    EquipmentId = equipment.Data.EquipmentId;
                    SerialNumber = equipment.Data.SerialNumber;
                    Description = equipment.Data.Description;
                    Condition = equipment.Data.Condition;
                    SiteId = equipment.Data.SiteId;
                    UserId = equipment.Data.UserId;
                    Name = equipment.Data.Name;

                    // Notify property changes
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(SerialNumber));
                    OnPropertyChanged(nameof(Description));
                    OnPropertyChanged(nameof(Condition));
                    OnPropertyChanged(nameof(SiteId));
                    OnPropertyChanged(nameof(UserId));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading equipment data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public ObservableCollection<EquipmentCondition> EquipmentConditions
        {
            get => _equipmentConditions;
            set
            {
                _equipmentConditions = value;
                OnPropertyChanged(nameof(EquipmentConditions));
            }
        }

        private async Task SaveEquipmentAsync()
        {
            if (IsBusy) return;

            // Validation
            if (string.IsNullOrWhiteSpace(SerialNumber))
            {
                MessageBox.Show("Serial number is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                MessageBox.Show("Description is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsBusy = true;
            try
            {
                OperationResult<EquipmentDto> result;
                

                if (EquipmentId > 0)
                {
                    var equipmentDto = new EquipmentUpdateDto
                    {
                        EquipmentId = EquipmentId,
                        SerialNumber = SerialNumber?.Trim(),
                        Description = Description?.Trim(),
                        Condition = Condition,
                        SiteId = SiteId,
                        UserId = UserId,
                        Name = Name?.Trim(),
                    };
                    // Update existing equipment
                    result = await _mainService.UpdateAsync(equipmentDto, _session.UserId);
                }
                else
                {
                    var equipmentDto = new EquipmentCreateDto
                    {
                        SerialNumber = SerialNumber?.Trim(),
                        Description = Description?.Trim(),
                        Condition = Condition,
                        SiteId = SiteId,
                        UserId = UserId,
                        Name = Name?.Trim()
                    };
                    // Create new equipment
                    result = await _mainService.CreateAsync(equipmentDto, _session.UserId);
                }

                if (result.Succeeded)
                {
                    MessageBox.Show($"Equipment {(EquipmentId > 0 ? "updated" : "created")} successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    _nav.NavigateBack();
                }
                else
                {
                    var errors = string.Join("\n", result.Errors);
                    MessageBox.Show($"Failed to save equipment:\n{errors}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving equipment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Cancel()
        {
            _nav.NavigateBack();
        }

        // INotifyPropertyChanged implementation
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}