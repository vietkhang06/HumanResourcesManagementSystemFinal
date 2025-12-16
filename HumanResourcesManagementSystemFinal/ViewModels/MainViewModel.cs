using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using HumanResourcesManagementSystemFinal.Views;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private Account _currentAccount;

    // Khởi tạo tạm thời với giá trị mặc định để tránh lỗi Null Reference
    [ObservableProperty] private Employee _currentUser = new Employee();

    [ObservableProperty] private string _welcomeMessage;
    [ObservableProperty] private string _currentPageName;
    [ObservableProperty] private string _pageTitle = "Trang Chủ";
    [ObservableProperty] private object _currentView;
    [ObservableProperty] private bool _isAdmin;

    // SỬA CONSTRUCTOR ĐỂ NHẬN ĐỐI TƯỢNG ACCOUNT TỪ LoginViewModel
    public MainViewModel(Account loggedInAccount)
    {
        if (loggedInAccount == null)
        {
            MessageBox.Show("Lỗi: Không nhận được thông tin tài khoản!");
            return;
        }

        // Bước 1: Lưu tài khoản hiện tại
        _currentAccount = loggedInAccount;

        // Bước 2: Lấy thông tin Employee liên quan từ DB
        try
        {
            using var context = new DataContext();

            var employee = context.Employees
             .Include(e => e.Position)
             .Include(e => e.Account)
             .FirstOrDefault(e => e.Account.AccountId == loggedInAccount.AccountId);


            if (employee == null)
            {
                MessageBox.Show("Lỗi: Không tìm thấy thông tin nhân viên liên kết với tài khoản!");
                return;
            }

            // Gán Account đã load đầy đủ (bao gồm Role) vào Employee
            employee.Account = loggedInAccount;

            CurrentUser = employee;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi truy vấn dữ liệu nhân viên: {ex.Message}");
            return;
        }

        // Bước 3: Thiết lập các thuộc tính View Model
        // IsAdmin được xác định dựa trên Role của Account đã được Include trong LoginViewModel
        IsAdmin = _currentAccount?.Role?.RoleName == "Admin" || _currentAccount?.Role?.RoleName == "Manager";

        // SỬ DỤNG CurrentUser (là thuộc tính)
        WelcomeMessage = $"Xin chào, {CurrentUser.LastName} {CurrentUser.FirstName}!";

        NavigateHome();
    }

    // Constructor cho Design Mode (Không thay đổi)
    public MainViewModel()
    {
        // Thiết lập dữ liệu giả cho chế độ Design Mode
        CurrentUser = new Employee
        {
            FirstName = "Design",
            LastName = "Mode",
            Position = new Position { Title = "Developer" }
        };
        _currentAccount = new Account { Role = new Role { RoleName = "Admin" } };
        IsAdmin = true;
        PageTitle = "Trang Chủ (Design Mode)";
        WelcomeMessage = "Xin chào, Design Mode Developer!";
    }

    // THUỘC TÍNH ĐỌC (READ-ONLY PROPERTIES)
    public string CurrentUserName => CurrentUser != null ? $"{CurrentUser.LastName} {CurrentUser.FirstName}" : "Unknown User";

    public string CurrentUserJob => CurrentUser?.Position?.Title ?? "N/A";

    public string CurrentUserAvatar
    {
        get
        {
            if (CurrentUser != null)
            {
                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages", $"{CurrentUser.Id}.jpg");
                if (File.Exists(imagePath)) return imagePath;

                imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages", $"{CurrentUser.Id}.png");
                if (File.Exists(imagePath)) return imagePath;
            }
            return "/Images/default_user.png";
        }
    }

    // =========================================================================
    // CÁC RELAY COMMANDS (Không thay đổi)
    // =========================================================================
    [RelayCommand]
    private void NavigateHome()
    {
        PageTitle = "Trang Chủ";
        CurrentPageName = "Home";

        if (IsAdmin)
        {
            CurrentView = new HomeControl();
        }
        else
        {
            int empId = CurrentUser?.Id ?? 0;
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
        PageTitle = "Bảng Lương";
        CurrentPageName = "Payroll";

        if (IsAdmin)
        {
            CurrentView = new PayrollControl();
        }
        else
        {
            MessageBox.Show("Chức năng xem lương cá nhân đang phát triển.");
        }
    }

    [RelayCommand]
    private void NavigateProfile()
    {
        PageTitle = "Hồ Sơ Của Tôi";
        CurrentPageName = "Profile";
        CurrentView = new ProfileControl();
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
        int empId = CurrentUser?.Id ?? 0;
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

    [RelayCommand]
    private void NavigateHistory()
    {
        PageTitle = "Lịch Sử Hoạt Động";
        CurrentPageName = "History";
        CurrentView = new ChangeHistoryControl();
    }

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