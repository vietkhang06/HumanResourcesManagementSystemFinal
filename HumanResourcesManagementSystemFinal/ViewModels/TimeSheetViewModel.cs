using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Threading; // Để dùng DispatcherTimer

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class TimeSheetViewModel : ObservableObject
{
    [ObservableProperty] private string _currentTime;
    [ObservableProperty] private string _currentDate;
    private DispatcherTimer _timer;
    public ObservableCollection<TimeSheet> TimeSheets { get; set; } = new();

    [ObservableProperty] private string _todayStatus = "Chưa vào ca";
    [ObservableProperty] private string _checkInTimeDisplay = "--:--";
    [ObservableProperty] private string _checkOutTimeDisplay = "--:--";

    public TimeSheetViewModel()
    {
        StartClock();
        LoadTimeSheetData();
    }

    private void StartClock()
    {
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (s, e) =>
        {
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            CurrentDate = DateTime.Now.ToString("dddd, dd/MM/yyyy");
        };
        _timer.Start();
    }

    private void LoadTimeSheetData()
    {
        TimeSheets.Add(new TimeSheet { Date = DateTime.Now.AddDays(-1), CheckInTime = TimeSpan.Parse("08:00"), CheckOutTime = TimeSpan.Parse("17:00"), HoursWorked = 8 });
        TimeSheets.Add(new TimeSheet { Date = DateTime.Now.AddDays(-2), CheckInTime = TimeSpan.Parse("08:15"), CheckOutTime = TimeSpan.Parse("17:15"), HoursWorked = 8 });

        CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        CurrentDate = DateTime.Now.ToString("dddd, dd/MM/yyyy");
    }

    [RelayCommand]
    private void CheckIn()
    {
        TodayStatus = "Đang làm việc";
        CheckInTimeDisplay = DateTime.Now.ToString("HH:mm");
        System.Windows.MessageBox.Show("Check-in thành công!", "Thông báo");
    }

    [RelayCommand]
    private void CheckOut()
    {
        TodayStatus = "Đã tan ca";
        CheckOutTimeDisplay = DateTime.Now.ToString("HH:mm");
        System.Windows.MessageBox.Show("Check-out thành công!", "Thông báo");
    }
}