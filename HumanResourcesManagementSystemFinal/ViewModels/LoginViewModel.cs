using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using Microsoft.EntityFrameworkCore;
using System; // Cần thêm System để dùng Action
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HumanResourcesManagementSystemFinal.ViewModels;
public partial class LoginViewModel : ObservableObject
{
    // --- NAVIGATOR: Khai báo hành động chuyển trang ---
    // ViewModel cha sẽ gán hàm xử lý vào đây
    public Action? NavigateToForgotPasswordAction { get; set; }

    public LoginViewModel() { }

    [ObservableProperty]
    private string _username = string.Empty;

    // ... (Giữ nguyên logic Login cũ của bạn ở đây) ...

    // Hàm chuyển sang màn hình Quên mật khẩu
    [RelayCommand]
    private void NavigateToForgotPassword()
    {
        // 1. Nếu bấm nút mà KHÔNG hiện bảng này -> Lỗi ở file XAML (Bước 1)
        MessageBox.Show("Bước 1: ViewModel đã nhận lệnh!", "Debug");

        NavigateToForgotPasswordAction?.Invoke();
    }

    // ... (Các hàm Exit, Login giữ nguyên) ...
    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }
}