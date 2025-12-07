using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq; 
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
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (s, e) => UpdateClock();
        _timer.Start();

        UpdateClock();
        LoadTodayState();
        LoadHistory();
    }

    private void UpdateClock()
    {
        DateTime now = DateTime.Now;
        _currentTimeStr = now.ToString("HH:mm:ss");
        _currentDateStr = now.ToString("dddd, dd-MM-yyyy");
        OnPropertyChanged(nameof(CurrentTimeStr)); 
        OnPropertyChanged(nameof(CurrentDateStr));
    }

    private void LoadTodayState()
    {
        using (var context = new DataContext())
        {
            int myId = UserSession.CurrentEmployeeId;
            DateTime today = DateTime.Today;
            var record = context.TimeSheets.FirstOrDefault(t => t.EmployeeId == myId && t.Date == today);
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
    }

    private void LoadHistory()
    {
        using (var context = new DataContext())
        {
            int myId = UserSession.CurrentEmployeeId;
            var list = context.TimeSheets
                              .Where(t => t.EmployeeId == myId)
                              .OrderByDescending(t => t.Date)
                              .Take(30)
                              .ToList();

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
    }

    [RelayCommand]
    private void CheckIn()
    {
        try
        {
            using (var context = new DataContext())
            {
                var newRecord = new TimeSheet
                {
                    EmployeeId = UserSession.CurrentEmployeeId,
                    Date = DateTime.Today,
                    CheckInTime = DateTime.Now.TimeOfDay,
                    CheckOutTime = null,
                    HoursWorked = 0
                };
                context.TimeSheets.Add(newRecord);
                context.SaveChanges();

                MessageBox.Show($"Check-in thành công lúc {newRecord.CheckInTime:hh\\:mm}!", "Thành công");
                LoadTodayState();
                LoadHistory();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi Check-in: " + ex.Message);
        }
    }

    [RelayCommand]
    private void CheckOut()
    {
        if (MessageBox.Show("Bạn có chắc muốn kết thúc ca làm việc?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

        try
        {
            using (var context = new DataContext())
            {
                int myId = UserSession.CurrentEmployeeId;
                DateTime today = DateTime.Today;

                var record = context.TimeSheets.FirstOrDefault(t => t.EmployeeId == myId && t.Date == today);

                if (record != null)
                {
                    record.CheckOutTime = DateTime.Now.TimeOfDay;
                    TimeSpan duration = TimeSpan.Zero;
                    if (record.CheckOutTime.HasValue && record.CheckInTime.HasValue)
                    {
                        duration = record.CheckOutTime.Value - record.CheckInTime.Value;
                    }
                    record.HoursWorked = Math.Round(duration.TotalHours, 1);
                    context.SaveChanges();
                    MessageBox.Show($"Check-out thành công! Tổng làm: {record.HoursWorked} giờ.", "Thành công");
                    LoadTodayState();
                    LoadHistory();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi Check-out: " + ex.Message);
        }
    }
}