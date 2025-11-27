using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    // Không cần MainLoginViewModel nữa
    public LoginViewModel() { }

    [ObservableProperty]
    private string _username = string.Empty;

    // Hàm xử lý đăng nhập
    [RelayCommand]
    private async Task Login(object parameter)
    {
        if (parameter is not PasswordBox passwordBox) return;
        string password = passwordBox.Password;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var context = new DataContext();

            var account = await context.Accounts
                .Include(a => a.Role)
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(u => u.Username == Username && u.PasswordHash == password);

            if (account != null)
            {
                if (!account.IsActive)
                {
                    MessageBox.Show("Tài khoản này đã bị khóa!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Hiển thị thông báo thành công
                string roleName = account.Role != null ? account.Role.RoleName : "N/A";
                string empName = account.Employee != null ? account.Employee.FullName : "Unknown";
                MessageBox.Show($"Đăng nhập thành công!\nXin chào: {empName}\nQuyền: {roleName}", "Thành công");

                // TODO: Mở Dashboard và đóng LoginWindow
                // var dashboard = new MainWindow();
                // dashboard.Show();

                // Để đóng cửa sổ hiện tại từ ViewModel, ta có thể dùng Application.Current
                // Tuy nhiên, cách chuẩn MVVM là dùng Service hoặc Event.
                // Cách đơn giản nhất (tạm thời):
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.DataContext == this)
                    {
                        window.Close();
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi hệ thống");
        }
    }

    // Hàm thoát
    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }

    // Hàm mở form quên mật khẩu (Mở cửa sổ mới thay vì đổi View)
    [RelayCommand]
    private void NavigateToForgotPassword()
    {
        MessageBox.Show("Tính năng đang phát triển (Sẽ mở cửa sổ Quên mật khẩu)", "Thông báo");
        // Code mở cửa sổ quên mật khẩu ở đây nếu cần
        // var forgotWindow = new ForgotPasswordWindow();
        // forgotWindow.ShowDialog();
    }
}