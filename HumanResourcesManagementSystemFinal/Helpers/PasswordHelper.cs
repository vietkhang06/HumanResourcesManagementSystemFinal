using System.Windows;
using System.Windows.Controls;

namespace HumanResourcesManagementSystemFinal.Helpers
{
    public static class PasswordHelper
    {
        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BindPassword",
                typeof(bool),
                typeof(PasswordHelper),
                new PropertyMetadata(false, OnBindPasswordChanged));

        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordHelper),
                new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        private static readonly DependencyProperty UpdatingPasswordProperty =
            DependencyProperty.RegisterAttached(
                "UpdatingPassword",
                typeof(bool),
                typeof(PasswordHelper));

        public static bool GetBindPassword(DependencyObject dp) =>
            (bool)dp.GetValue(BindPasswordProperty);

        public static void SetBindPassword(DependencyObject dp, bool value) =>
            dp.SetValue(BindPasswordProperty, value);

        public static string GetBoundPassword(DependencyObject dp) =>
            (string)dp.GetValue(BoundPasswordProperty);

        public static void SetBoundPassword(DependencyObject dp, string value) =>
            dp.SetValue(BoundPasswordProperty, value);

        private static bool GetUpdatingPassword(DependencyObject dp) =>
            (bool)dp.GetValue(UpdatingPasswordProperty);

        private static void SetUpdatingPassword(DependencyObject dp, bool value) =>
            dp.SetValue(UpdatingPasswordProperty, value);

        private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is PasswordBox pb)
            {
                if ((bool)e.NewValue)
                    pb.PasswordChanged += HandlePasswordChanged;
                else
                    pb.PasswordChanged -= HandlePasswordChanged;
            }
        }

        private static void OnBoundPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is PasswordBox pb && !GetUpdatingPassword(pb))
            {
                pb.Password = e.NewValue?.ToString() ?? "";
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pb)
            {
                SetUpdatingPassword(pb, true);
                SetBoundPassword(pb, pb.Password);
                SetUpdatingPassword(pb, false);
            }
        }
    }
}
