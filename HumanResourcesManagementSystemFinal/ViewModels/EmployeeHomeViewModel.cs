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
        private int _currentEmployeeId;

        public EmployeeHomeViewModel(int employeeId)
        {
            _currentEmployeeId = employeeId;
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
            try
            {
                using var context = new DataContext();

                var emp = await context.Employees.FindAsync(_currentEmployeeId);
                if (emp != null)
                {
                    WelcomeMessage = $"Xin chào, {emp.LastName} {emp.FirstName}!";
                }

                var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DaysWorkedThisMonth = await context.TimeSheets
                    .AsNoTracking()
                    .CountAsync(t => t.EmployeeId == _currentEmployeeId && t.Date >= startOfMonth);

                var todaySheet = await context.TimeSheets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.EmployeeId == _currentEmployeeId && t.Date.Date == DateTime.Today);

                if (todaySheet != null)
                {
                    TodayCheckInTime = todaySheet.CheckInTime.HasValue
                        ? todaySheet.CheckInTime.Value.ToString(@"hh\:mm")
                        : "--:--";
                }

                var myRequests = await context.LeaveRequests
                    .AsNoTracking()
                    .Where(l => l.EmployeeId == _currentEmployeeId)
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