using System.Windows;
using System.Windows.Controls;

namespace HumanResourcesManagementSystemFinal.Helpers
{
    // Đây là phiên bản ổn định hơn để ngăn chặn vòng lặp ghi đè
    public static class PasswordHelper
    {
        // Attached Property 1: BOUNDPASSWORD (Chứa giá trị mật khẩu được bind)
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordHelper), new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        // Attached Property 2: BIND PASSWORD (Dùng để kích hoạt/ngừng kích hoạt sự kiện)
        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached("BindPassword", typeof(bool), typeof(PasswordHelper), new PropertyMetadata(false, OnBindPasswordChanged));

        // Attached Property 3: Internal để tránh vòng lặp
        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordHelper));


        // --- GETTERS/SETTERS CÔNG KHAI ---
        public static string GetBoundPassword(DependencyObject dp) => (string)dp.GetValue(BoundPasswordProperty);
        public static void SetBoundPassword(DependencyObject dp, string value) => dp.SetValue(BoundPasswordProperty, value);

        public static bool GetBindPassword(DependencyObject dp) => (bool)dp.GetValue(BindPasswordProperty);
        public static void SetBindPassword(DependencyObject dp, bool value) => dp.SetValue(BindPasswordProperty, value);

        // --- GETTERS/SETTERS NỘI BỘ ---
        private static bool GetIsUpdating(DependencyObject dp) => (bool)dp.GetValue(IsUpdatingProperty);
        private static void SetIsUpdating(DependencyObject dp, bool value) => dp.SetValue(IsUpdatingProperty, value);


        // --- LOGIC XỬ LÝ SỰ KIỆN ---

        private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = dp as PasswordBox;
            if (passwordBox == null) return;

            bool wasBound = (bool)e.OldValue;
            bool neededBinding = (bool)e.NewValue;

            if (wasBound)
            {
                // Hủy đăng ký sự kiện cũ
                passwordBox.PasswordChanged -= HandlePasswordChanged;
            }

            if (neededBinding)
            {
                // Đăng ký sự kiện mới
                passwordBox.PasswordChanged += HandlePasswordChanged;

                // Đồng bộ hóa giá trị ban đầu từ ViewModel sang PasswordBox
                UpdatePasswordBox(passwordBox, GetBoundPassword(passwordBox));
            }
        }

        private static void OnBoundPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = dp as PasswordBox;
            if (passwordBox == null) return;

            // Chỉ cập nhật PasswordBox nếu thay đổi không đến từ chính nó
            UpdatePasswordBox(passwordBox, (string)e.NewValue);
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox == null) return;

            SetIsUpdating(passwordBox, true);
            SetBoundPassword(passwordBox, passwordBox.Password); // Cập nhật ViewModel
            SetIsUpdating(passwordBox, false);
        }

        // Phương thức nội bộ để cập nhật PasswordBox
        private static void UpdatePasswordBox(PasswordBox passwordBox, string newPassword)
        {
            if (!GetIsUpdating(passwordBox) && passwordBox.Password != newPassword)
            {
                passwordBox.Password = newPassword;
            }
        }
    }
}