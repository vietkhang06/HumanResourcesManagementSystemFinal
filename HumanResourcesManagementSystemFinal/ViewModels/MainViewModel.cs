using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Views;
using HumanResourcesManagementSystemFinal.Services;
using System.Windows;
using System.IO;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // === DỮ LIỆU NGƯỜI DÙNG ===
    [ObservableProperty] private Employee _currentUser; // Lưu thông tin nhân viên đăng nhập
    [ObservableProperty] private string _welcomeMessage;

    // === ĐIỀU HƯỚNG GIAO DIỆN ===
    [ObservableProperty] private string _currentPageName;
    [ObservableProperty] private string _pageTitle = "Trang Chủ";
    [ObservableProperty] private object _currentView;

    // === QUYỀN HẠN ===
    [ObservableProperty] private bool _isAdmin;

    // Lưu trữ tài khoản đăng nhập (để lấy Role và EmployeeId)
    private Account _currentAccount;

    // 1. CONSTRUCTOR CHÍNH (Được gọi từ LoginWindow)
    public MainViewModel(Employee loggedInUser)
    {
        if (loggedInUser == null)
        {
            // Fallback an toàn nếu null
            MessageBox.Show("Lỗi: Không nhận được thông tin người dùng!");
            return;
        }

        _currentUser = loggedInUser;
        _currentAccount = loggedInUser.Account; // Account đã được Include từ Login

        // Kiểm tra quyền (Admin hoặc Manager được coi là Admin trong ngữ cảnh này)
        IsAdmin = _currentAccount?.Role?.RoleName == "Admin" || _currentAccount?.Role?.RoleName == "Manager";

        // Thiết lập lời chào
        _welcomeMessage = $"Xin chào, {_currentUser.LastName} {_currentUser.FirstName}!";

        // Điều hướng mặc định khi mở app
        NavigateHome();
    }

    // 2. CONSTRUCTOR MẶC ĐỊNH (Chỉ dùng cho Design-Time hoặc Test)
    public MainViewModel()
    {
        // Giả lập dữ liệu để Designer hiển thị được giao diện
        IsAdmin = true;
        PageTitle = "Trang Chủ (Design Mode)";
    }

    // === CÁC PROPERTY HIỂN THỊ TRÊN GIAO DIỆN ===
    public string CurrentUserName => _currentUser != null ? $"{_currentUser.LastName} {_currentUser.FirstName}" : "Unknown User";

    public string CurrentUserJob => _currentUser?.Position?.Title ?? "N/A";

    public string CurrentUserAvatar
    {
        get
        {
            if (_currentUser != null)
            {
                // 1. Tìm ảnh theo ID nhân viên
                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages", $"{_currentUser.Id}.jpg");
                if (File.Exists(imagePath)) return imagePath;

                imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages", $"{_currentUser.Id}.png");
                if (File.Exists(imagePath)) return imagePath;
            }
            // 2. Ảnh mặc định nếu không tìm thấy
            return "/Images/default_user.png"; // Đảm bảo file này tồn tại trong project (Build Action: Content)
        }
    }

    // === CÁC LỆNH ĐIỀU HƯỚNG (NAVIGATION COMMANDS) ===

    [RelayCommand]
    private void NavigateHome()
    {
        PageTitle = "Trang Chủ";
        CurrentPageName = "Home";

        if (IsAdmin)
        {
            // Admin thấy Dashboard tổng quan
            CurrentView = new HomeControl();
        }
        else
        {
            // Nhân viên thường thấy Dashboard cá nhân
            int empId = _currentUser?.Id ?? 0;
            CurrentView = new EmployeeHomeControl
            {
                DataContext = new EmployeeHomeViewModel(empId)
            };
        }
    }

    [RelayCommand]
    private void NavigateEmployee()
    {
        if (!IsAdmin)
        {
            MessageBox.Show("Bạn không có quyền truy cập chức năng này!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        PageTitle = "Quản Lý Nhân Viên";
        CurrentPageName = "Employee";
        CurrentView = new ManageEmployeeControl();
    }

    [RelayCommand]
    private void NavigateDepartment()
    {
        if (!IsAdmin)
        {
            MessageBox.Show("Bạn không có quyền truy cập chức năng này!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        PageTitle = "Phòng Ban & Vị Trí";
        CurrentPageName = "Department";
        CurrentView = new Department_Position_Control();
    }

    [RelayCommand]
    private void NavigatePayroll()
    {
        // Có thể mở cho cả Admin và Nhân viên (nhân viên chỉ xem lương mình)
        PageTitle = "Bảng Lương";
        CurrentPageName = "Payroll";

        if (IsAdmin)
        {
            CurrentView = new PayrollControl();
        }
        else
        {
            // Nếu có control xem lương cá nhân thì gọi ở đây
            MessageBox.Show("Chức năng xem lương cá nhân đang phát triển.");
        }
    }

    [RelayCommand]
    private void NavigateProfile()
    {
        PageTitle = "Hồ Sơ Của Tôi";
        CurrentPageName = "Profile";

        // Truyền ID người dùng vào ViewModel của Profile
        CurrentView = new ProfileControl();
        // Lưu ý: ProfileViewModel cần được cập nhật để nhận ID người dùng hiện tại
    }

    [RelayCommand]
    private void NavigateTimeSheet()
    {
        PageTitle = "Chấm Công";
        CurrentPageName = "TimeSheet";
        CurrentView = new TimeSheetControl();
    }

    [RelayCommand]
    public void NavigateLeaveRequest()
    {
        PageTitle = "Quản Lý Nghỉ Phép";
        CurrentPageName = "LeaveRequest";

        var leaveService = new LeaveRequestService(new DataContext());
        int empId = _currentUser?.Id ?? 0;
        string role = _currentAccount?.Role?.RoleName ?? "Employee";

        var leaveViewModel = new LeaveRequestViewModel(leaveService, empId, role);
        var view = new LeaveRequestControl();
        view.DataContext = leaveViewModel;

        CurrentView = view;
    }

    [RelayCommand]
    private void NavigateChangePassword()
    {
        PageTitle = "Đổi Mật Khẩu";
        CurrentPageName = "ChangePassword";

        if (_currentAccount != null)
        {
            CurrentView = new ChangePasswordControl
            {
                DataContext = new ChangePasswordViewModel(_currentAccount.AccountId)
            };
        }
    }

    // === ĐĂNG XUẤT ===
    [RelayCommand]
    private void Logout(object parameter)
    {
        var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            if (parameter is Window currentWindow)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                currentWindow.Close();
            }
        }
    }
}