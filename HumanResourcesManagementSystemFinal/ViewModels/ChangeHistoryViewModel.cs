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
        [ObservableProperty]
        private ObservableCollection<ChangeHistory> _histories = new();

        [ObservableProperty]
        private string _keyword;

        [ObservableProperty]
        private DateTime? _startDate = DateTime.Today.AddDays(-30);

        [ObservableProperty]
        private DateTime? _endDate = DateTime.Today;

        [ObservableProperty]
        private string _selectedActionType;

        public ObservableCollection<string> ActionTypes { get; } = new() { "Tất cả", "CREATE", "UPDATE", "DELETE", "LOGIN" };

        [ObservableProperty]
        private bool _isLoading;

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
                            .Include(h => h.Account)
                            .ThenInclude(a => a.Employee)
                            .AsQueryable();

                        if (StartDate.HasValue)
                            query = query.Where(h => h.ChangeTime >= StartDate.Value);

                        if (EndDate.HasValue)
                            query = query.Where(h => h.ChangeTime < EndDate.Value.AddDays(1));

                        if (!string.IsNullOrEmpty(SelectedActionType) && SelectedActionType != "Tất cả")
                        {
                            query = query.Where(h => h.ActionType == SelectedActionType);
                        }

                        if (!string.IsNullOrWhiteSpace(Keyword))
                        {
                            string k = Keyword.ToLower();
                            query = query.Where(h =>
                                (h.Account != null && h.Account.Username.ToLower().Contains(k)) ||
                                h.TableName.ToLower().Contains(k) ||
                                (h.Details != null && h.Details.ToLower().Contains(k))
                            );
                        }

                        var list = await query
                            .OrderByDescending(h => h.ChangeTime)
                            .Take(200)
                            .ToListAsync();

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Histories = new ObservableCollection<ChangeHistory>(list);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
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

        private void ShowErrorMessage(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Lỗi hệ thống:");
            sb.AppendLine(ex.Message);

            var inner = ex.InnerException;
            while (inner != null)
            {
                sb.AppendLine("--- Inner Exception ---");
                sb.AppendLine(inner.Message);
                inner = inner.InnerException;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(sb.ToString(), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        [RelayCommand]
        public async Task ExportExcelAsync()
        {
            if (Histories == null || Histories.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Xuất Nhật Ký Hoạt Động",
                    FileName = $"NhatKyHoatDong_{DateTime.Now:ddMMyyyy_HHmm}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    await Task.Run(() =>
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("NhatKy");

                            var titleRange = worksheet.Range("A1:E1");
                            titleRange.Merge().Value = "NHẬT KÝ HOẠT ĐỘNG HỆ THỐNG";
                            titleRange.Style.Font.FontSize = 16;
                            titleRange.Style.Font.Bold = true;
                            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            titleRange.Style.Fill.BackgroundColor = XLColor.White;

                            worksheet.Cell("A2").Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";

                            int row = 4;
                            worksheet.Cell(row, 1).Value = "Thời Gian";
                            worksheet.Cell(row, 2).Value = "Người Thực Hiện";
                            worksheet.Cell(row, 3).Value = "Hành Động";
                            worksheet.Cell(row, 4).Value = "Bảng Dữ Liệu";
                            worksheet.Cell(row, 5).Value = "Chi Tiết";

                            var header = worksheet.Range(row, 1, row, 5);
                            header.Style.Font.Bold = true;
                            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#1A3D64");
                            header.Style.Font.FontColor = XLColor.White;
                            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            foreach (var item in Histories)
                            {
                                row++;
                                worksheet.Cell(row, 1).Value = item.ChangeTime;
                                worksheet.Cell(row, 1).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

                                string performer = "Hệ thống";
                                if (item.Account != null)
                                {
                                    if (item.Account.Employee != null)
                                    {
                                        performer = $"{item.Account.Employee.FirstName} {item.Account.Employee.LastName} ({item.Account.Username})";
                                    }
                                    else
                                    {
                                        performer = item.Account.Username;
                                    }
                                }
                                worksheet.Cell(row, 2).Value = performer;

                                worksheet.Cell(row, 3).Value = item.ActionType;
                                worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                                if (item.ActionType == "DELETE") worksheet.Cell(row, 3).Style.Font.FontColor = XLColor.Red;
                                else if (item.ActionType == "CREATE") worksheet.Cell(row, 3).Style.Font.FontColor = XLColor.Green;
                                else worksheet.Cell(row, 3).Style.Font.FontColor = XLColor.Blue;

                                worksheet.Cell(row, 4).Value = item.TableName;
                                worksheet.Cell(row, 5).Value = item.Details;
                                worksheet.Cell(row, 5).Style.Alignment.WrapText = true;
                            }

                            worksheet.Column(1).Width = 20;
                            worksheet.Column(2).Width = 35;
                            worksheet.Column(3).Width = 15;
                            worksheet.Column(4).Width = 20;
                            worksheet.Column(5).Width = 60;

                            var dataRange = worksheet.Range(4, 1, row, 5);
                            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                            workbook.SaveAs(saveFileDialog.FileName);
                        }
                    });

                    MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}