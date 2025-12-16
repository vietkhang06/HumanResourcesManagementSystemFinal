using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using HumanResourcesManagementSystemFinal.Views;
using System.IO;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private Employee _currentUser;
    [ObservableProperty] private string _welcomeMessage;
    [ObservableProperty] private string _currentPageName;
    [ObservableProperty] private string _pageTitle = "Trang Chủ";
    [ObservableProperty] private object _currentView;
    [ObservableProperty] private bool _isAdmin;

    private Account _currentAccount;

    public MainViewModel(Employee loggedInUser)
    {
        if (loggedInUser == null)
        {
            MessageBox.Show("Lỗi: Không nhận được thông tin người dùng!");
            return;
        }

        _currentUser = loggedInUser;
        _currentAccount = loggedInUser.Account;

        IsAdmin = _currentAccount?.Role?.RoleName == "Admin" || _currentAccount?.Role?.RoleName == "Manager";
        _welcomeMessage = $"Xin chào, {_currentUser.LastName} {_currentUser.FirstName}!";

        NavigateHome();
    }

    public MainViewModel()
    {
        IsAdmin = true;
        PageTitle = "Trang Chủ (Design Mode)";
    }

    public string CurrentUserName => _currentUser != null ? $"{_currentUser.LastName} {_currentUser.FirstName}" : "Unknown User";

    public string CurrentUserJob => _currentUser?.Position?.Title ?? "N/A";

    public string CurrentUserAvatar
    {
        get
        {
            if (_currentUser != null)
            {
                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages", $"{_currentUser.Id}.jpg");
                if (File.Exists(imagePath)) return imagePath;

                imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages", $"{_currentUser.Id}.png");
                if (File.Exists(imagePath)) return imagePath;
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