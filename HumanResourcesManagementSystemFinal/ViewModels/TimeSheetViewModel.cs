using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Threading; // Để dùng DispatcherTimer

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class TimeSheetViewModel : ObservableObject
{
    // --- 1. ĐỒNG HỒ THỜI GIAN THỰC ---
    [ObservableProperty] private string _currentTime;
    [ObservableProperty] private string _currentDate;

    private DispatcherTimer _timer;

    // --- 2. DANH SÁCH LỊCH SỬ ---
    public ObservableCollection<TimeSheet> TimeSheets { get; set; } = new();

    // --- 3. TRẠNG THÁI HÔM NAY ---
    [ObservableProperty] private string _todayStatus = "Chưa vào ca";
    [ObservableProperty] private string _checkInTimeDisplay = "--:--";
    [ObservableProperty] private string _checkOutTimeDisplay = "--:--";

    public TimeSheetViewModel()
    {
        // Khởi tạo đồng hồ
        StartClock();

        // Load dữ liệu mẫu
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
        // TODO: Kết nối DB lấy dữ liệu của User đang đăng nhập
        // Fake data
        TimeSheets.Add(new TimeSheet { Date = DateTime.Now.AddDays(-1), CheckInTime = TimeSpan.Parse("08:00"), CheckOutTime = TimeSpan.Parse("17:00"), HoursWorked = 8 });
        TimeSheets.Add(new TimeSheet { Date = DateTime.Now.AddDays(-2), CheckInTime = TimeSpan.Parse("08:15"), CheckOutTime = TimeSpan.Parse("17:15"), HoursWorked = 8 });

        // Cập nhật trạng thái hôm nay (Giả lập)
        CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        CurrentDate = DateTime.Now.ToString("dddd, dd/MM/yyyy");
    }

    [RelayCommand]
    private void CheckIn()
    {
        // Logic Check-in vào DB
        TodayStatus = "Đang làm việc";
        CheckInTimeDisplay = DateTime.Now.ToString("HH:mm");
        System.Windows.MessageBox.Show("Check-in thành công!", "Thông báo");
    }

    [RelayCommand]
    private void CheckOut()
    {
        // Logic Check-out vào DB
        TodayStatus = "Đã tan ca";
        CheckOutTimeDisplay = DateTime.Now.ToString("HH:mm");
        System.Windows.MessageBox.Show("Check-out thành công!", "Thông báo");
    }
}