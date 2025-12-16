using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;
using HumanResourcesManagementSystemFinal.Helpers;
using System.Security.Cryptography;
using System.Windows.Input;
using HumanResourcesManagementSystemFinal.Others;

namespace HumanResourcesManagementSystemFinal.ViewModels
{

    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _password = string.Empty;

        // CHỈ CẦN MỘT KHAI BÁO CHO TÍNH NĂNG HIDE/SHOW
        [ObservableProperty]
        private bool _isPasswordVisible = false;

        [ObservableProperty]
        private bool _isRememberMeChecked = false;

        // KHAI BÁO ACTION CHỈ MỘT LẦN
        public Action? NavigateToForgotPasswordAction { get; set; }

        [ObservableProperty]
        private string _username = string.Empty;

        public LoginViewModel()
        {
            LoadSettings();
        }

        // TẠO COMMAND ĐỂ BẬT/TẮT MẬT KHẨU
        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        // TẢI CÀI ĐẶT
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

        // LƯU CÀI ĐẶT
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


        // LOGIN COMMAND
        [RelayCommand]
        private async Task Login()
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
                  .FirstOrDefaultAsync(u => u.Username == Username && u.PasswordHash == password);

                if (account != null)
                {
                    SaveSettings(password);

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

        // NAVIGATE TO FORGOT PASSWORD COMMAND
        [RelayCommand]
        private void NavigateToForgotPassword()
        {
            NavigateToForgotPasswordAction?.Invoke();
        }

        // EXIT COMMAND
        [RelayCommand]
        private void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}