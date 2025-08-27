using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Equipments;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Sites;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.WPF.Commands;
using KitchenEquipmentDemo.Enterprise.WPF.Models;
using KitchenEquipmentDemo.Enterprise.WPF.Services.Navigation;
using KitchenEquipmentDemo.Enterprise.WPF.ViewModels.Base;

namespace KitchenEquipmentDemo.Enterprise.WPF.ViewModels
{
    public class SiteFormViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly ISiteService _siteService;
        private readonly IEquipmentService _equipmentService;
        private readonly AuthSession _session;
        private readonly INavigationService _nav;
        private readonly SiteDto _siteToUpdate;

        public string ScreenName { get; set; }
        public int SiteId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }

        // Equipment assignment properties
        private ObservableCollection<EquipmentDto> _availableEquipment;
        private ObservableCollection<EquipmentDto> _assignedEquipment;
        private EquipmentDto _selectedAvailableEquipment;
        private EquipmentDto _selectedAssignedEquipment;
        private bool _isBusy;

        public ICommand SaveSiteCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AssignEquipmentCommand { get; }
        public ICommand UnassignEquipmentCommand { get; }

        public SiteFormViewModel(ISiteService siteService,
                                IEquipmentService equipmentService,
                                AuthSession session,
                                INavigationService nav)
        {
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
            _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _nav = nav ?? throw new ArgumentNullException(nameof(nav));

            SaveSiteCommand = new AsyncRelayCommand(SaveSiteAsync, () => !IsBusy);
            CancelCommand = new RelayCommand(Cancel, () => !IsBusy);
            AssignEquipmentCommand = new AsyncRelayCommand(AssignEquipmentAsync, () => !IsBusy && SelectedAvailableEquipment != null);
            UnassignEquipmentCommand = new AsyncRelayCommand(UnassignEquipmentAsync, () => !IsBusy && SelectedAssignedEquipment != null);

            _siteToUpdate = (SiteDto)_nav?.NavigationParameter;
            IsBusy = true;
            InitializeCollections();
            IsBusy = false;

            if (_siteToUpdate != null && _siteToUpdate.SiteId > 0)
            {
                _ = LoadSiteDataAsync(_siteToUpdate.SiteId);
                ScreenName = string.IsNullOrWhiteSpace(_siteToUpdate.ScreenName) ? "Edit Site" : _siteToUpdate.ScreenName;
            }
            else
            {
                ScreenName = "Add New Site";
                Active = true; // Default to active for new sites
            }
        }

        private void InitializeCollections()
        {
            AvailableEquipment = new ObservableCollection<EquipmentDto>();
            AssignedEquipment = new ObservableCollection<EquipmentDto>();
        }

        private async Task LoadSiteDataAsync(int siteId)
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var site = await _siteService.GetAsync(_session.UserId, siteId);
                if (site?.Data != null)
                {
                    SiteId = site.Data.SiteId;
                    Code = site.Data.Code;
                    Name = site.Data.Name;
                    Description = site.Data.Description;
                    Active = site.Data.Active;

                    // Notify property changes
                    OnPropertyChanged(nameof(Code));
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(Description));
                    OnPropertyChanged(nameof(Active));
                }

                // Load equipment data
                await LoadEquipmentDataAsync(siteId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading site data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadEquipmentDataAsync(int siteId)
        {
            try
            {
                // Load available equipment (where SiteId is null)
                var availableResult = await _equipmentService.GetPagedAsync(
                    page: 1,
                    pageSize: 1000, // Large number to get all available equipment
                    searchString: "",
                    ownerId: _session.UserId,
                    actorUserId: _session.UserId,
                    orderBy: "SerialNumber"
                );

                AvailableEquipment = new ObservableCollection<EquipmentDto>(
                    availableResult.Items.Where(e => e.SiteId == null || e.SiteId == 0)
                );

                // Load assigned equipment (where SiteId matches current site)
                var assignedResult = await _equipmentService.GetPagedAsync(
                    page: 1,
                    pageSize: 1000, // Large number to get all assigned equipment
                    searchString: "",
                    ownerId: _session.UserId,
                    actorUserId: _session.UserId,
                    orderBy: "SerialNumber"
                );

                AssignedEquipment = new ObservableCollection<EquipmentDto>(
                    assignedResult.Items.Where(e => e.SiteId == siteId)
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading equipment data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        public ObservableCollection<EquipmentDto> AvailableEquipment
        {
            get => _availableEquipment;
            set
            {
                _availableEquipment = value;
                OnPropertyChanged(nameof(AvailableEquipment));
            }
        }

        public ObservableCollection<EquipmentDto> AssignedEquipment
        {
            get => _assignedEquipment;
            set
            {
                _assignedEquipment = value;
                OnPropertyChanged(nameof(AssignedEquipment));
            }
        }

        public EquipmentDto SelectedAvailableEquipment
        {
            get => _selectedAvailableEquipment;
            set
            {
                _selectedAvailableEquipment = value;
                OnPropertyChanged(nameof(SelectedAvailableEquipment));
            }
        }

        public EquipmentDto SelectedAssignedEquipment
        {
            get => _selectedAssignedEquipment;
            set
            {
                _selectedAssignedEquipment = value;
                OnPropertyChanged(nameof(SelectedAssignedEquipment));
            }
        }

        private async Task SaveSiteAsync()
        {
            if (IsBusy) return;

            // Validation
            if (string.IsNullOrWhiteSpace(Code))
            {
                MessageBox.Show("Site code is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Site name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsBusy = true;
            try
            {
                OperationResult<SiteDto> result;
                var siteDto = new SiteDto
                {
                    SiteId = SiteId,
                    Code = Code?.Trim(),
                    Name = Name?.Trim(),
                    Description = Description?.Trim(),
                    Active = Active
                };

                if (SiteId > 0)
                {                   

                    SiteUpdateDto siteUpdateDto = new SiteUpdateDto
                    {
                        SiteId = SiteId,
                        Code = Code?.Trim(),
                        Name = Name?.Trim(),
                        Description = Description?.Trim(),
                        Active = Active,
                        UserId = _session.UserId,
                    };
                    // Update existing site
                    result = await _siteService.UpdateAsync(siteUpdateDto, _session.UserId);
                }
                else
                {
                    SiteCreateDto siteCreateDto = new SiteCreateDto
                    {
                        Code = Code?.Trim(),
                        Name = Name?.Trim(),
                        Description = Description?.Trim(),
                        UserId = _session.UserId,
                        Active = Active
                    };
                    result = await _siteService.CreateAsync(siteCreateDto, _session.UserId);
                }

                if (result.Succeeded)
                {
                    MessageBox.Show($"Site {(SiteId > 0 ? "updated" : "created")} successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    _nav.NavigateBack();
                }
                else
                {
                    var errors = string.Join("\n", result.Errors);
                    MessageBox.Show($"Failed to save site:\n{errors}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving site: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AssignEquipmentAsync()
        {
            if (SelectedAvailableEquipment == null || SiteId == 0) return;

            IsBusy = true;
            try
            {
                var equipmentDto = SelectedAvailableEquipment;
                equipmentDto.SiteId = SiteId;

                EquipmentUpdateDto equipmentUpdateDto = new EquipmentUpdateDto
                {
                    EquipmentId = equipmentDto.EquipmentId,
                    SerialNumber = equipmentDto.SerialNumber,
                    Description = equipmentDto.Description,
                    Condition = equipmentDto.Condition,
                    SiteId = equipmentDto.SiteId,
                    UserId = equipmentDto.UserId,
                    Active = equipmentDto.Active,
                };

                var result = await _equipmentService.UpdateAsync(equipmentUpdateDto, _session.UserId);
                if (result.Succeeded)
                {
                    // Move equipment from available to assigned
                    AssignedEquipment.Add(equipmentDto);
                    AvailableEquipment.Remove(equipmentDto);
                    MessageBox.Show("Equipment assigned to site successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var errors = string.Join("\n", result.Errors);
                    MessageBox.Show($"Failed to assign equipment:\n{errors}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning equipment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UnassignEquipmentAsync()
        {
            if (SelectedAssignedEquipment == null) return;

            IsBusy = true;
            try
            {
                var equipmentDto = SelectedAssignedEquipment;
                equipmentDto.SiteId = null;

                //EquipmentCreateDto equipmenCreateDto = new EquipmentCreateDto
                //{
                //    EquipmentId = equipmentDto.EquipmentId,
                //    SerialNumber = equipmentDto.SerialNumber,
                //    Description = equipmentDto.Description,
                //    Condition = equipmentDto.Condition,
                //    SiteId = equipmentDto.SiteId,
                //    UserId = equipmentDto.UserId,
                //    Active = equipmentDto.Active,
                //};

                EquipmentUpdateDto equipmentUpdateDto = new EquipmentUpdateDto
                {
                    EquipmentId = equipmentDto.EquipmentId,
                    SerialNumber = equipmentDto.SerialNumber,
                    Description = equipmentDto.Description,
                    Condition = equipmentDto.Condition,
                    SiteId = equipmentDto.SiteId,
                    UserId = equipmentDto.UserId,
                    Active = equipmentDto.Active,
                };

                var result = await _equipmentService.UpdateAsync(equipmentUpdateDto, _session.UserId);
                if (result.Succeeded)
                {
                    // Move equipment from assigned to available
                    AvailableEquipment.Add(equipmentDto);
                    AssignedEquipment.Remove(equipmentDto);
                    MessageBox.Show("Equipment unassigned from site successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var errors = string.Join("\n", result.Errors);
                    MessageBox.Show($"Failed to unassign equipment:\n{errors}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error unassigning equipment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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