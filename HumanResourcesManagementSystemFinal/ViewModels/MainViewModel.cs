using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Views;
using HumanResourcesManagementSystemFinal.Services;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _pageTitle = "Trang Chủ";

    [ObservableProperty]
    private object _currentView;

    [ObservableProperty]
    private bool _isAdmin;

    private Account _currentAccount;

    public MainViewModel(Account currentAccount)
    {
        _currentAccount = currentAccount;
        _isAdmin = currentAccount.Role?.RoleName == "Admin";
        NavigateHome();
    }

    public MainViewModel()
    {
        _isAdmin = true;
        CurrentView = new HomeControl();
    }

    [RelayCommand]
    private void NavigateHome()
    {
        PageTitle = "Trang Chủ";

        if (_isAdmin)
        {
            CurrentView = new HomeControl();
        }
        else
        {
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
        CurrentView = new ManageEmployeeControl();
    }

    [RelayCommand]
    private void NavigateDepartment()
    {
        PageTitle = "Phòng Ban & Vị Trí";
        CurrentView = new Department_Position_Control();
    }

    [RelayCommand]
    private void NavigatePayroll()
    {
        PageTitle = "Tính Lương";
        if (_isAdmin)
        {
            CurrentView = new PayrollControl();
        }
    }

    [RelayCommand]
    private void NavigateProfile()
    {
        PageTitle = "Hồ Sơ Của Tôi";
        CurrentView = new ProfileControl();
    }

    [RelayCommand]
    private void NavigateTimeSheet()
    {
        PageTitle = "Chấm Công";
        CurrentView = new TimeSheetControl();
    }

    [RelayCommand]
    public void NavigateLeaveRequest()
    {
        var leaveService = new LeaveRequestService(new DataContext());

        int empId = _currentAccount.EmployeeId ?? 0;
        string role = _currentAccount.Role?.RoleName ?? "Employee";

        var leaveViewModel = new LeaveRequestViewModel(leaveService, empId, role);

        var view = new LeaveRequestControl();
        view.DataContext = leaveViewModel;

        CurrentView = view;
        PageTitle = "Quản Lý Nghỉ Phép";
    }

    [RelayCommand]
    private void Logout(object parameter)
    {
        if (parameter is Window currentWindow)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            currentWindow.Close();
        }
    }
}