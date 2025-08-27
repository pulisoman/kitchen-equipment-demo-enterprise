using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Sites;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.Data.Models;
using KitchenEquipmentDemo.Enterprise.WPF.Commands;
using KitchenEquipmentDemo.Enterprise.WPF.Models;
using KitchenEquipmentDemo.Enterprise.WPF.Services.Navigation;
using KitchenEquipmentDemo.Enterprise.WPF.ViewModels.Base;

namespace KitchenEquipmentDemo.Enterprise.WPF.ViewModels
{
    public class SitesViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly ISiteService _mainService;
        private readonly AuthSession _session;
        private readonly INavigationService _nav;

        private string _screenName;
        private bool _isBusy;
        private string _queryText;
        private int _pageSize;
        private string _sortBy;
        private ObservableCollection<SiteDto> _sites;
        private ObservableCollection<int> _pageSizeOptions;
        private ObservableCollection<string> _sortFields;
        private int _currentPage = 1;
        private int _total;
        private int _totalPages;
        private SiteDto _selectedSite;

        public SitesViewModel(ISiteService mainService, AuthSession session, INavigationService navigationService)
        {
            _mainService = mainService;
            _session = session;
            _nav = navigationService;

            // Initialize commands
            SearchCommand = new AsyncRelayCommand(SearchAsync, () => !IsBusy);
            NextPageCommand = new AsyncRelayCommand(NextPageAsync, () => !IsBusy && HasNextPage);
            PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync, () => !IsBusy && HasPreviousPage);
            EditCommand = new RelayCommand<SiteDto>(Edit, (site) => !IsBusy && site != null);
            DeleteCommand = new AsyncRelayCommand<SiteDto>(DeleteAsync, (site) => !IsBusy && site != null);
            AddCommand = new RelayCommand(Add, () => !IsBusy);

            // Initialize collections
            IsBusy = true;
            InitializeCollections();
            IsBusy = false;
            _ = LoadInitialDataAsync();
        }

        private void InitializeCollections()
        {
            ScreenName = "Sites Management";

            // Page size options
            PageSizeOptions = new ObservableCollection<int> { 10, 25, 50, 100 };

            // Sort fields
            SortFields = new ObservableCollection<string>
        {
            "SiteId",
            "Code",
            "Name",
            "Description",
            "Active",
            "UserId",
            "CreatedAt"
        };

            // Default values
            PageSize = 25;
            SortBy = "SiteId";
            Sites = new ObservableCollection<SiteDto>();

            // Initialize filters as null (show all)
            ActiveFilter = null;
            OwnerFilter = null;
        }

        private bool? _activeFilter;
        private int? _ownerFilter;

        public bool? ActiveFilter
        {
            get => _activeFilter;
            set
            {
                _activeFilter = value;
                OnPropertyChanged(nameof(ActiveFilter));
                _ = SearchAsync(); // Auto-search when filter changes
            }
        }

        public int? OwnerFilter
        {
            get => _ownerFilter;
            set
            {
                _ownerFilter = value;
                OnPropertyChanged(nameof(OwnerFilter));
                _ = SearchAsync(); // Auto-search when filter changes
            }
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

        public ObservableCollection<SiteDto> Sites
        {
            get => _sites;
            set
            {
                _sites = value;
                OnPropertyChanged(nameof(Sites));
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
                (EditCommand as RelayCommand<SiteDto>)?.RaiseCanExecuteChanged();
                (DeleteCommand as AsyncRelayCommand<SiteDto>)?.RaiseCanExecuteChanged();
                (AddCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public SiteDto SelectedSite
        {
            get => _selectedSite;
            set
            {
                _selectedSite = value;
                OnPropertyChanged(nameof(SelectedSite));
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
                    activeStatus: ActiveFilter,
                    ownerId: _session.UserId,
                    actorUserId: _session.UserId,
                    orderBy: SortBy
                );

                Sites = new ObservableCollection<SiteDto>(result.Items);
                Total = result.Total;
                TotalPages = result.TotalPages;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching sites: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void Edit(SiteDto site)
        {

            //_nav.Navigate<SiteFormViewModel>(site);
            //_nav.Navigate<EquipmentFormViewModel>(equipment);
            site.Action = "Edit";
            site.ScreenName = "Edit Site";
            _nav.Navigate<SiteFormViewModel>(site);
        }

        private void Add()
        {
            _nav.Navigate<SiteFormViewModel>(new SiteDto() { Action = "Add", ScreenName = "Add Equipment" });
        }

        private async Task DeleteAsync(SiteDto site)
        {
            var result = MessageBox.Show($"Are you sure you want to delete site: {site.Name}?",
                                        "Confirm Delete",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    await _mainService.DeleteAsync(site.SiteId, _session.UserId);
                    IsBusy = false;
                    await SearchAsync();
                    MessageBox.Show("Site deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting site: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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