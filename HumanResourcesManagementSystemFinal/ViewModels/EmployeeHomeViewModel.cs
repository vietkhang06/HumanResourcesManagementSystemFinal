using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class EmployeeHomeViewModel : ObservableObject
    {
        [ObservableProperty] private string welcomeMessage;
        [ObservableProperty] private int daysWorkedThisMonth;
        [ObservableProperty] private double remainingLeaveDays = 12;
        [ObservableProperty] private string todayCheckInTime = "--:--";
        [ObservableProperty] private string nextHoliday = "25/12";

        public ObservableCollection<LeaveRequest> MyLeaveRequests { get; } = new();

        private string currentEmployeeId;

        public EmployeeHomeViewModel() { }

        public EmployeeHomeViewModel(string employeeId)
        {
            currentEmployeeId = employeeId;

            if (System.ComponentModel.DesignerProperties
                .GetIsInDesignMode(new DependencyObject()))
                return;

            _ = LoadEmployeeDataAsync();
        }

        private string GetDeepErrorMessage(Exception ex)
        {
            var sb = new StringBuilder(ex.Message);
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
            if (string.IsNullOrEmpty(currentEmployeeId))
                return;

            try
            {
                using var context = new DataContext();

                var emp = await context.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeID == currentEmployeeId);

                if (emp != null)
                    WelcomeMessage = $"Xin chào, {emp.FullName}!";

                var startOfMonth = new DateTime(
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    1
                );

                DaysWorkedThisMonth = await context.TimeSheets
                    .AsNoTracking()
                    .CountAsync(t =>
                        t.EmployeeID == currentEmployeeId &&
                        t.WorkDate >= startOfMonth
                    );

                var todaySheet = await context.TimeSheets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t =>
                        t.EmployeeID == currentEmployeeId &&
                        t.WorkDate.Date == DateTime.Today
                    );

                TodayCheckInTime = todaySheet?.TimeIn.HasValue == true
                    ? todaySheet.TimeIn.Value.ToString(@"hh\:mm")
                    : "--:--";

                var requests = await context.LeaveRequests
                    .AsNoTracking()
                    .Where(l => l.EmployeeID == currentEmployeeId)
                    .OrderByDescending(l => l.StartDate)
                    .Take(5)
                    .ToListAsync();

                MyLeaveRequests.Clear();
                foreach (var r in requests)
                    MyLeaveRequests.Add(r);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể tải dữ liệu trang chủ:\n" + GetDeepErrorMessage(ex),
                    "Lỗi hệ thống",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
