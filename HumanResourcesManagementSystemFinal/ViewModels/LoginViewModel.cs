using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Helpers;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Others;
using HumanResourcesManagementSystemFinal.ViewModels;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using System.Text;
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
        [ObservableProperty]
        private string _username = string.Empty;
        public Action? NavigateToForgotPasswordAction { get; set; }

        public LoginViewModel()
        {
            LoadSettings();
        }

        private string GetDeepErrorMessage(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(ex.Message);
            var inner = ex.InnerException;
            while (inner != null)
            {
                sb.AppendLine(inner.Message);
                inner = inner.InnerException;
            }
            return sb.ToString();
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
                    try
                    {
                        Password = DataProtectionHelper.Unprotect(encryptedPassword);
                    }
                    catch
                    {
                        Password = string.Empty;
                    }
                }
            }
        }

        private void SaveSettings(string rawPassword)
        {
            if (IsRememberMeChecked)
            {
                Settings.Default.SavedUsername = Username;
                Settings.Default.SavedPassword = DataProtectionHelper.Protect(rawPassword);
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
        private async Task LoginAsync(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new DataContext();

                var account = await context.Accounts
                    .AsNoTracking()
                    .Include(a => a.Role)
                    .Include(a => a.Employee)
                        .ThenInclude(e => e.Department)
                    .Include(a => a.Employee)
                        .ThenInclude(e => e.Position)
                    .FirstOrDefaultAsync(u => u.Username == Username && u.PasswordHash == Password);

                if (account == null)
                {
                    MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu!", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!account.IsActive)
                {
                    MessageBox.Show("Tài khoản đã bị khóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AppSession.CurrentUser = account.Employee;
                AppSession.CurrentRole = account.Role?.RoleName;
                UserSession.CurrentEmployeeId = account.EmployeeId ?? 0;
                UserSession.CurrentRole = account.Role?.RoleName;

                SaveSettings(Password);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var mainViewModel = new MainViewModel(account);
                    var dashboard = new MainWindow
                    {
                        DataContext = mainViewModel
                    };
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
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hệ thống:\n{GetDeepErrorMessage(ex)}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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