using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models; // Để dùng Account
using HumanResourcesManagementSystemFinal.Views; // Để mở lại Login
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // Lưu thông tin người đang đăng nhập
    [ObservableProperty]
    private string _welcomeMessage = "Xin chào!";

    // Constructor nhận vào Account từ màn hình Login gửi sang
    public MainViewModel(Account currentAccount)
    {
        if (currentAccount != null)
        {
            // Lấy tên hiển thị (ưu tiên tên nhân viên, nếu null thì lấy username)
            string name = currentAccount.Employee?.FullName ?? currentAccount.Username;
            string role = currentAccount.Role?.RoleName ?? "N/A";
            WelcomeMessage = $"Xin chào, {name} ({role})";
        }
    }

    // Constructor mặc định (để tránh lỗi Design-time XAML)
    public MainViewModel() { }

    // --- HÀM ĐĂNG XUẤT ---
    [RelayCommand]
    private void Logout(Window currentWindow)
    {
        // 1. Mở lại màn hình Login
        var loginWindow = new LoginWindow();
        loginWindow.Show();

        // 2. Đóng Dashboard hiện tại
        currentWindow.Close();
    }
}