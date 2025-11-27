using System.Windows;
using System.Windows.Input;
using HumanResourcesManagementSystemFinal.ViewModels;

namespace HumanResourcesManagementSystemFinal.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();

        // Mở màn hình đăng nhập ngay khi chạy
        ShowLogin();
    }

    // --- HÀM 1: HIỂN THỊ MÀN HÌNH ĐĂNG NHẬP ---
    public void ShowLogin()
    {
        var loginVM = new LoginViewModel();

        // Sửa đoạn này:
        loginVM.NavigateToForgotPasswordAction = () =>
        {
            // 2. Nếu hiện bảng Bước 1 mà KHÔNG hiện bảng này -> Lỗi kết nối Action
            MessageBox.Show("Bước 2: Window đang chuyển trang!", "Debug");
            ShowForgotPassword();
        };

        var loginView = new LoginControl();
        loginView.DataContext = loginVM;
        MainFrame.Content = loginView;
    }

    // --- HÀM 2: HIỂN THỊ MÀN HÌNH QUÊN MẬT KHẨU ---
    public void ShowForgotPassword()
    {
        // 1. Tạo ViewModel
        var forgotVM = new ForgotPasswordViewModel();

        // 2. Gán sự kiện: Khi ViewModel bảo "Quay lại", thì chạy hàm ShowLogin
        forgotVM.NavigateToLoginAction = () => ShowLogin();

        // 3. Tạo View và gán DataContext
        var forgotView = new ForgotPasswordControl();
        forgotView.DataContext = forgotVM;

        // 4. Đưa vào màn hình chính
        MainFrame.Content = forgotView;
    }

    // --- CÁC SỰ KIỆN CƠ BẢN ---
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        this.DragMove();
    }
}