using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Views;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class ForgotPasswordViewModel : ObservableObject
{
    // Biến tham chiếu đến ViewModel cha để thực hiện chuyển trang
    private readonly LoginWindow _mainViewModel;

    // Constructor nhận ViewModel cha
    public ForgotPasswordViewModel(LoginWindow mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    // Constructor mặc định để tránh lỗi Design-time (nếu cần)
    public ForgotPasswordViewModel() { }

    // Biến lưu Email người dùng nhập
    [ObservableProperty]
    private string _email = string.Empty;

    // --- LỆNH: QUAY LẠI ĐĂNG NHẬP ---
    [RelayCommand]
    private void SwitchToLogin()
    {
        // Gọi hàm của cha để chuyển về màn hình Login
        if (_mainViewModel != null)
        {
            _mainViewModel.ShowLoginView();
        }
    }

    // --- LỆNH: GỬI YÊU CẦU RESET MẬT KHẨU ---
    [RelayCommand]
    private void SendResetLink()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            MessageBox.Show("Vui lòng nhập email!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // TODO: Thêm logic gửi email thật ở đây
        // Ví dụ: Kiểm tra email có tồn tại trong DB không, sau đó gửi mã OTP...

        MessageBox.Show($"Đã gửi yêu cầu đặt lại mật khẩu đến: {Email}\nVui lòng kiểm tra hộp thư.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

        // Sau khi gửi xong, có thể tự động quay lại màn hình đăng nhập
        SwitchToLogin();
    }
}