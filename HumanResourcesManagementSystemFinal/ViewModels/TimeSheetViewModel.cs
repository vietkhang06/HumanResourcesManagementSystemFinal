using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.Collections.Generic;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public class TimeSheetDTO
    {
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public DateTime WorkDate { get; set; }
        public string CheckInText { get; set; }
        public string CheckOutText { get; set; }
        public string TotalHoursText { get; set; }
        public string StatusText { get; set; }
        public string StatusColor { get; set; }
    }

    public partial class TimeSheetViewModel : ObservableObject
    {
        // Các mốc giờ (Để test thì tạm thời không dùng đến logic chặn giờ)
        private readonly TimeSpan _shiftStart = new(8, 0, 0);
        private readonly TimeSpan _lunchStart = new(12, 0, 0);
        private readonly TimeSpan _lunchEnd = new(13, 0, 0);
        private readonly TimeSpan _shiftEnd = new(17, 0, 0);
        private readonly TimeSpan _lateThreshold = new(8, 15, 0);

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

        private void UpdateClock()
        {
            DateTime now = DateTime.Now;
            CurrentTimeStr = now.ToString("HH:mm:ss");
            CurrentDateStr = now.ToString("dddd, dd/MM/yyyy");
        }

        private void UpdateButtonStateRealTime()
        {
            // --- LOGIC GỐC (Đã bỏ chặn giờ để Test) ---
            // var now = DateTime.Now.TimeOfDay;
            // bool isWorkingHours = (now >= _shiftStart && now <= _shiftEnd);
            // bool isLunchTime = (now >= _lunchStart && now < _lunchEnd);
            // CanCheckIn = !_dbHasCheckedIn && isWorkingHours && !isLunchTime;

            // --- LOGIC ĐỂ TEST: Cho phép bấm nút bất cứ lúc nào ---
            CanCheckIn = !_dbHasCheckedIn;
            CanCheckOut = _dbHasCheckedIn && !_dbHasCheckedOut;
        }

        private async Task InitializeAsync()
        {
            if (string.IsNullOrEmpty(UserSession.CurrentEmployeeId)) return;
            await LoadTodayStateAsync();
            await LoadHistoryAsync();
        }

        private async Task LoadTodayStateAsync()
        {
            try
            {
                using var context = new DataContext();
                string myId = UserSession.CurrentEmployeeId;
                DateTime today = DateTime.Today;

                var record = await context.TimeSheets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.EmployeeID == myId && t.WorkDate == today);

                if (record == null)
                {
                    _dbHasCheckedIn = false;
                    _dbHasCheckedOut = false;
                    TodayStatusText = "Chưa vào ca";
                    TodayStatusColor = "#718096"; // Xám
                    TodayCheckInStr = "--:--";
                    TodayCheckOutStr = "--:--";
                }
                else
                {
                    _dbHasCheckedIn = true;
                    TodayCheckInStr = record.TimeIn?.ToString(@"hh\:mm");

                    if (record.TimeOut == null || record.TimeOut == TimeSpan.Zero)
                    {
                        _dbHasCheckedOut = false;
                        TodayStatusText = "Đang làm việc";
                        TodayStatusColor = "#38A169"; // Xanh lá
                        TodayCheckOutStr = "--:--";
                    }
                    else
                    {
                        _dbHasCheckedOut = true;
                        TodayStatusText = "Đã kết thúc ca";
                        TodayStatusColor = "#2B6CB0"; // Xanh dương
                        TodayCheckOutStr = record.TimeOut?.ToString(@"hh\:mm");
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

            // --- TẠM THỜI BỎ CHẶN GIỜ ĐỂ TEST ---
            /*
            if (nowTime < _shiftStart || nowTime > _shiftEnd)
            {
                MessageBox.Show($"Hiện tại là {nowTime:hh\\:mm}, không nằm trong khung giờ làm việc ({_shiftStart:hh\\:mm} - {_shiftEnd:hh\\:mm}).",
                    "Chưa đến giờ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (nowTime >= _lunchStart && nowTime < _lunchEnd)
            {
                MessageBox.Show("Hiện đang là giờ nghỉ trưa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            */

            try
            {
                using var context = new DataContext();
                string myId = UserSession.CurrentEmployeeId;

                bool exists = await context.TimeSheets.AnyAsync(t => t.EmployeeID == myId && t.WorkDate == DateTime.Today);
                if (exists) { await LoadTodayStateAsync(); return; }

                var newRecord = new TimeSheet
                {
                    TimeSheetID = GenerateTimeSheetID(context),
                    EmployeeID = myId,
                    WorkDate = DateTime.Today,
                    TimeIn = nowTime,
                    TimeOut = null,
                    ActualHours = 0
                };

                context.TimeSheets.Add(newRecord);
                await context.SaveChangesAsync();

                string msg = "Check-in thành công (Test Mode)!";
                if (nowTime > _lateThreshold) msg += "\n(Lưu ý: Đã quá 8:15)";

                MessageBox.Show(msg, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                await InitializeAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi Check-in: " + ex.Message); }
        }

        [RelayCommand]
        private async Task CheckOutAsync()
        {
            var nowTime = DateTime.Now.TimeOfDay;

            // --- TẠM THỜI BỎ HỎI VỀ SỚM ĐỂ TEST ---
            /*
            if (nowTime < _shiftEnd)
            {
                if (MessageBox.Show("Chưa đến giờ tan ca (17:00). Bạn có chắc muốn về sớm?",
                    "Xác nhận về sớm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
            }
            */

            try
            {
                using var context = new DataContext();
                string myId = UserSession.CurrentEmployeeId;
                var record = await context.TimeSheets.FirstOrDefaultAsync(t => t.EmployeeID == myId && t.WorkDate == DateTime.Today);

                if (record != null)
                {
                    record.TimeOut = nowTime;

                    double totalHours = 0;
                    if (record.TimeIn.HasValue)
                    {
                        TimeSpan inTime = record.TimeIn.Value;
                        TimeSpan outTime = nowTime;

                        // Tính giờ đơn giản để test
                        totalHours = (outTime - inTime).TotalHours;

                        // Logic trừ giờ nghỉ trưa nếu làm xuyên trưa (giữ lại logic này cũng được)
                        if (inTime < _lunchStart && outTime > _lunchEnd)
                        {
                            totalHours -= 1.0;
                        }
                    }

                    record.ActualHours = Math.Max(0, Math.Round(totalHours, 1));

                    await context.SaveChangesAsync();
                    MessageBox.Show($"Check-out thành công!\nTổng giờ công: {record.ActualHours}h.", "Hoàn thành", MessageBoxButton.OK, MessageBoxImage.Information);
                    await InitializeAsync();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi Check-out: " + ex.Message); }
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                using var context = new DataContext();
                string myId = UserSession.CurrentEmployeeId;

                var list = await context.TimeSheets
                    .AsNoTracking()
                    .Where(t => t.EmployeeID == myId)
                    .OrderByDescending(t => t.WorkDate)
                    .Take(30)
                    .ToListAsync();

                HistoryList.Clear();
                foreach (var item in list)
                {
                    string status;
                    string color;

                    if (item.TimeOut == null)
                    {
                        status = (item.WorkDate == DateTime.Today) ? "Đang làm việc" : "Quên Check-out";
                        color = (item.WorkDate == DateTime.Today) ? "#ECC94B" : "#A0AEC0";
                    }
                    else
                    {
                        // Logic xét trạng thái đơn giản hóa
                        status = "Hoàn thành";
                        color = "#38A169";
                    }

                    HistoryList.Add(new TimeSheetDTO
                    {
                        WorkDate = item.WorkDate,
                        CheckInText = item.TimeIn?.ToString(@"hh\:mm") ?? "--:--",
                        CheckOutText = item.TimeOut?.ToString(@"hh\:mm") ?? "--:--",
                        TotalHoursText = item.ActualHours > 0 ? $"{item.ActualHours}h" : "...",
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
            // Logic xuất báo cáo giữ nguyên như cũ
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
                    var allRecords = await context.TimeSheets
                        .Include(t => t.Employee)
                        .Where(t => t.WorkDate >= startOfMonth)
                        .OrderBy(t => t.EmployeeID)
                        .ThenBy(t => t.WorkDate)
                        .ToListAsync();

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
                        item.EmployeeID = currentUser.EmployeeID;
                        item.EmployeeName = currentUser.FullName;
                        dataToExport.Add(item);
                    }
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = exportAll ? "Báo Cáo Tổng Hợp" : "Báo Cáo Cá Nhân",
                    FileName = exportAll
                        ? $"TongHopChamCong_Thang{DateTime.Now:MM_yyyy}.xlsx"
                        : $"ChamCong_{AppSession.CurrentUser.EmployeeID}_{DateTime.Now:MM_yyyy}.xlsx"
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
                            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1A3D64");
                            headerRange.Style.Font.FontColor = XLColor.White;

                            int currentRow = 5;
                            foreach (var item in dataToExport)
                            {
                                worksheet.Cell(currentRow, 1).Value = item.EmployeeID;
                                worksheet.Cell(currentRow, 2).Value = item.EmployeeName;
                                worksheet.Cell(currentRow, 3).Value = item.WorkDate.ToString("dd/MM/yyyy");
                                worksheet.Cell(currentRow, 4).Value = item.CheckInText;
                                worksheet.Cell(currentRow, 5).Value = item.CheckOutText;
                                worksheet.Cell(currentRow, 6).Value = item.TotalHoursText;
                                worksheet.Cell(currentRow, 7).Value = item.StatusText;
                                currentRow++;
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
            if (item.TimeOut != null) status = "Hoàn thành";

            return new TimeSheetDTO
            {
                EmployeeID = emp.EmployeeID,
                EmployeeName = emp.FullName,
                WorkDate = item.WorkDate,
                CheckInText = item.TimeIn?.ToString(@"hh\:mm") ?? "--:--",
                CheckOutText = item.TimeOut?.ToString(@"hh\:mm") ?? "--:--",
                TotalHoursText = item.ActualHours > 0 ? $"{item.ActualHours}h" : "...",
                StatusText = status
            };
        }
    }
}