using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class TimeSheetViewModel : ObservableObject
{
    private readonly TimeSpan _shiftStart = new(8, 0, 0);    // 08:00: Bắt đầu cho phép Check-in
    private readonly TimeSpan _lunchStart = new(12, 0, 0);   // 12:00: Bắt đầu nghỉ trưa
    private readonly TimeSpan _lunchEnd = new(13, 0, 0);   // 13:00: Hết nghỉ trưa
    private readonly TimeSpan _shiftEnd = new(17, 0, 0);   // 17:00: Hết giờ làm việc
    private readonly TimeSpan _lateThreshold = new(8, 15, 0); // 08:15: Mốc tính đi muộn
    [ObservableProperty] private string _currentTimeStr;
    [ObservableProperty] private string _currentDateStr;
    [ObservableProperty] private string _todayStatusText = "Chưa vào ca";
    [ObservableProperty] private string _todayStatusColor = "#718096";
    [ObservableProperty] private string _todayCheckInStr = "--:--";
    [ObservableProperty] private string _todayCheckOutStr = "--:--";
    [ObservableProperty] private bool _canCheckIn;
    [ObservableProperty] private bool _canCheckOut;
    private bool _dbHasCheckedIn = false;
    private bool _dbHasCheckedOut = false;
    public ObservableCollection<TimeSheetDTO> HistoryList { get; set; } = new();
    private DispatcherTimer _timer;

    public TimeSheetViewModel()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) =>
        {
            UpdateClock();
            UpdateButtonStateRealTime(); 
        };
        _timer.Start();

        UpdateClock();
        _ = InitializeAsync();
    }

    private void UpdateClock()
    {
        DateTime now = DateTime.Now;
        CurrentTimeStr = now.ToString("HH:mm:ss");
        CurrentDateStr = now.ToString("dddd, dd/MM/yyyy");
    }

    private void UpdateButtonStateRealTime()
    {
        var now = DateTime.Now.TimeOfDay;

        // Điều kiện để Nút Check-in SÁNG (Enabled):
        // 1. Chưa Check-in trong Database (_dbHasCheckedIn == false)
        // 2. Phải >= 08:00 (_shiftStart)
        // 3. Phải <= 17:00 (_shiftEnd)
        // 4. Không nằm trong giờ nghỉ trưa (12:00 - 13:00)
        bool isWorkingHours = (now >= _shiftStart && now <= _shiftEnd);
        bool isLunchTime = (now >= _lunchStart && now < _lunchEnd);
        CanCheckIn = !_dbHasCheckedIn && isWorkingHours && !isLunchTime;
        // Điều kiện Nút Check-out SÁNG:
        // 1. Đã Check-in rồi
        // 2. Chưa Check-out
        // (Không chặn giờ về, vì nhân viên có thể xin về sớm bất cứ lúc nào)
        CanCheckOut = _dbHasCheckedIn && !_dbHasCheckedOut;
    }

    private async Task InitializeAsync()
    {
        await LoadTodayStateAsync();
        await LoadHistoryAsync();
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
                _dbHasCheckedIn = false;
                _dbHasCheckedOut = false;

                TodayStatusText = "Chưa vào ca";
                TodayStatusColor = "#718096";
                TodayCheckInStr = "--:--";
                TodayCheckOutStr = "--:--";
            }
            else
            {
                _dbHasCheckedIn = true;
                TodayCheckInStr = record.CheckInTime?.ToString(@"hh\:mm");

                if (record.CheckOutTime == null || record.CheckOutTime == TimeSpan.Zero)
                {
                    _dbHasCheckedOut = false;
                    TodayStatusText = "Đang làm việc";
                    TodayStatusColor = "#38A169";
                    TodayCheckOutStr = "--:--";
                }
                else
                {
                    _dbHasCheckedOut = true;
                    TodayStatusText = "Đã kết thúc ca";
                    TodayStatusColor = "#2B6CB0";
                    TodayCheckOutStr = record.CheckOutTime?.ToString(@"hh\:mm");
                }
            }

            UpdateButtonStateRealTime();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
        }
    }

    [RelayCommand]
    private async Task CheckInAsync()
    {
        var nowTime = DateTime.Now.TimeOfDay;

        if (nowTime < _shiftStart || nowTime > _shiftEnd || (nowTime >= _lunchStart && nowTime < _lunchEnd))
        {
            MessageBox.Show("Không trong khung giờ cho phép chấm công.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var context = new DataContext();
            bool exists = await context.TimeSheets.AnyAsync(t => t.EmployeeId == UserSession.CurrentEmployeeId && t.Date == DateTime.Today);
            if (exists) { await LoadTodayStateAsync(); return; }

            var newRecord = new TimeSheet
            {
                EmployeeId = UserSession.CurrentEmployeeId,
                Date = DateTime.Today,
                CheckInTime = nowTime,
                CheckOutTime = null,
                HoursWorked = 0
            };

            context.TimeSheets.Add(newRecord);
            await context.SaveChangesAsync();

            string msg = "Check-in thành công!";
            if (nowTime > _lateThreshold) msg += "\n(Lưu ý: Bạn đã đi muộn)";

            MessageBox.Show(msg, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            await InitializeAsync();
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
    }

    [RelayCommand]
    private async Task CheckOutAsync()
    {
        var nowTime = DateTime.Now.TimeOfDay;
        if (nowTime < _shiftEnd)
        {
            if (MessageBox.Show("Chưa đến giờ tan ca (17:00). Bạn có chắc muốn về sớm?",
               "Xác nhận về sớm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
        }

        try
        {
            using var context = new DataContext();
            var record = await context.TimeSheets.FirstOrDefaultAsync(t => t.EmployeeId == UserSession.CurrentEmployeeId && t.Date == DateTime.Today);

            if (record != null)
            {
                record.CheckOutTime = nowTime;

                double totalHours = 0;
                if (record.CheckInTime.HasValue)
                {
                    TimeSpan inTime = record.CheckInTime.Value;
                    TimeSpan outTime = nowTime;

                    // Nếu làm xuyên qua trưa (Vào sáng, Ra chiều) -> Trừ 1h nghỉ
                    if (inTime < _lunchStart && outTime > _lunchEnd)
                    {
                        totalHours = (outTime - inTime).TotalHours - 1.0;
                    }
                    else
                    {
                        totalHours = (outTime - inTime).TotalHours;
                    }
                }

                record.HoursWorked = Math.Max(0, Math.Round(totalHours, 1));

                await context.SaveChangesAsync();
                MessageBox.Show($"Check-out thành công!\nTổng giờ công: {record.HoursWorked}h.", "Hoàn thành", MessageBoxButton.OK, MessageBoxImage.Information);
                await InitializeAsync();
            }
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
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
                string status;
                string color;

                if (item.CheckOutTime == null)
                {
                    status = (item.Date == DateTime.Today) ? "Đang làm việc" : "Quên Check-out";
                    color = (item.Date == DateTime.Today) ? "#ECC94B" : "#A0AEC0";
                }
                else
                {
                    bool isLate = item.CheckInTime.Value > _lateThreshold;
                    bool isEarly = item.CheckOutTime.Value < _shiftEnd;

                    if (!isLate && !isEarly && item.HoursWorked >= 8)
                    {
                        status = "Đủ công"; color = "#38A169";
                    }
                    else if (isLate && isEarly)
                    {
                        status = "Muộn & Về sớm"; color = "#DD6B20";
                    }
                    else if (isLate)
                    {
                        status = "Đi muộn"; color = "#D69E2E";
                    }
                    else if (isEarly)
                    {
                        status = "Về sớm"; color = "#D69E2E";
                    }
                    else
                    {
                        status = "Thiếu giờ"; color = "#E53E3E";
                    }
                }

                HistoryList.Add(new TimeSheetDTO
                {
                    Date = item.Date,
                    CheckInText = item.CheckInTime?.ToString(@"hh\:mm") ?? "--:--",
                    CheckOutText = item.CheckOutTime?.ToString(@"hh\:mm") ?? "--:--",
                    TotalHoursText = item.HoursWorked > 0 ? $"{item.HoursWorked}h" : "...",
                    StatusText = status,
                    StatusColor = color
                });
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task ExportReportAsync()
    {
        bool isAdmin = AppSession.CurrentRole == "Admin" || AppSession.CurrentRole == "Manager"; 
        bool exportAll = false;
        if (isAdmin)
        {
            var result = MessageBox.Show(
                "Bạn đang là Quản trị viên.\n\n" +
                "- Yes: Xuất báo cáo TỔNG HỢP (Toàn bộ nhân viên)\n" +
                "- No: Chỉ xuất báo cáo CÁ NHÂN của bạn\n" +
                "- Cancel: Hủy bỏ",
                "Lựa chọn xuất báo cáo",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel) return;
            if (result == MessageBoxResult.Yes) exportAll = true;
        }
        var dataToExport = new List<TimeSheetDTO>();

        try
        {
            using var context = new DataContext();

            if (exportAll)
            {
                var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                var allRecords = await context.TimeSheets.Include(t => t.Employee) .Where(t => t.Date >= startOfMonth).OrderBy(t => t.EmployeeId).ThenBy(t => t.Date).ToListAsync();

                foreach (var item in allRecords)
                {
                    dataToExport.Add(ConvertToDTO(item, item.Employee));
                }
            }
            else
            {
                if (HistoryList == null || HistoryList.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo");
                    return;
                }
                var currentUser = AppSession.CurrentUser;
                foreach (var item in HistoryList)
                {
                    var dto = item;
                    dataToExport.Add(dto);
                }
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = exportAll ? "Báo Cáo Tổng Hợp" : "Báo Cáo Cá Nhân",
                FileName = exportAll
                    ? $"TongHopChamCong_Thang{DateTime.Now:MM_yyyy}.xlsx"
                    : $"ChamCong_{AppSession.CurrentUser.Id}_{DateTime.Now:MM_yyyy}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                await Task.Run(() =>
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Lịch Sử Chấm Công");
                        var titleRange = worksheet.Range("A1:G1");
                        titleRange.Merge().Value = "BÁO CÁO CHI TIẾT CHẤM CÔNG";
                        titleRange.Style.Font.Bold = true;
                        titleRange.Style.Font.FontSize = 16;
                        titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        titleRange.Style.Fill.BackgroundColor = XLColor.White;
                        worksheet.Cell("A2").Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                        int headerRow = 4; 
                        worksheet.Cell(headerRow, 1).Value = "Mã NV";
                        worksheet.Cell(headerRow, 2).Value = "Họ Tên";
                        worksheet.Cell(headerRow, 3).Value = "Ngày";
                        worksheet.Cell(headerRow, 4).Value = "Giờ Vào";
                        worksheet.Cell(headerRow, 5).Value = "Giờ Ra";
                        worksheet.Cell(headerRow, 6).Value = "Tổng Giờ";
                        worksheet.Cell(headerRow, 7).Value = "Trạng Thái";
                        var headerRange = worksheet.Range(headerRow, 1, headerRow, 7);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Font.FontColor = XLColor.White;
                        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1A3D64"); 
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        int currentRow = 5;
                        foreach (var item in dataToExport)
                        {
                            if (exportAll)
                            {
                                worksheet.Cell(currentRow, 1).Value = item.EmployeeId;
                                worksheet.Cell(currentRow, 2).Value = item.EmployeeName;
                            }
                            else
                            {
                                worksheet.Cell(currentRow, 1).Value = AppSession.CurrentUser.Id;
                                worksheet.Cell(currentRow, 2).Value = AppSession.CurrentUser.FirstName + " " + AppSession.CurrentUser.LastName;
                            }

                            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(currentRow, 3).Value = item.Date.ToString("dd/MM/yyyy");
                            worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(currentRow, 4).Value = item.CheckInText;
                            worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(currentRow, 5).Value = item.CheckOutText;
                            worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(currentRow, 6).Value = item.TotalHoursText;
                            worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            var statusCell = worksheet.Cell(currentRow, 7);
                            statusCell.Value = item.StatusText;
                            statusCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            statusCell.Style.Font.Bold = true;

                            if (item.StatusText.Contains("Muộn") || item.StatusText.Contains("Thiếu") || item.StatusText.Contains("Về sớm"))
                            {
                                statusCell.Style.Font.FontColor = XLColor.Red;
                            }
                            else if (item.StatusText.Contains("Đủ công"))
                            {
                                statusCell.Style.Font.FontColor = XLColor.Green;
                            }
                            else
                            {
                                statusCell.Style.Font.FontColor = XLColor.Orange; 
                            }

                            currentRow++; 
                        }

                        if (currentRow > 5)
                        {
                            var dataRange = worksheet.Range(headerRow, 1, currentRow - 1, 7);
                            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(saveFileDialog.FileName);
                    }
                });

                MessageBox.Show("Xuất file thành công!", "Thông báo");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi: " + ex.Message);
        }
    }

    private TimeSheetDTO ConvertToDTO(TimeSheet item, Employee emp)
    {
        string status = "Đang làm";

        return new TimeSheetDTO
        {
            EmployeeId = emp.Id,       
            EmployeeName = emp.FirstName + " " + emp.LastName, 
            Date = item.Date,
            CheckInText = item.CheckInTime?.ToString(@"hh\:mm") ?? "--:--",
            CheckOutText = item.CheckOutTime?.ToString(@"hh\:mm") ?? "--:--",
            TotalHoursText = item.HoursWorked > 0 ? $"{item.HoursWorked}h" : "...",
            StatusText = status
        };
    }
}