using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq; // <--- Cần thêm cái này để dùng .ToList()
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    public Action? NavigateToForgotPasswordAction { get; set; }

    [ObservableProperty]
    private string _username = string.Empty;

    public LoginViewModel() { }

    [RelayCommand]
    private async Task Login(object parameter)
    {
        if (parameter is not PasswordBox passwordBox) return;
        string password = passwordBox.Password;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var context = new DataContext();
            var account = await context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(u => u.Username == Username && u.PasswordHash == password);

            if (account != null)
            {
                var mainViewModel = new MainViewModel(account);
                var dashboard = new MainWindow();
                dashboard.DataContext = mainViewModel;
                dashboard.Show();
                var openWindows = Application.Current.Windows.Cast<Window>().ToList();

                foreach (var window in openWindows)
                {
                    if (window != dashboard)
                    {
                        window.Close();
                    }
                }
            }
            else
            {
                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu!", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi kết nối Database: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void NavigateToForgotPassword()
    {
        NavigateToForgotPasswordAction?.Invoke();
    }

    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }
}