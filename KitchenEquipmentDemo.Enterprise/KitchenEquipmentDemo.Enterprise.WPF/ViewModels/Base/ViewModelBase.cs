﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KitchenEquipmentDemo.Enterprise.WPF.ViewModels.Base
{
    /// <summary>
    /// Minimal MVVM base with INotifyPropertyChanged helper methods.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Raise([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            Raise(propertyName);
            return true;
        }
    }
}
