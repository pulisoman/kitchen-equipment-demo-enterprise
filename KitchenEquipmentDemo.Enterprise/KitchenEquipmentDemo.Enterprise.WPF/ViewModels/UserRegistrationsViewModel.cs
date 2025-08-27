using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.UserRegistrations.KitchenEquipmentDemo.Enterprise.WPF.Dtos;
using KitchenEquipmentDemo.Enterprise.Application.Contracts.Services;
using KitchenEquipmentDemo.Enterprise.Data.Models;
using KitchenEquipmentDemo.Enterprise.WPF.Commands;
using KitchenEquipmentDemo.Enterprise.WPF.Models;
using KitchenEquipmentDemo.Enterprise.WPF.Services.Navigation;
using KitchenEquipmentDemo.Enterprise.WPF.ViewModels.Base;

namespace KitchenEquipmentDemo.Enterprise.WPF.ViewModels
{
    public class UserRegistrationsViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly IUserRegistrationService _mainService;
        private readonly AuthSession _session;
        private readonly INavigationService _nav;

        private string _screenName;
        private bool _isBusy;
        private string _queryText;
        private int _pageSize;
        private string _sortBy;
        private string _status;
        private ObservableCollection<UserRegistrationDto> _requests;
        private ObservableCollection<int> _pageSizeOptions;
        private ObservableCollection<string> _sortFields;
        private ObservableCollection<string> _statusFields;
        private int _currentPage = 1;
        private int _total;
        private int _totalPages;
        private UserRegistrationDto _selectedRequest;

        public UserRegistrationsViewModel(IUserRegistrationService mainService, AuthSession session, INavigationService navigationService)
        {
            _mainService = mainService;
            _session = session;
            _nav = navigationService;

            // Initialize commands
            SearchCommand = new AsyncRelayCommand(SearchAsync, () => !IsBusy);
            NextPageCommand = new AsyncRelayCommand(NextPageAsync, () => !IsBusy && HasNextPage);
            PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync, () => !IsBusy && HasPreviousPage);
            ApproveCommand = new AsyncRelayCommand<UserRegistrationDto>(ApproveAsync, (request) => !IsBusy && request != null);
            RejectCommand = new AsyncRelayCommand<UserRegistrationDto>(RejectAsync, (request) => !IsBusy && request != null);

            // Initialize collections
            IsBusy = true;
            InitializeCollections();
            IsBusy = false;
            _ = LoadInitialDataAsync();
        }

        private void InitializeCollections()
        {
            ScreenName = "User Registration Requests";
            // Page size options
            PageSizeOptions = new ObservableCollection<int> { 10, 25, 50, 100 };

            // Sort fields - adjust based on your UserRegistrationRequestDto properties
            SortFields = new ObservableCollection<string>
            {
                "RequestId",
                "UserName",
                "FirstName",
                "LastName",
                "EmailAddress",
                "CreatedAt",
                "Status"
            };

            StatusFields = new ObservableCollection<string>
            {
                "All",
                "Pending",
                "Approved",
                "Denied",
            };

            // Default values
            PageSize = 25;
            SortBy = "RequestId";
            Status = "Pending";
            Requests = new ObservableCollection<UserRegistrationDto>();
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

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                // Refresh data when sort field changes
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

        public ObservableCollection<UserRegistrationDto> Requests
        {
            get => _requests;
            set
            {
                _requests = value;
                OnPropertyChanged(nameof(Requests));
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

        public ObservableCollection<string> StatusFields
        {
            get => _statusFields;
            set
            {
                _statusFields = value;
                OnPropertyChanged(nameof(StatusFields));
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
                (ApproveCommand as AsyncRelayCommand<UserRegistrationDto>)?.RaiseCanExecuteChanged();
                (RejectCommand as AsyncRelayCommand<UserRegistrationDto>)?.RaiseCanExecuteChanged();
            }
        }

        public UserRegistrationDto SelectedRequest
        {
            get => _selectedRequest;
            set
            {
                _selectedRequest = value;
                OnPropertyChanged(nameof(SelectedRequest));
            }
        }

        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;

        // Commands
        public ICommand SearchCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }

        // Methods
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
                    status: Status,
                    actorUserId: _session.UserId,
                    orderBy: SortBy
                );

                // Update the requests collection
                Requests = new ObservableCollection<UserRegistrationDto>(result.Items);
                Total = result.Total;
                TotalPages = result.TotalPages;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching registration requests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async Task ApproveAsync(UserRegistrationDto request)
        {
            var result = MessageBox.Show($"Are you sure you want to approve registration request for: {request.UserName}?",
                                        "Confirm Approval",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    // Call your service to approve the request
                    await _mainService.ApproveAsync(request.RequestId, _session.UserId);
                    IsBusy = false;
                    await SearchAsync(); // Refresh the list
                    MessageBox.Show("Registration request approved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error approving request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task RejectAsync(UserRegistrationDto request)
        {
            var result = MessageBox.Show($"Are you sure you want to reject registration request for: {request.UserName}?",
                                        "Confirm Rejection",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    // Call your service to reject the request
                    await _mainService.DenyAsync(request.RequestId, _session.UserId, "Rejected by administrator");
                    IsBusy = false;
                    await SearchAsync(); // Refresh the list
                    MessageBox.Show("Registration request rejected successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error rejecting request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

    // DTO for UserRegistrationRequest (you might need to create this in your contracts)
    
}