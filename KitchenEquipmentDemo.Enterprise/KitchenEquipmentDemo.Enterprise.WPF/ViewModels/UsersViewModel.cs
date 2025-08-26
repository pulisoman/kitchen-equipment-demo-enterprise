using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
    public class UsersViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly IUserService _mainService;
        private readonly AuthSession _session;
        private readonly INavigationService _nav;

        private string _screenName;
        private bool _isBusy;
        private string _queryText;
        private int _pageSize;
        private string _sortBy;
        private ObservableCollection<UserDto> _users;
        private ObservableCollection<int> _pageSizeOptions;
        private ObservableCollection<string> _sortFields;
        private int _currentPage = 1;
        private int _total;
        private int _totalPages;
        private UserDto _selectedUser;

        public UsersViewModel(IUserService mainService, AuthSession session, INavigationService navigationService)
        {
            _mainService = mainService;
            _session = session;
            _nav = navigationService;

            // Initialize commands
            SearchCommand = new AsyncRelayCommand(SearchAsync, () => !IsBusy);
            NextPageCommand = new AsyncRelayCommand(NextPageAsync, () => !IsBusy && HasNextPage);
            PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync, () => !IsBusy && HasPreviousPage);
            EditCommand = new RelayCommand<UserDto>(Edit, (user) => !IsBusy && user != null);
            DeleteCommand = new AsyncRelayCommand<UserDto>(DeleteAsync, (user) => !IsBusy && user != null);

            // Initialize collections
            IsBusy = true;
            InitializeCollections();
            IsBusy = false;
            _ = LoadInitialDataAsync();
        }

        private void InitializeCollections()
        {
            ScreenName = "Users Management";
            // Page size options
            PageSizeOptions = new ObservableCollection<int> { 10, 25, 50, 100 };

            // Sort fields - adjust based on your UserDto properties
            SortFields = new ObservableCollection<string>
            {
                "UserId",
                "UserName",
                "FirstName",
                "LastName",
                "EmailAddress"
            };

            // Default values
            PageSize = 25;
            SortBy = "UserId";
            Users = new ObservableCollection<UserDto>();
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

        // Properties
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
                // Refresh data when page size changes
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
                // Refresh data when sort field changes
                _ = SearchAsync();
            }
        }

        public ObservableCollection<UserDto> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged(nameof(Users));
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
                // Update command availability when busy state changes
                (SearchCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (NextPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (PreviousPageCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (EditCommand as RelayCommand<UserDto>)?.RaiseCanExecuteChanged();
                (DeleteCommand as AsyncRelayCommand<UserDto>)?.RaiseCanExecuteChanged();
            }
        }

        public UserDto SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
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

        // Methods
        private async Task SearchAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;

                // Call the user service to get paged users
                var result = await _mainService.GetPagedAsync(
                    page: CurrentPage,
                    pageSize: PageSize,
                    searchString: QueryText,
                    showDeletedOnly: false, 
                    actorUserId: _session.UserId,
                    orderBy: SortBy
                );

                // Update the users collection
                Users = new ObservableCollection<UserDto>(result.Items);
                Total = result.Total;
                TotalPages = result.TotalPages;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void Edit(UserDto user)
        {
            _nav.Navigate<UserFormViewModel>(user);
        }

        private async Task DeleteAsync(UserDto user)
        {
            var result = MessageBox.Show($"Are you sure you want to delete user: {user.UserName}?",
                                        "Confirm Delete",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    //Call your service to delete the user
                    await _mainService.DeleteAsync(user.UserId, _session.UserId);
                    IsBusy = false;
                    await SearchAsync(); // Refresh the list
                    MessageBox.Show("User deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        // Load initial data
        public async Task LoadInitialDataAsync()
        {
            await SearchAsync();
        }

        // INotifyPropertyChanged implementation
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}