using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ClosedXML.Excel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class ChangeHistoryViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<ChangeHistory> _histories = new();
        [ObservableProperty] private string _keyword;
        [ObservableProperty] private DateTime? _startDate = DateTime.Today.AddDays(-30);
        [ObservableProperty] private DateTime? _endDate = DateTime.Today;
        [ObservableProperty] private string _selectedActionType;
        [ObservableProperty] private bool _isLoading;

        public ObservableCollection<string> ActionTypes { get; } =
            new() { "Tất cả", "CREATE", "UPDATE", "DELETE", "LOGIN" };

        public ChangeHistoryViewModel()
        {
            SelectedActionType = "Tất cả";
            _ = LoadDataAsync();
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            Histories.Clear();

            try
            {
                await Task.Run(async () =>
                {
                    using var context = new DataContext();

                    var query = context.ChangeHistories
                        .AsNoTracking()
                        .Include(h => h.ChangeByUser)
                        .AsQueryable();

                    if (StartDate.HasValue)
                        query = query.Where(h => h.ChangeTime.Date >= StartDate.Value.Date);

                    if (EndDate.HasValue)
                        query = query.Where(h => h.ChangeTime.Date <= EndDate.Value.Date);

                    if (!string.IsNullOrEmpty(SelectedActionType) &&
                        SelectedActionType != "Tất cả")
                        query = query.Where(h => h.ActionType == SelectedActionType);

                    if (!string.IsNullOrWhiteSpace(Keyword))
                    {
                        string k = Keyword.ToLower();
                        query = query.Where(h =>
                            (h.ChangeByUser != null && h.ChangeByUser.FullName.ToLower().Contains(k)) ||
                            (h.ChangeByUserID != null && h.ChangeByUserID.ToLower().Contains(k)) ||
                            (h.TableName != null && h.TableName.ToLower().Contains(k)) ||
                            (h.Details != null && h.Details.ToLower().Contains(k)));
                    }

                    var list = await query
                        .OrderByDescending(h => h.ChangeTime)
                        .Take(200)
                        .ToListAsync();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Histories = new ObservableCollection<ChangeHistory>(list);
                    });
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message));
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task ClearFilterAsync()
        {
            Keyword = string.Empty;
            SelectedActionType = "Tất cả";
            StartDate = DateTime.Today.AddDays(-30);
            EndDate = DateTime.Today;
            await LoadDataAsync();
        }

        [RelayCommand]
        public async Task ExportExcelAsync()
        {
            if (Histories == null || Histories.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!");
                return;
            }

            try
            {
                SaveFileDialog dialog = new()
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Xuất Nhật Ký Hoạt Động",
                    FileName = $"NhatKyHoatDong_{DateTime.Now:ddMMyyyy_HHmm}.xlsx"
                };

                if (dialog.ShowDialog() != true) return;

                await Task.Run(() =>
                {
                    using var workbook = new XLWorkbook();
                    var ws = workbook.Worksheets.Add("NhatKy");

                    ws.Range("A1:E1").Merge().Value = "NHẬT KÝ HOẠT ĐỘNG HỆ THỐNG";
                    ws.Range("A1:E1").Style.Font.Bold = true;
                    ws.Range("A1:E1").Style.Font.FontSize = 16;
                    ws.Range("A1:E1").Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    ws.Cell("A2").Value =
                        $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";

                    int row = 4;
                    ws.Cell(row, 1).Value = "Thời Gian";
                    ws.Cell(row, 2).Value = "Người Thực Hiện";
                    ws.Cell(row, 3).Value = "Hành Động";
                    ws.Cell(row, 4).Value = "Bảng Dữ Liệu";
                    ws.Cell(row, 5).Value = "Chi Tiết";

                    ws.Range(row, 1, row, 5).Style.Font.Bold = true;
                    ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor =
                        XLColor.FromHtml("#1A3D64");
                    ws.Range(row, 1, row, 5).Style.Font.FontColor = XLColor.White;

                    foreach (var item in Histories)
                    {
                        row++;

                        ws.Cell(row, 1).Value = item.ChangeTime;
                        ws.Cell(row, 1).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

                        string performer = item.ChangeByUser != null
                            ? $"{item.ChangeByUser.FullName} ({item.ChangeByUserID})"
                            : item.ChangeByUserID ?? "Hệ thống";

                        ws.Cell(row, 2).Value = performer;
                        ws.Cell(row, 3).Value = item.ActionType;
                        ws.Cell(row, 4).Value = item.TableName;
                        ws.Cell(row, 5).Value = item.Details;
                        ws.Cell(row, 5).Style.Alignment.WrapText = true;
                    }

                    ws.Columns().AdjustToContents();
                    ws.Range(4, 1, row, 5).Style.Border.OutsideBorder =
                        XLBorderStyleValues.Thin;
                    ws.Range(4, 1, row, 5).Style.Border.InsideBorder =
                        XLBorderStyleValues.Thin;

                    workbook.SaveAs(dialog.FileName);
                });

                MessageBox.Show("Xuất file thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file: " + ex.Message);
            }
        }
    }
}
