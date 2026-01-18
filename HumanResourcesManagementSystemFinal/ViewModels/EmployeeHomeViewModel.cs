using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using HumanResourcesManagementSystemFinal.Data; // Để dùng DataContext
using HumanResourcesManagementSystemFinal.Models; // Để dùng TimeSheet

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

        private string GenerateTimeSheetID(DataContext context)
        {
            var lastID = context.TimeSheets
                .OrderByDescending(t => t.TimeSheetID)
                .Select(t => t.TimeSheetID)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(lastID)) return "TS001";

            string numPart = lastID.Substring(2);
            if (int.TryParse(numPart, out int num))
            {
                return "TS" + (num + 1).ToString("D3");
            }
            return "TS" + new Random().Next(100, 999);
        }

        [RelayCommand]
        private async Task QuickCheckInAsync()
        {
            // Kiểm tra xem đã đăng nhập chưa
            if (string.IsNullOrEmpty(currentEmployeeId)) return;

            var nowTime = DateTime.Now.TimeOfDay;
            // Ngưỡng đi muộn (giống bên TimeSheetViewModel)
            var lateThreshold = new TimeSpan(8, 15, 0);

            try
            {
                using var context = new DataContext();
                var today = DateTime.Today;

                // 1. Kiểm tra xem hôm nay đã chấm công chưa
                var existing = await context.TimeSheets
                    .FirstOrDefaultAsync(t => t.EmployeeID == currentEmployeeId && t.WorkDate == today);

                if (existing != null)
                {
                    MessageBox.Show("Hôm nay bạn đã thực hiện chấm công rồi!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 2. Tạo ID mới theo chuẩn TSxxx
                string newId = GenerateTimeSheetID(context);

                // 3. Lưu vào Database
                var newRecord = new TimeSheet
                {
                    TimeSheetID = newId,
                    EmployeeID = currentEmployeeId,
                    WorkDate = today,
                    TimeIn = nowTime,
                    TimeOut = null, // Chưa check-out
                    ActualHours = 0
                };

                context.TimeSheets.Add(newRecord);
                await context.SaveChangesAsync();

                // 4. Thông báo thành công & Cảnh báo nếu đi muộn
                string msg = $"Check-in thành công lúc {nowTime:hh\\:mm}!";
                if (nowTime > lateThreshold)
                {
                    msg += "\n(Lưu ý: Đã quá 8:15 - Đi muộn)";
                }

                MessageBox.Show(msg, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // 5. Cập nhật lại giao diện
                await LoadEmployeeDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi chấm công: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 2. OPEN POPUP COMMAND
        [RelayCommand]
        private async Task OpenLeavePopup()
        {
            if (string.IsNullOrEmpty(currentEmployeeId)) return;

            // Create ViewModel and View
            var popupViewModel = new LeaveRequestPopupViewModel(currentEmployeeId);
            var popupView = new LeaveRequestPopup();

            // Bind them
            popupView.DataContext = popupViewModel;
            popupViewModel.CloseAction = new Action(popupView.Close);

            // Show and wait
            popupView.ShowDialog();

            // Refresh dashboard list after closing
            await LoadEmployeeDataAsync();
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
