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
    public class EquipmentsViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly IEquipmentService _mainService;
        private readonly AuthSession _session;
        private readonly INavigationService _nav;

        private string _screenName;
        private bool _isBusy;
        private string _queryText;
        private int _pageSize;
        private string _sortBy;
        private ObservableCollection<EquipmentDto> _equipments;
        private ObservableCollection<int> _pageSizeOptions;
        private ObservableCollection<string> _sortFields;
        private int _currentPage = 1;
        private int _total;
        private int _totalPages;
        private EquipmentDto _selectedEquipment;

        public EquipmentsViewModel(IEquipmentService mainService, AuthSession session, INavigationService navigationService)
        {
            _mainService = mainService;
            _session = session;
            _nav = navigationService;

            // Initialize commands
            SearchCommand = new AsyncRelayCommand(SearchAsync, () => !IsBusy);
            NextPageCommand = new AsyncRelayCommand(NextPageAsync, () => !IsBusy && HasNextPage);
            PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync, () => !IsBusy && HasPreviousPage);
            EditCommand = new RelayCommand<EquipmentDto>(Edit, (equipment) => !IsBusy && equipment != null);
            DeleteCommand = new AsyncRelayCommand<EquipmentDto>(DeleteAsync, (equipment) => !IsBusy && equipment != null);
            AddCommand = new RelayCommand(Add, () => !IsBusy);

            // Initialize collections
            IsBusy = true;
            InitializeCollections();
            IsBusy = false;
            _ = LoadInitialDataAsync();
        }

        private void InitializeCollections()
        {
            ScreenName = "Equipment Management";
            // Page size options
            PageSizeOptions = new ObservableCollection<int> { 10, 25, 50, 100 };

            // Sort fields - updated based on EquipmentDto properties
            SortFields = new ObservableCollection<string>
            {
                "EquipmentId",
                "SerialNumber",
                "Description",
                "Condition",
                "SiteId",
                "UserId"
            };

            // Default values
            PageSize = 25;
            SortBy = "EquipmentId";
            Equipments = new ObservableCollection<EquipmentDto>();
        }

        public string ScreenName
        {
            get => _screenName;
            set
            {
                _screenName = value;
                OnPropertyChanged(nameof(ScreenName));
            }
        }

        public string QueryText
        {
            get => _queryText;
            set
            {
                _queryText = value;
                OnPropertyChanged(nameof(QueryText));
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                OnPropertyChanged(nameof(PageSize));
                _ = SearchAsync();
            }
        }

        public string SortBy
        {
            get => _sortBy;
            set
            {
                _sortBy = value;
                OnPropertyChanged(nameof(SortBy));
                _ = SearchAsync();
            }
        }

        public ObservableCollection<EquipmentDto> Equipments
        {
            get => _equipments;
            set
            {
                _equipments = value;
                OnPropertyChanged(nameof(Equipments));
            }
        }

        public ObservableCollection<int> PageSizeOptions
        {
            get => _pageSizeOptions;
            set
            {
                _pageSizeOptions = value;
                OnPropertyChanged(nameof(PageSizeOptions));
            }
        }

        public ObservableCollection<string> SortFields
        {
            get => _sortFields;
            set
            {
                _sortFields = value;
                OnPropertyChanged(nameof(SortFields));
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
                OnPropertyChanged(nameof(HasNextPage));
                OnPropertyChanged(nameof(HasPreviousPage));
            }
        }

        public int Total
        {
            get => _total;
            set
            {
                _total = value;
                OnPropertyChanged(nameof(Total));
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(HasNextPage));
                OnPropertyChanged(nameof(HasPreviousPage));
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                (SearchCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (NextPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (PreviousPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (EditCommand as RelayCommand<EquipmentDto>)?.RaiseCanExecuteChanged();
                (DeleteCommand as AsyncRelayCommand<EquipmentDto>)?.RaiseCanExecuteChanged();
                (AddCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public EquipmentDto SelectedEquipment
        {
            get => _selectedEquipment;
            set
            {
                _selectedEquipment = value;
                OnPropertyChanged(nameof(SelectedEquipment));
            }
        }

        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;

        // Commands
        public ICommand SearchCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddCommand { get; }

        private async Task SearchAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;

                var result = await _mainService.GetPagedAsync(
                    page: CurrentPage,
                    pageSize: PageSize,
                    searchString: QueryText,
                    ownerId: _session.UserId,
                    actorUserId: _session.UserId,
                    orderBy: SortBy
                );

                Equipments = new ObservableCollection<EquipmentDto>(result.Items);
                Total = result.Total;
                TotalPages = result.TotalPages;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching equipment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task NextPageAsync()
        {
            CurrentPage++;
            await SearchAsync();
        }

        private async Task PreviousPageAsync()
        {
            CurrentPage--;
            await SearchAsync();
        }

        private void Edit(EquipmentDto equipment)
        {
            //_nav.Navigate<EquipmentFormViewModel>(equipment);
            equipment.Action = "Edit";
            equipment.ScreenName = "Edit Equipment";
            _nav.Navigate<EquipmentFormViewModel>(equipment);
        }

        private void Add()
        {
            _nav.Navigate<EquipmentFormViewModel>(new EquipmentDto() { Action ="Add", ScreenName="Add Equipment"});
        }

        private async Task DeleteAsync(EquipmentDto equipment)
        {
            var result = MessageBox.Show($"Are you sure you want to delete equipment: {equipment.Description}?",
                                        "Confirm Delete",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    await _mainService.DeleteAsync(equipment.EquipmentId, _session.UserId);
                    IsBusy = false;
                    await SearchAsync();
                    MessageBox.Show("Equipment deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting equipment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        public async Task LoadInitialDataAsync()
        {
            await SearchAsync();
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        protected new virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}