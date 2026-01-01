using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class EmployeeHomeViewModel : ObservableObject
    {
        [ObservableProperty] private string _welcomeMessage;
        [ObservableProperty] private int _daysWorkedThisMonth;
        [ObservableProperty] private double _remainingLeaveDays = 12;
        [ObservableProperty] private string _todayCheckInTime = "--:--";
        [ObservableProperty] private string _nextHoliday = "25/12";

        public ObservableCollection<LeaveRequest> MyLeaveRequests { get; set; } = new();

        // 1. Đổi từ int sang string
        private string _currentEmployeeId;

        // 2. Constructor nhận string
        public EmployeeHomeViewModel(string employeeId)
        {
            _currentEmployeeId = employeeId;
            // Kiểm tra DesignMode để tránh lỗi khi mở XAML Designer
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;

            _ = LoadEmployeeDataAsync();
        }

        public EmployeeHomeViewModel() { }

        private string GetDeepErrorMessage(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(ex.Message);
            var inner = ex.InnerException;
            while (inner != null)
            {
                sb.AppendLine(inner.Message);
                inner = inner.InnerException;
            }
            return sb.ToString();
        }

        [RelayCommand]
        private async Task LoadEmployeeDataAsync()
        {
            if (string.IsNullOrEmpty(_currentEmployeeId)) return;

            try
            {
                using var context = new DataContext();

                // 3. Tìm nhân viên theo String ID
                var emp = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeID == _currentEmployeeId);
                if (emp != null)
                {
                    // 4. Dùng FullName thay vì First/Last
                    WelcomeMessage = $"Xin chào, {emp.FullName}!";
                }

                // 5. Tính số ngày làm việc
                var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DaysWorkedThisMonth = await context.TimeSheets
                    .AsNoTracking()
                    // TimeSheet: EmployeeID (string), WorkDate (thay cho Date)
                    .CountAsync(t => t.EmployeeID == _currentEmployeeId && t.WorkDate >= startOfMonth);

                // 6. Lấy giờ Check-in hôm nay
                var todaySheet = await context.TimeSheets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.EmployeeID == _currentEmployeeId && t.WorkDate.Date == DateTime.Today);

                if (todaySheet != null)
                {
                    // TimeSheet: TimeIn (thay cho CheckInTime)
                    TodayCheckInTime = todaySheet.TimeIn.HasValue
                        ? todaySheet.TimeIn.Value.ToString(@"hh\:mm")
                        : "--:--";
                }

                // 7. Lấy danh sách nghỉ phép
                var myRequests = await context.LeaveRequests
                    .AsNoTracking()
                    // LeaveRequest: EmployeeID (string)
                    .Where(l => l.EmployeeID == _currentEmployeeId)
                    .OrderByDescending(l => l.StartDate)
                    .Take(5)
                    .ToListAsync();

                MyLeaveRequests.Clear();
                foreach (var req in myRequests)
                {
                    MyLeaveRequests.Add(req);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải dữ liệu trang chủ:\n" + GetDeepErrorMessage(ex), "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}