using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Helpers;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Others;
using HumanResourcesManagementSystemFinal.Services;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _isPasswordVisible = false;

        [ObservableProperty]
        private bool _isRememberMeChecked = false;

        public Action? NavigateToForgotPasswordAction { get; set; }

        [ObservableProperty]
        private string _username = string.Empty;

        public LoginViewModel()
        {
            LoadSettings();
        }

        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private void LoadSettings()
        {
            if (Settings.Default.RememberMe)
            {
                IsRememberMeChecked = true;
                Username = Settings.Default.SavedUsername ?? string.Empty;

                string encryptedPassword = Settings.Default.SavedPassword ?? string.Empty;
                if (!string.IsNullOrEmpty(encryptedPassword))
                {
                    Password = DataProtectionHelper.Unprotect(encryptedPassword);
                }
            }
        }

        private void SaveSettings(string rawPassword)
        {
            if (IsRememberMeChecked)
            {
                Settings.Default.SavedUsername = Username;
                string encryptedPassword = DataProtectionHelper.Protect(rawPassword);
                Settings.Default.SavedPassword = encryptedPassword;
                Settings.Default.RememberMe = true;
            }
            else
            {
                Settings.Default.SavedUsername = string.Empty;
                Settings.Default.SavedPassword = string.Empty;
                Settings.Default.RememberMe = false;
            }
            Settings.Default.Save();
        }

        [RelayCommand]
        private async Task Login(object parameter)
        {
            string password = Password;

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
                    .Include(a => a.Employee)
                        .ThenInclude(e => e.Department)
                    .Include(a => a.Employee)
                        .ThenInclude(e => e.Position)
                    .FirstOrDefaultAsync(u => u.Username == Username && u.PasswordHash == password);
                AppSession.CurrentUser = account.Employee; // Lưu nhân viên
                AppSession.CurrentRole = account.Role?.RoleName;     // Lưu quyền từ Account

                if (account != null)
                {
                    if (account.IsActive)
                    {
                        UserSession.CurrentEmployeeId = account.EmployeeId ?? 0;
                        UserSession.CurrentRole = account.Role?.RoleName;

                        SaveSettings(password);

                        var mainViewModel = new MainViewModel(account);
                        var dashboard = new MainWindow();
                        dashboard.DataContext = mainViewModel;
                        dashboard.Show();

                        if (parameter is Window loginWindow)
                        {
                            loginWindow.Close();
                        }
                        else
                        {
                            var openWindows = Application.Current.Windows.Cast<Window>().ToList();
                            foreach (var window in openWindows)
                            {
                                if (window.GetType().Name != "MainWindow")
                                {
                                    window.Close();
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Tài khoản đã bị khóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
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
}