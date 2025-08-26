using System.ComponentModel;

namespace KitchenEquipmentDemo.Enterprise.WPF.Services.Navigation
{
    public interface INavigationService : INotifyPropertyChanged
    {
        object PreviousViewModel { get; }
        object CurrentViewModel { get; }
        object NavigationParameter { get; }
        void Navigate<TViewModel>(object parameter = null) where TViewModel : class;
        void Navigate(object viewModel, object parameter = null);
        void NavigateBack();
        void Clear();
    }
}