using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace KitchenEquipmentDemo.Enterprise.WPF.Services.Navigation
{
    public sealed class NavigationService : INavigationService, INotifyPropertyChanged
    {
        private readonly IServiceProvider _provider;
        private object _previousViewModel;
        private object _currentViewModel;
        private object _navigationParameter;

        public NavigationService(IServiceProvider provider) => _provider = provider;

        public object PreviousViewModel
        {
            get => _previousViewModel;
            private set
            {
                _previousViewModel = value;
                OnPropertyChanged();
            }
        }
        public object CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                if (Equals(_currentViewModel, value)) return;
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public object NavigationParameter
        {
            get => _navigationParameter;
            private set
            {
                if (Equals(_navigationParameter, value)) return;
                _navigationParameter = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Navigate<TViewModel>(object parameter = null) where TViewModel : class
        {
            NavigationParameter = parameter;
            PreviousViewModel = CurrentViewModel;
            CurrentViewModel = _provider.GetRequiredService<TViewModel>();
        }

        public void Navigate(object viewModel, object parameter = null)
        {
            NavigationParameter = parameter;
            PreviousViewModel = CurrentViewModel;
            CurrentViewModel = viewModel;
        }

        public void NavigateBack()
        {
            if (PreviousViewModel != null)
            {
                // Get the type of the previous ViewModel
                Type previousType = PreviousViewModel.GetType();

                // Use reflection to call Navigate<T> with the correct type
                var method = this.GetType().GetMethod("Navigate", new[] { typeof(object) });
                var genericMethod = method.MakeGenericMethod(previousType);
                genericMethod.Invoke(this, new object[] { null });
            }
        }

        public void Clear()
        {
            PreviousViewModel = null;
            NavigationParameter = null;
            CurrentViewModel = null;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}