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
using ClosedXML.Excel;
using Microsoft.Win32;

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

        public ObservableCollection<string> ActionTypes { get; } = new() { "Tất cả", "CREATE", "UPDATE", "DELETE", "LOGIN" };

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
                    using (var context = new DataContext())
                    {
                        var query = context.ChangeHistories
                            .AsNoTracking()
                            .Include(h => h.ChangeByUser) // Nối bảng để lấy tên
                            .AsQueryable();

                        if (StartDate.HasValue)
                            query = query.Where(h => h.ChangeTime.Date >= StartDate.Value.Date);

                        if (EndDate.HasValue)
                            query = query.Where(h => h.ChangeTime.Date <= EndDate.Value.Date);

                        if (!string.IsNullOrEmpty(SelectedActionType) && SelectedActionType != "Tất cả")
                            query = query.Where(h => h.ActionType == SelectedActionType);

                        if (!string.IsNullOrWhiteSpace(Keyword))
                        {
                            string k = Keyword.ToLower();
                            query = query.Where(h =>
                                (h.ChangeByUser != null && h.ChangeByUser.FullName.ToLower().Contains(k)) ||
                                (h.ChangeByUserID != null && h.ChangeByUserID.ToLower().Contains(k)) ||
                                (h.TableName != null && h.TableName.ToLower().Contains(k)) ||
                                (h.Details != null && h.Details.ToLower().Contains(k))
                            );
                        }

                        var list = await query.OrderByDescending(h => h.ChangeTime).Take(200).ToListAsync();

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Histories = new ObservableCollection<ChangeHistory>(list);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message));
            }
            finally { IsLoading = false; }
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
            // 1. Kiểm tra dữ liệu
            if (Histories == null || Histories.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 2. Cấu hình hộp thoại lưu file
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Xuất Nhật Ký Hoạt Động",
                    FileName = $"NhatKyHoatDong_{DateTime.Now:ddMMyyyy_HHmm}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // 3. Chạy tác vụ nền để không đơ giao diện
                    await Task.Run(() =>
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("NhatKy");

                            // --- TẠO HEADER TIÊU ĐỀ ---
                            var titleRange = worksheet.Range("A1:E1");
                            titleRange.Merge().Value = "NHẬT KÝ HOẠT ĐỘNG HỆ THỐNG";
                            titleRange.Style.Font.FontSize = 16;
                            titleRange.Style.Font.Bold = true;
                            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            worksheet.Cell("A2").Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";

                            // --- TẠO CỘT HEADER BẢNG ---
                            int row = 4;
                            worksheet.Cell(row, 1).Value = "Thời Gian";
                            worksheet.Cell(row, 2).Value = "Người Thực Hiện";
                            worksheet.Cell(row, 3).Value = "Hành Động";
                            worksheet.Cell(row, 4).Value = "Bảng Dữ Liệu";
                            worksheet.Cell(row, 5).Value = "Chi Tiết";

                            // Style cho dòng Header
                            var headerRange = worksheet.Range(row, 1, row, 5);
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1A3D64"); // Màu xanh đậm
                            headerRange.Style.Font.FontColor = XLColor.White;
                            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            // --- ĐỔ DỮ LIỆU ---
                            foreach (var item in Histories)
                            {
                                row++;

                                // Cột 1: Thời gian
                                worksheet.Cell(row, 1).Value = item.ChangeTime;
                                worksheet.Cell(row, 1).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

                                // Cột 2: Người thực hiện (Logic ưu tiên Tên -> Mã -> Hệ thống)
                                string performer = "Hệ thống";
                                if (item.ChangeByUser != null)
                                {
                                    performer = $"{item.ChangeByUser.FullName} ({item.ChangeByUserID})";
                                }
                                else if (!string.IsNullOrEmpty(item.ChangeByUserID))
                                {
                                    performer = item.ChangeByUserID;
                                }
                                worksheet.Cell(row, 2).Value = performer;

                                // Cột 3: Hành động (Tô màu)
                                worksheet.Cell(row, 3).Value = item.ActionType;
                                worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                                if (item.ActionType == "DELETE")
                                    worksheet.Cell(row, 3).Style.Font.FontColor = XLColor.Red;
                                else if (item.ActionType == "CREATE")
                                    worksheet.Cell(row, 3).Style.Font.FontColor = XLColor.Green;
                                else
                                    worksheet.Cell(row, 3).Style.Font.FontColor = XLColor.Blue;

                                // Cột 4: Tên bảng
                                worksheet.Cell(row, 4).Value = item.TableName;

                                // Cột 5: Chi tiết (Tự động xuống dòng)
                                worksheet.Cell(row, 5).Value = item.Details;
                                worksheet.Cell(row, 5).Style.Alignment.WrapText = true;
                            }

                            // --- ĐỊNH DẠNG CỘT & VIỀN ---
                            worksheet.Column(1).Width = 20;
                            worksheet.Column(2).Width = 35;
                            worksheet.Column(3).Width = 15;
                            worksheet.Column(4).Width = 20;
                            worksheet.Column(5).Width = 60; // Cột chi tiết rộng hơn

                            // Kẻ khung viền cho toàn bộ bảng dữ liệu
                            var dataTableRange = worksheet.Range(4, 1, row, 5);
                            dataTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            dataTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                            workbook.SaveAs(saveFileDialog.FileName);
                        }
                    });

                    MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}