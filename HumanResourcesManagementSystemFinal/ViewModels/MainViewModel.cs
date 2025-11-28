using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models; // Để nhận Account từ Login
using HumanResourcesManagementSystemFinal.Views;  // Để new các UserControl (HomeControl...)
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // --- 1. CÁC BIẾN QUẢN LÝ TRẠNG THÁI ---

    // Tiêu đề trang (Thay đổi khi bấm Menu)
    [ObservableProperty]
    private string _pageTitle = "Trang Chủ";

    // Nội dung chính (Thay đổi View động)
    [ObservableProperty]
    private object _currentView;

    // --- 2. CONSTRUCTOR (KHỞI TẠO) ---

    // Constructor chính: Nhận thông tin tài khoản từ màn hình Login
    public MainViewModel(Account currentAccount)
    {
        // Bạn có thể dùng biến currentAccount để hiển thị "Xin chào Admin" nếu muốn
        // Ví dụ: PageTitle = $"Xin chào {currentAccount.Username}";

        // Mặc định khi mở lên là vào Trang Chủ
        // LƯU Ý: Đảm bảo bạn đã tạo file Views/HomeControl.xaml
        CurrentView = new HomeControl();
    }

    // Constructor mặc định (Bắt buộc phải có để tránh lỗi Design-time trong XAML)
    public MainViewModel()
    {
        CurrentView = new HomeControl();
    }

    // --- 3. CÁC LỆNH ĐIỀU HƯỚNG (NAVIGATION COMMANDS) ---

    [RelayCommand]
    private void NavigateHome()
    {
        PageTitle = "Trang Chủ";
        CurrentView = new HomeControl();
    }

    [RelayCommand]
    private void NavigateEmployee()
    {
        PageTitle = "Quản Lý Nhân Viên";
        // SAU NÀY BẠN TẠO FILE EmployeeControl.xaml THÌ BỎ COMMENT DÒNG DƯỚI:
        // CurrentView = new EmployeeControl(); 
    }

    [RelayCommand]
    private void NavigateDepartment()
    {
        PageTitle = "Phòng Ban & Vị Trí";
        // CurrentView = new DepartmentControl();
    }

    [RelayCommand]
    private void NavigatePayroll()
    {
        PageTitle = "Tính Lương (Admin)";
        // CurrentView = new PayrollControl();
    }

    [RelayCommand]
    private void NavigateProfile()
    {
        PageTitle = "Hồ Sơ Của Tôi";
        // CurrentView = new ProfileControl();
    }

    [RelayCommand]
    private void NavigateTimeSheet()
    {
        PageTitle = "Chấm Công";
        // CurrentView = new TimeSheetControl();
    }

    // --- 4. LỆNH ĐĂNG XUẤT (FIX LỖI XAML) ---

    // Thay vì dùng (Window window), ta dùng (object parameter) để tránh lỗi kiểm tra kiểu dữ liệu của XAML
    [RelayCommand]
    private void Logout(object parameter)
    {
        // Kiểm tra xem tham số truyền vào có đúng là Window không
        if (parameter is Window currentWindow)
        {
            // 1. Mở lại màn hình Login
            var loginWindow = new LoginWindow();
            loginWindow.Show();

            // 2. Đóng Dashboard hiện tại
            currentWindow.Close();
        }
    }
}