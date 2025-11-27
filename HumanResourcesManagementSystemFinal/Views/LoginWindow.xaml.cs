using System.Windows;
using System.Windows.Input;
using HumanResourcesManagementSystemFinal.ViewModels;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            // Mặc định khi mở lên sẽ hiện màn hình Đăng nhập
            ShowLoginView();
        }

        // --- HÀM CHUYỂN TRANG (Sửa lỗi CS1061) ---

        public void ShowLoginView()
        {
            // Hiển thị UserControl Login
            var view = new LoginControl();
            // Gán ViewModel mới hoặc dùng lại cái cũ tùy logic của bạn
            view.DataContext = new LoginViewModel();
            MainContent.Content = view;
        }

        public void ShowForgotPasswordView()
        {
            // Hiển thị UserControl Quên mật khẩu
            var view = new ForgotPasswordControl();
            view.DataContext = new ForgotPasswordViewModel();
            MainContent.Content = view;
        }

        // --- SỰ KIỆN GIAO DIỆN ---

        // Xử lý nút Đóng (Sửa lỗi CS1061 CloseButton_Click)
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Xử lý kéo thả cửa sổ
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}