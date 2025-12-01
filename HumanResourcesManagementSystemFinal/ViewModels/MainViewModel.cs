using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Views;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // --- 1. QUẢN LÝ TRẠNG THÁI ---

    [ObservableProperty]
    private string _pageTitle = "Trang Chủ";

    [ObservableProperty]
    private object _currentView;

    // [MỚI] Biến để kiểm tra xem có phải Admin không (Dùng để ẩn/hiện nút trên Menu)
    [ObservableProperty]
    private bool _isAdmin;

    // [MỚI] Biến lưu tài khoản hiện tại để dùng cho các trang con
    private Account _currentAccount;

    // --- 2. KHỞI TẠO (CONSTRUCTOR) ---

    // Constructor chính nhận Account từ màn hình Login
    public MainViewModel(Account currentAccount)
    {
        _currentAccount = currentAccount; // Lưu lại để dùng sau

        // Kiểm tra quyền: Nếu RoleName là "Admin" thì IsAdmin = true
        // (Dấu ?. để tránh lỗi nếu Role bị null)
        _isAdmin = currentAccount.Role?.RoleName == "Admin";

        // Gọi hàm này để quyết định xem sẽ hiện Dashboard nào đầu tiên
        NavigateHome();
    }

    // Constructor mặc định (Để tránh lỗi thiết kế XAML)
    public MainViewModel()
    {
        // Mặc định cho designer xem tạm giao diện Admin
        _isAdmin = true;
        CurrentView = new HomeControl();
    }

    // --- 3. ĐIỀU HƯỚNG (NAVIGATION) ---

    [RelayCommand]
    private void NavigateHome()
    {
        PageTitle = "Trang Chủ";

        // [LOGIC QUAN TRỌNG]: Phân luồng giao diện
        if (_isAdmin)
        {
            // Nếu là Admin -> Hiện Dashboard tổng quan
            CurrentView = new HomeControl();
        }
        else
        {
            // Nếu là Nhân viên -> Hiện Dashboard cá nhân
            // Truyền EmployeeId vào để nó chỉ tải dữ liệu của người này
            int empId = _currentAccount.EmployeeId ?? 0;
            CurrentView = new EmployeeHomeControl
            {
                DataContext = new EmployeeHomeViewModel(empId)
            };
        }
    }

    [RelayCommand]
    private void NavigateEmployee()
    {
        PageTitle = "Quản Lý Nhân Viên";
        // Lưu ý: Kiểm tra lại tên Class View của bạn là ManageEmployee hay ManageEmployeeControl nhé
        CurrentView = new ManageEmployeeControl();
    }

    [RelayCommand]
    private void NavigateDepartment()
    {
        PageTitle = "Phòng Ban & Vị Trí";
        CurrentView = new Views.Department_Position_Control();
    }

    [RelayCommand]
    private void NavigatePayroll()
    {
        PageTitle = "Tính Lương";
        // Trang này thường chỉ Admin mới vào được (Đã ẩn nút ở View, nhưng chặn thêm ở đây cho chắc)
        if (_isAdmin)
        {
            CurrentView = new PayrollControl();
        }
    }

    [RelayCommand]
    private void NavigateProfile()
    {
        PageTitle = "Hồ Sơ Của Tôi";
        // Truyền thông tin user hiện tại vào Profile để hiển thị đúng người
        // (Giả sử ProfileViewModel có cách nhận dữ liệu, tạm thời new mặc định)
        CurrentView = new ProfileControl();
    }

    [RelayCommand]
    private void NavigateTimeSheet()
    {
        PageTitle = "Chấm Công";
        CurrentView = new TimeSheetControl();
    }

    // --- 4. HỆ THỐNG ---

    [RelayCommand]
    private void Logout(object parameter)
    {
        // Kiểm tra tham số truyền vào có phải là cửa sổ không
        if (parameter is Window currentWindow)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();     // Mở lại Login
            currentWindow.Close();  // Đóng Dashboard
        }
    }
}