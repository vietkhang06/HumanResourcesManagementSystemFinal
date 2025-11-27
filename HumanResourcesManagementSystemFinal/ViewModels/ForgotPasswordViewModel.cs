using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class ForgotPasswordViewModel : ObservableObject
{
    // --- NAVIGATOR ---
    public Action? NavigateToLoginAction { get; set; }

    // Constructor chuẩn, không cần tham số LoginViewModel nữa
    public ForgotPasswordViewModel() { }

    [ObservableProperty]
    private string _email = string.Empty;

    // --- LỆNH: QUAY LẠI ĐĂNG NHẬP ---
    [RelayCommand]
    private void SwitchToLogin()
    {
        // Báo hiệu cho cha biết là muốn quay về
        NavigateToLoginAction?.Invoke();
    }

    // --- LỆNH: GỬI YÊU CẦU ---
    [RelayCommand]
    private void SendResetLink()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            MessageBox.Show("Vui lòng nhập email!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Logic gửi mail giả lập
        MessageBox.Show($"Đã gửi yêu cầu đến: {Email}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

        // Gửi xong thì tự động quay về đăng nhập
        SwitchToLogin();
    }
}