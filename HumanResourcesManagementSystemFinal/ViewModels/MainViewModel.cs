using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class MainViewModel : ObservableObject, IRecipient<ValueChangedMessage<string>>
    {
        private readonly Account _currentAccount;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentUserName))]
        [NotifyPropertyChangedFor(nameof(CurrentUserJob))]
        [NotifyPropertyChangedFor(nameof(CurrentUserAvatar))]
        private Employee _currentUser = new();

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

            WeakReferenceMessenger.Default.Register(this);

            _ = LoadCurrentUserAsync();
            NavigateHome();
        }

        public MainViewModel()
        {
            _currentAccount = new Account { Role = new Role { RoleName = "Admin" } };
            IsAdmin = true;
            PageTitle = "Trang Chủ (Design)";
            WelcomeMessage = "Xin chào, Developer!";
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
                    .FirstOrDefaultAsync(e => e.Account.UserID == _currentAccount.UserID);

                if (employee != null)
                {
                    employee.Account = _currentAccount;
                    CurrentUser = employee;

                    AppSession.CurrentUser = employee;
                    AppSession.CurrentRole = _currentAccount.Role?.RoleName;

                    WelcomeMessage = $"Xin chào, {CurrentUser.FullName}!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải thông tin cá nhân: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string CurrentUserName => !string.IsNullOrEmpty(CurrentUser?.EmployeeID) ? CurrentUser.FullName : "Người dùng";

        public string CurrentUserJob => CurrentUser?.Position?.PositionName ?? (_isAdmin ? "Administrator" : "N/A");

        public string CurrentUserAvatar => CurrentUser?.EmployeeID;

        public void Receive(ValueChangedMessage<string> message)
        {
            if (message.Value == "RefreshUser" || (CurrentUser != null && message.Value == CurrentUser.EmployeeID))
            {
                RefreshCurrentUser();
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
                ShowAccessDenied();
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
                ShowAccessDenied();
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
            string currentEmpId = CurrentUser?.EmployeeID;
            CurrentView = new ProfileControl(currentEmpId);
        }

        [RelayCommand]
        private void NavigateTimeSheet()
        {
            PageTitle = "Chấm Công";
            CurrentPageName = "TimeSheet";
            var viewModel = new TimeSheetViewModel();
            var view = new TimeSheetControl();
            view.DataContext = viewModel;
            CurrentView = view;
        }

        [RelayCommand]
        private void NavigateLeaveRequest()
        {
            PageTitle = "Quản Lý Nghỉ Phép";
            CurrentPageName = "LeaveRequest";
            var leaveService = new LeaveRequestService(new DataContext());
            string empId = CurrentUser?.EmployeeID;
            string role = _currentAccount?.Role?.RoleName ?? "Employee";
            var leaveViewModel = new LeaveRequestViewModel(leaveService, empId, role);
            CurrentView = new LeaveRequestControl
            {
                DataContext = leaveViewModel
            };
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
                    DataContext = new ChangePasswordViewModel(_currentAccount.UserID)
                };
            }
        }

        [RelayCommand]
        private void NavigateHistory()
        {
            PageTitle = "Lịch Sử Hoạt Động";
            CurrentPageName = "History";
            var viewModel = new ChangeHistoryViewModel();
            var view = new ChangeHistoryControl();
            view.DataContext = viewModel;
            CurrentView = view;
        }

        [RelayCommand]
        private void Logout(object parameter)
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);

            AppSession.CurrentUser = null;
            AppSession.CurrentRole = null;

            new LoginWindow().Show();
            if (parameter is Window w)
            {
                w.Close();
            }
            else
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow || window.DataContext == this)
                    {
                        window.Close();
                        break;
                    }
                }
            }
        }

        private void ShowAccessDenied()
        {
            MessageBox.Show("Bạn không có quyền truy cập chức năng này!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public async void RefreshCurrentUser()
        {
            if (string.IsNullOrEmpty(CurrentUser?.EmployeeID)) return;

            await LoadCurrentUserAsync();

            string tempId = CurrentUser.EmployeeID;

            CurrentUser.EmployeeID = null;
            OnPropertyChanged(nameof(CurrentUserAvatar));

            CurrentUser.EmployeeID = tempId;
            OnPropertyChanged(nameof(CurrentUserAvatar));

            OnPropertyChanged(nameof(CurrentUserName));
            OnPropertyChanged(nameof(CurrentUserJob));
        }
    }
}