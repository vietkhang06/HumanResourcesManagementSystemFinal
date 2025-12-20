using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class TimeSheetViewModel : ObservableObject
{
    [ObservableProperty] private string _currentTimeStr;
    [ObservableProperty] private string _currentDateStr;
    [ObservableProperty] private string _todayStatusText = "Chưa vào ca";
    [ObservableProperty] private string _todayStatusColor;
    [ObservableProperty] private string _todayCheckInStr = "--:--";
    [ObservableProperty] private string _todayCheckOutStr = "--:--";
    [ObservableProperty] private bool _canCheckIn;
    [ObservableProperty] private bool _canCheckOut;

    public ObservableCollection<TimeSheetDTO> HistoryList { get; set; } = new();

    private DispatcherTimer _timer;

    public TimeSheetViewModel()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (s, e) => UpdateClock();
        _timer.Start();

        UpdateClock();
        _ = InitializeAsync();
    }

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

    private async Task InitializeAsync()
    {
        await LoadTodayStateAsync();
        await LoadHistoryAsync();
    }

    private void UpdateClock()
    {
        DateTime now = DateTime.Now;
        CurrentTimeStr = now.ToString("HH:mm:ss");
        CurrentDateStr = now.ToString("dddd, dd-MM-yyyy");
    }

    private async Task LoadTodayStateAsync()
    {
        try
        {
            using var context = new DataContext();
            int myId = UserSession.CurrentEmployeeId;
            DateTime today = DateTime.Today;

            var record = await context.TimeSheets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.EmployeeId == myId && t.Date == today);

            if (record == null)
            {
                TodayStatusText = "Chưa vào ca";
                TodayStatusColor = "#718096";
                TodayCheckInStr = "--:--";
                TodayCheckOutStr = "--:--";
                CanCheckIn = true;
                CanCheckOut = false;
            }
            else if (record.CheckOutTime == TimeSpan.Zero || record.CheckOutTime == null)
            {
                TodayStatusText = "Đang làm việc";
                TodayStatusColor = "#38A169";
                TodayCheckInStr = record.CheckInTime.HasValue ? record.CheckInTime.Value.ToString(@"hh\:mm") : "--:--";
                TodayCheckOutStr = "--:--";
                CanCheckIn = false;
                CanCheckOut = true;
            }
            else
            {
                TodayStatusText = "Đã kết thúc ca";
                TodayStatusColor = "#E53E3E";
                TodayCheckInStr = record.CheckInTime.HasValue ? record.CheckInTime.Value.ToString(@"hh\:mm") : "--:--";
                TodayCheckOutStr = record.CheckOutTime.Value.ToString(@"hh\:mm");
                CanCheckIn = false;
                CanCheckOut = false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tải trạng thái chấm công:\n" + GetDeepErrorMessage(ex), "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadHistoryAsync()
    {
        try
        {
            using var context = new DataContext();
            int myId = UserSession.CurrentEmployeeId;

            var list = await context.TimeSheets
                .AsNoTracking()
                .Where(t => t.EmployeeId == myId)
                .OrderByDescending(t => t.Date)
                .Take(30)
                .ToListAsync();

            HistoryList.Clear();
            foreach (var item in list)
            {
                string status = "Đang làm";
                string color = "#ECC94B";

                if (item.CheckOutTime != null && item.CheckOutTime != TimeSpan.Zero)
                {
                    if (item.HoursWorked >= 8) { status = "Đủ công"; color = "#38A169"; }
                    else { status = "Thiếu công"; color = "#E53E3E"; }
                }

                HistoryList.Add(new TimeSheetDTO
                {
                    Date = item.Date,
                    CheckInText = item.CheckInTime.HasValue ? item.CheckInTime.Value.ToString(@"hh\:mm") : "--:--",
                    CheckOutText = (item.CheckOutTime != null && item.CheckOutTime != TimeSpan.Zero) ? item.CheckOutTime.Value.ToString(@"hh\:mm") : "--:--",
                    TotalHoursText = item.HoursWorked > 0 ? $"{item.HoursWorked:F1} giờ" : "...",
                    StatusText = status,
                    StatusColor = color
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tải lịch sử chấm công:\n" + GetDeepErrorMessage(ex), "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task CheckInAsync()
    {
        try
        {
            using var context = new DataContext();

            bool alreadyCheckedIn = await context.TimeSheets.AnyAsync(t => t.EmployeeId == UserSession.CurrentEmployeeId && t.Date == DateTime.Today);
            if (alreadyCheckedIn)
            {
                MessageBox.Show("Bạn đã chấm công ngày hôm nay rồi!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                await InitializeAsync();
                return;
            }

            var newRecord = new TimeSheet
            {
                EmployeeId = UserSession.CurrentEmployeeId,
                Date = DateTime.Today,
                CheckInTime = DateTime.Now.TimeOfDay,
                CheckOutTime = null,
                HoursWorked = 0
            };

            context.TimeSheets.Add(newRecord);
            await context.SaveChangesAsync();

            MessageBox.Show($"Check-in thành công lúc {newRecord.CheckInTime:hh\\:mm}!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            await InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi Check-in:\n" + GetDeepErrorMessage(ex), "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task CheckOutAsync()
    {
        if (MessageBox.Show("Bạn có chắc muốn kết thúc ca làm việc?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

        try
        {
            using var context = new DataContext();
            int myId = UserSession.CurrentEmployeeId;
            DateTime today = DateTime.Today;

            var record = await context.TimeSheets.FirstOrDefaultAsync(t => t.EmployeeId == myId && t.Date == today);

            if (record != null)
            {
                if (record.CheckOutTime != null && record.CheckOutTime != TimeSpan.Zero)
                {
                    MessageBox.Show("Bạn đã Check-out ngày hôm nay rồi.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await InitializeAsync();
                    return;
                }

                record.CheckOutTime = DateTime.Now.TimeOfDay;
                TimeSpan duration = TimeSpan.Zero;

                if (record.CheckOutTime.HasValue && record.CheckInTime.HasValue)
                {
                    duration = record.CheckOutTime.Value - record.CheckInTime.Value;
                }

                record.HoursWorked = Math.Round(duration.TotalHours, 1);

                await context.SaveChangesAsync();

                MessageBox.Show($"Check-out thành công! Tổng làm: {record.HoursWorked} giờ.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                await InitializeAsync();
            }
            else
            {
                MessageBox.Show("Không tìm thấy dữ liệu Check-in ngày hôm nay.", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi Check-out:\n" + GetDeepErrorMessage(ex), "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}