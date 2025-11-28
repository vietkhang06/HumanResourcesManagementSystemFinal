using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class ForgotPasswordViewModel : ObservableObject
{
    public Action? NavigateToLoginAction { get; set; }

    public ForgotPasswordViewModel() { }

    [ObservableProperty]
    private string _email = string.Empty;

    [RelayCommand]
    private void SwitchToLogin()
    {
        NavigateToLoginAction?.Invoke();
    }

    [RelayCommand]
    private void SendResetLink()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            MessageBox.Show("Vui lòng nhập email!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        MessageBox.Show($"Đã gửi yêu cầu đến: {Email}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

        SwitchToLogin();
    }
}