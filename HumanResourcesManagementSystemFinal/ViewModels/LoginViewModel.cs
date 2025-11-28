using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data; // Để dùng DataContext
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;           // Để dùng FirstOrDefaultAsync
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    // Action điều hướng (Giữ nguyên)
    public Action? NavigateToForgotPasswordAction { get; set; }

    [ObservableProperty]
    private string _username = string.Empty;

    public LoginViewModel() { }

    // --- HÀM ĐĂNG NHẬP (ĐÃ CẬP NHẬT KẾT NỐI DB) ---
    [RelayCommand]
    private async Task Login(object parameter)
    {
        // 1. Lấy mật khẩu từ PasswordBox (vì mật khẩu không Binding trực tiếp được)
        if (parameter is not PasswordBox passwordBox) return;
        string password = passwordBox.Password;

        // 2. Kiểm tra nhập liệu
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // 3. Kết nối Database tìm tài khoản
            // Dùng 'using' để tự đóng kết nối sau khi dùng xong
            using var context = new DataContext();

            // Tìm xem có ai trùng Username và Password không
            var account = await context.Accounts
                .Include(a => a.Role) // Kèm theo thông tin Quyền (Admin/Nhân viên)
                .FirstOrDefaultAsync(u => u.Username == Username && u.PasswordHash == password);

            // 4. Xử lý kết quả
            if (account != null)
            {
                // ==> ĐĂNG NHẬP THÀNH CÔNG <==

                // 1. Ẩn (hoặc đóng) cửa sổ Login hiện tại
                // Chúng ta cần tìm cửa sổ đang chứa ViewModel này để đóng nó
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.DataContext == this || window.GetType().Name == "LoginWindow")
                    {
                        window.Close();
                        break;
                    }
                }

                // 2. Mở Dashboard và truyền Account sang
                // (Lưu ý: Phải thêm using HumanResourcesManagementSystemFinal vào đầu file để thấy MainWindow)
                var mainViewModel = new MainViewModel(account);
                var dashboard = new MainWindow();
                dashboard.DataContext = mainViewModel; // Gán ViewModel cho View
                dashboard.Show();
            }
            else
            {
                // ==> ĐĂNG NHẬP THẤT BẠI <==
                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu!", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi kết nối Database: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // --- CÁC HÀM KHÁC GIỮ NGUYÊN ---
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