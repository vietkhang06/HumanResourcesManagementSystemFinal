using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Views;
using HumanResourcesManagementSystemFinal.Models; // Cần thêm namespace Models
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
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

            // Load Account kèm Employee
            var account = await context.Accounts
                .Include(a => a.Role)
                .Include(a => a.Employee).ThenInclude(e => e.Position)
                .Include(a => a.Employee).ThenInclude(e => e.Department)
                .FirstOrDefaultAsync(u => u.Username == Username && u.PasswordHash == password);

            if (account != null)
            {
                if (account.IsActive == false)
                {
                    MessageBox.Show("Tài khoản này đã bị vô hiệu hóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // === ĐOẠN SỬA QUAN TRỌNG ĐỂ CỨU ADMIN ===
                var loggedInUser = account.Employee;

                // Nếu là Admin mà chưa có thông tin nhân viên (Employee bị null)
                // Ta tạo một nhân viên "giả" để MainViewModel không bị lỗi
                if (loggedInUser == null && account.Role.RoleName == "Admin")
                {
                    loggedInUser = new Employee
                    {
                        Id = 0, // ID giả
                        FirstName = "System",
                        LastName = "Administrator",
                        Position = new Position { Title = "Quản trị viên hệ thống" },
                        Account = account // Gắn ngược lại account để MainViewModel dùng
                    };
                }
                // ==========================================

                if (loggedInUser != null)
                {
                    // Truyền nhân viên (thật hoặc giả) vào MainViewModel
                    var mainViewModel = new MainViewModel(loggedInUser);

                    var dashboard = new MainWindow();
                    dashboard.DataContext = mainViewModel;
                    dashboard.Show();

                    // Đóng Login Window
                    var openWindows = Application.Current.Windows.Cast<Window>().ToList();
                    foreach (var window in openWindows)
                    {
                        if (window != dashboard) window.Close();
                    }
                }
                else
                {
                    // Trường hợp tài khoản nhân viên thường nhưng dữ liệu bị lỗi (mất liên kết)
                    MessageBox.Show("Lỗi dữ liệu: Tài khoản này không liên kết với hồ sơ nhân viên nào!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu!", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
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