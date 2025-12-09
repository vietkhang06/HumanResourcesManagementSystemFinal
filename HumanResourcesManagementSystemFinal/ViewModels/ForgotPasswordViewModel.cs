using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Net.Mail;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class ForgotPasswordViewModel : ObservableObject
{
    public Action? NavigateToLoginAction { get; set; }

    public ForgotPasswordViewModel() { }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string _emailAddress = string.Empty;

    [ObservableProperty]
    private string _cccd = string.Empty;

    [RelayCommand]
    private void SwitchToLogin()
    {
        NavigateToLoginAction?.Invoke();
    }

    [RelayCommand(CanExecute = nameof(CanSubmitRequest))]
    private void SubmitRequest()
    {
        bool accountExists = CheckAccountExistence();

        if (accountExists)
        {
            MessageBox.Show(
                "Yêu cầu đặt lại mật khẩu đã được gửi. Vui lòng kiểm tra phương thức xác thực đã đăng ký.",
                "Thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            SwitchToLogin();
        }
        else
        {
            MessageBox.Show(
                "Thông tin xác thực không khớp với bất kỳ tài khoản nào.",
                "Lỗi Xác thực",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private bool CheckAccountExistence()
    {
        if (!string.IsNullOrWhiteSpace(Username) && Username.Equals("testuser", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(EmailAddress) && EmailAddress.Equals("test@example.com", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    // Điều kiện để kích hoạt nút "GỬI YÊU CẦU"
    private bool CanSubmitRequest()
    {
        return !string.IsNullOrWhiteSpace(Username) ||
               !string.IsNullOrWhiteSpace(PhoneNumber) ||
               !string.IsNullOrWhiteSpace(EmailAddress) ||
               !string.IsNullOrWhiteSpace(Cccd);
    }
}