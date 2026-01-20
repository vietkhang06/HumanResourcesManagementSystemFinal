using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class EmployeeHomeViewModel : ObservableObject
    {
        // --- Properties hiển thị ---
        [ObservableProperty] private string welcomeMessage;
        [ObservableProperty] private int daysWorkedThisMonth;
        [ObservableProperty] private double remainingLeaveDays = 12;
        [ObservableProperty] private string todayCheckInTime = "--:--";

        // --- Properties cho Nút Check-in ---
        [ObservableProperty] private string _checkInButtonContent = "Vào Ca";
        [ObservableProperty] private string _checkInButtonColor = "#1A3D64"; // Xanh đậm mặc định
        [ObservableProperty] private bool _isCheckInEnabled = true;
        [ObservableProperty] private string _checkInButtonIcon = "🕒";

        public ObservableCollection<LeaveRequest> MyLeaveRequests { get; } = new();
        public ObservableCollection<Notification> Notifications { get; } = new();

        private string currentEmployeeId;
        private TimeSheet _todayTimeSheet; // Lưu trữ record chấm công hôm nay

        public EmployeeHomeViewModel() { }

        public EmployeeHomeViewModel(string employeeId)
        {
            currentEmployeeId = employeeId;
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
            _ = LoadEmployeeDataAsync();
        }

        // --- XỬ LÝ LOGIC CHECK IN / OUT ---
        [RelayCommand]
        private async Task QuickCheckInAsync()
        {
            if (string.IsNullOrEmpty(currentEmployeeId)) return;

            try
            {
                using var context = new DataContext();
                var nowTime = DateTime.Now.TimeOfDay;
                var today = DateTime.Today;

                // Tải lại record mới nhất từ DB để đảm bảo đồng bộ
                var existingSheet = await context.TimeSheets
                    .FirstOrDefaultAsync(t => t.EmployeeID == currentEmployeeId && t.WorkDate == today);

                // TRƯỜNG HỢP 1: CHƯA CHECK-IN -> THỰC HIỆN CHECK-IN
                if (existingSheet == null)
                {
                    string newId = GenerateTimeSheetID(context);
                    var newRecord = new TimeSheet
                    {
                        TimeSheetID = newId,
                        EmployeeID = currentEmployeeId,
                        WorkDate = today,
                        TimeIn = nowTime,
                        TimeOut = null,
                        ActualHours = 0
                    };

                    context.TimeSheets.Add(newRecord);
                    await context.SaveChangesAsync();

                    // Cảnh báo đi muộn (ví dụ sau 8:15)
                    var lateThreshold = new TimeSpan(8, 15, 0);
                    string msg = $"Check-in thành công lúc {nowTime:hh\\:mm}!";
                    if (nowTime > lateThreshold) msg += "\n(Lưu ý: Đã quá giờ quy định)";

                    MessageBox.Show(msg, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                // TRƯỜNG HỢP 2: ĐÃ CHECK-IN, CHƯA CHECK-OUT -> THỰC HIỆN CHECK-OUT
                else if (existingSheet.TimeIn != null && existingSheet.TimeOut == null)
                {
                    // Tính thời gian làm việc
                    var duration = nowTime - existingSheet.TimeIn.Value;
                    double totalHours = duration.TotalHours;

                    // CẢNH BÁO NẾU VỀ SỚM (Ví dụ: Dưới 8 tiếng hoặc 9 tiếng tùy quy định)
                    if (totalHours < 8.0) // Giả sử quy định là 8 tiếng
                    {
                        var result = MessageBox.Show(
                            $"Bạn mới làm việc được {totalHours:F1} giờ.\nQuy định là 8 giờ.\n\nBạn có chắc chắn muốn Ra Ca (Check-out) không?",
                            "Cảnh báo về sớm",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);

                        if (result == MessageBoxResult.No) return;
                    }

                    // Cập nhật Database
                    existingSheet.TimeOut = nowTime;
                    existingSheet.ActualHours = totalHours > 0 ? totalHours : 0;

                    await context.SaveChangesAsync();
                    MessageBox.Show($"Check-out thành công lúc {nowTime:hh\\:mm}!\nTổng công: {totalHours:F2} giờ.", "Hoàn thành", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                // TRƯỜNG HỢP 3: ĐÃ CHECK-OUT RỒI -> KHÔNG LÀM GÌ (Nút đã bị disable từ UI, đây là chốt chặn cuối)
                else
                {
                    return;
                }

                // Tải lại giao diện để cập nhật trạng thái nút
                await LoadEmployeeDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- CẬP NHẬT TRẠNG THÁI NÚT DỰA TRÊN DỮ LIỆU ---
        private void UpdateCheckInButtonState()
        {
            if (_todayTimeSheet == null)
            {
                // Chưa Check-in
                CheckInButtonContent = "Vào Ca";
                CheckInButtonColor = "#1A3D64"; // Xanh đậm
                CheckInButtonIcon = "🕒";
                IsCheckInEnabled = true;
            }
            else if (_todayTimeSheet.TimeIn != null && _todayTimeSheet.TimeOut == null)
            {
                // Đã Check-in, Chưa Check-out
                CheckInButtonContent = "Tan Ca";
                CheckInButtonColor = "#DC2626"; // Màu Cam (Amber)
                CheckInButtonIcon = "🏃";
                IsCheckInEnabled = true;
            }
            else
            {
                // Đã hoàn thành (Checked Out)
                CheckInButtonContent = "Hoàn Thành";
                CheckInButtonColor = "#A0AEC0"; // Xám
                CheckInButtonIcon = "✅";
                IsCheckInEnabled = false; // Khóa nút
            }
        }

        [RelayCommand]
        private async Task LoadEmployeeDataAsync()
        {
            if (string.IsNullOrEmpty(currentEmployeeId)) return;

            try
            {
                using var context = new DataContext();

                // 1. Thông tin nhân viên
                var emp = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeID == currentEmployeeId);
                if (emp != null) WelcomeMessage = $"{emp.FullName}!";

                // 2. Thống kê công
                var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DaysWorkedThisMonth = await context.TimeSheets.AsNoTracking()
                    .CountAsync(t => t.EmployeeID == currentEmployeeId && t.WorkDate >= startOfMonth);

                // 3. Lấy record chấm công hôm nay
                _todayTimeSheet = await context.TimeSheets
                    .FirstOrDefaultAsync(t => t.EmployeeID == currentEmployeeId && t.WorkDate.Date == DateTime.Today);

                TodayCheckInTime = _todayTimeSheet?.TimeIn.HasValue == true
                    ? _todayTimeSheet.TimeIn.Value.ToString(@"hh\:mm")
                    : "--:--";

                // Cập nhật trạng thái nút Check-in/Out
                UpdateCheckInButtonState();

                // 4. Load Đơn nghỉ phép & Thông báo (Giữ nguyên code cũ)
                var requests = await context.LeaveRequests.AsNoTracking()
                    .Where(l => l.EmployeeID == currentEmployeeId).OrderByDescending(l => l.StartDate).Take(5).ToListAsync();
                MyLeaveRequests.Clear();
                foreach (var r in requests) MyLeaveRequests.Add(r);

                var notifs = await context.Notifications.AsNoTracking()
                    .OrderByDescending(n => n.Date).Take(5).ToListAsync();
                Notifications.Clear();
                foreach (var n in notifs) Notifications.Add(n);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + GetDeepErrorMessage(ex));
            }
        }

        // ... (Giữ nguyên các hàm Helper: GetDeepErrorMessage, GenerateTimeSheetID, OpenLeavePopup) ...
        private string GetDeepErrorMessage(Exception ex)
        {
            var sb = new StringBuilder(ex.Message);
            var inner = ex.InnerException;
            while (inner != null) { sb.AppendLine(inner.Message); inner = inner.InnerException; }
            return sb.ToString();
        }

        private string GenerateTimeSheetID(DataContext context)
        {
            var lastID = context.TimeSheets.OrderByDescending(t => t.TimeSheetID).Select(t => t.TimeSheetID).FirstOrDefault();
            if (string.IsNullOrEmpty(lastID)) return "TS001";
            string numPart = lastID.Substring(2);
            if (int.TryParse(numPart, out int num)) return "TS" + (num + 1).ToString("D3");
            return "TS" + new Random().Next(100, 999);
        }

        [RelayCommand]
        private async Task OpenLeavePopup()
        {
            if (string.IsNullOrEmpty(currentEmployeeId)) return;
            var popupViewModel = new LeaveRequestPopupViewModel(currentEmployeeId);
            var popupView = new LeaveRequestPopup();
            popupView.DataContext = popupViewModel;
            popupViewModel.CloseAction = new Action(popupView.Close);
            popupView.ShowDialog();
            await LoadEmployeeDataAsync();
        }
    }
}