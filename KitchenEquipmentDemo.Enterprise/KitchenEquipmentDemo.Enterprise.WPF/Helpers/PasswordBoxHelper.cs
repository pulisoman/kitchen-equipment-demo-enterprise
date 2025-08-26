using System.Windows;
using System.Windows.Controls;

namespace KitchenEquipmentDemo.Enterprise.WPF.Helpers
{
    /// <summary>
    /// Minimal attached behavior to bind PasswordBox.Password as a string.
    /// For production, consider SecureString + hashing on the service side.
    /// </summary>
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BindPassword",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false, OnBindPasswordChanged));

        private static readonly DependencyProperty UpdatingPasswordProperty =
            DependencyProperty.RegisterAttached(
                "UpdatingPassword",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false));

        public static string GetBoundPassword(DependencyObject d) => (string)d.GetValue(BoundPasswordProperty);
        public static void SetBoundPassword(DependencyObject d, string value) => d.SetValue(BoundPasswordProperty, value);

        public static bool GetBindPassword(DependencyObject d) => (bool)d.GetValue(BindPasswordProperty);
        public static void SetBindPassword(DependencyObject d, bool value) => d.SetValue(BindPasswordProperty, value);

        private static bool GetUpdatingPassword(DependencyObject d) => (bool)d.GetValue(UpdatingPasswordProperty);
        private static void SetUpdatingPassword(DependencyObject d, bool value) => d.SetValue(UpdatingPasswordProperty, value);

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox pb)
            {
                pb.PasswordChanged -= HandlePasswordChanged;

                if (!GetUpdatingPassword(pb))
                    pb.Password = e.NewValue as string ?? string.Empty;

                pb.PasswordChanged += HandlePasswordChanged;
            }
        }

        private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox pb)
            {
                if ((bool)e.OldValue) pb.PasswordChanged -= HandlePasswordChanged;
                if ((bool)e.NewValue) pb.PasswordChanged += HandlePasswordChanged;
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = (PasswordBox)sender;
            SetUpdatingPassword(pb, true);
            SetBoundPassword(pb, pb.Password);
            SetUpdatingPassword(pb, false);
        }
    }
}
