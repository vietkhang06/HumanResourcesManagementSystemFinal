using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Account _currentAccount;
    [ObservableProperty] private Employee _currentUser = new();
    [ObservableProperty] private string _welcomeMessage;
    [ObservableProperty] private string _currentPageName;
    [ObservableProperty] private string _pageTitle = "Trang Chủ";
    [ObservableProperty] private object _currentView;
    [ObservableProperty] private bool _isAdmin;

    public MainViewModel(Account loggedInAccount)
    {
        if (loggedInAccount == null)
        {
            MessageBox.Show("Lỗi: Không nhận được thông tin tài khoản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _currentAccount = loggedInAccount;
        IsAdmin = _currentAccount.Role?.RoleName == "Admin" || _currentAccount.Role?.RoleName == "Manager";

        _ = LoadCurrentUserAsync();

        NavigateHome();
    }

    public MainViewModel()
    {
        // Constructor cho Design Mode
        _currentAccount = new Account { Role = new Role { RoleName = "Admin" } };
        IsAdmin = true;
        PageTitle = "Trang Chủ";
        WelcomeMessage = "Xin chào, Design Mode Developer!";
    }

    private async Task LoadCurrentUserAsync()
    {
        try
        {
            using var context = new DataContext();
            var employee = await context.Employees
                .AsNoTracking()
                .Include(e => e.Position)
                .Include(e => e.Account)
                // 1. Sửa AccountId -> UserID
                .FirstOrDefaultAsync(e => e.Account.UserID == _currentAccount.UserID);

            if (employee != null)
            {
                employee.Account = _currentAccount;
                CurrentUser = employee;
                // 2. Sửa Name -> FullName
                WelcomeMessage = $"Xin chào, {CurrentUser.FullName}!";
                OnPropertyChanged(nameof(CurrentUserAvatar));
                OnPropertyChanged(nameof(CurrentUserJob));
                OnPropertyChanged(nameof(CurrentUserName));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải thông tin cá nhân: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // 3. Cập nhật hiển thị tên và chức vụ
    public string CurrentUserName => !string.IsNullOrEmpty(CurrentUser?.EmployeeID) ? CurrentUser.FullName : "Người dùng";
    public string CurrentUserJob => CurrentUser?.Position?.PositionName ?? "N/A"; // Title -> PositionName

    public string CurrentUserAvatar
    {
        get
        {
            // 4. Kiểm tra ID string
            if (!string.IsNullOrEmpty(CurrentUser?.EmployeeID))
            {
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
                string jpgPath = Path.Combine(baseDir, $"{CurrentUser.EmployeeID}.jpg");
                string pngPath = Path.Combine(baseDir, $"{CurrentUser.EmployeeID}.png");

                if (File.Exists(jpgPath)) return jpgPath;
                if (File.Exists(pngPath)) return pngPath;
            }
            return "/Images/default_user.png";
        }
    }

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
            // 5. Truyền string ID
            string empId = CurrentUser?.EmployeeID;
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
            MessageBox.Show("Chức năng xem lương cá nhân đang được phát triển.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
    private void NavigateLeaveRequest()
    {
        PageTitle = "Quản Lý Nghỉ Phép";
        CurrentPageName = "LeaveRequest";

        var leaveService = new LeaveRequestService(new DataContext());
        // 6. Truyền string ID
        string empId = CurrentUser?.EmployeeID;
        string role = _currentAccount?.Role?.RoleName ?? "Employee";

        var leaveViewModel = new LeaveRequestViewModel(leaveService, empId, role);
        var view = new LeaveRequestControl
        {
            DataContext = leaveViewModel
        };

        CurrentView = view;
    }

    [RelayCommand]
    private void NavigateChangePassword()
    {
        PageTitle = "Đổi Mật Khẩu";
        CurrentPageName = "ChangePassword";

        if (_currentAccount != null)
        {
            // 7. Truyền int UserID (parse from string)
            if (int.TryParse(_currentAccount.UserID, out int userId))
            {
                CurrentView = new ChangePasswordControl
                {
                    DataContext = new ChangePasswordViewModel(userId.ToString())
                };
            }
            else
            {
                MessageBox.Show("ID tài khoản không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            if (parameter is Window currentWindow)
            {
                new LoginWindow().Show();
                currentWindow.Close();
            }
        }
    }
}