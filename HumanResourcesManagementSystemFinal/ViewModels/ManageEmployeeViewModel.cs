using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OfficeOpenXml;
using OfficeOpenXml.Style;
namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class ManageEmployeeViewModel : ObservableObject
    {
        private List<Employee> _allEmployees = new();
        public ObservableCollection<Employee> Employees { get; } = new();
        public ObservableCollection<Department> Departments { get; } = new();

        [ObservableProperty] private string _searchText;
        [ObservableProperty] private Department _selectedDepartment;
        [ObservableProperty] private Employee _selectedEmployee;

        public ManageEmployeeViewModel()
        {
            LoadDataFromDb();
        }

        public void LoadDataFromDb()
        {
            try
            {
                using var context = new DataContext();

                var deptList = context.Departments.ToList();
                deptList.Insert(0, new Department
                {
                    DepartmentID = "",
                    DepartmentName = "Tất cả"
                });

                Departments.Clear();
                foreach (var dept in deptList)
                    Departments.Add(dept);

                if (SelectedDepartment == null)
                    SelectedDepartment = Departments.FirstOrDefault();


                _allEmployees = context.Employees
                    .AsNoTracking()
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .Include(e => e.Manager)
                    .Include(e => e.WorkContracts)
                    .Include(e => e.Account)
                    .OrderByDescending(e => e.EmployeeID)
                    .ToList();

                var today = DateTime.Today;

                var todayTimeSheets = context.TimeSheets
                    .Where(t => t.WorkDate == today)
                    .ToList();

                foreach (var emp in _allEmployees)
                {
                    if (emp.Status == "Resigned" || emp.Status == "Đã nghỉ việc")
                        continue;

                    var timesheet = todayTimeSheets.FirstOrDefault(t => t.EmployeeID == emp.EmployeeID);

                    if (timesheet == null || timesheet.TimeIn == null)
                    {
                        emp.Status = "Chưa vào làm";
                    }
                    else if (timesheet.TimeOut != null)
                    {
                        emp.Status = "Đã tan làm";
                    }
                    else
                    {
                        emp.Status = "Đang làm việc";
                    }
                }

                FilterEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnSearchTextChanged(string value) => FilterEmployees();
        partial void OnSelectedDepartmentChanged(Department value) => FilterEmployees();

        private void FilterEmployees()
        {
            IEnumerable<Employee> query = _allEmployees;

            if (SelectedDepartment != null && !string.IsNullOrEmpty(SelectedDepartment.DepartmentID))
            {
                query = query.Where(e => e.DepartmentID == SelectedDepartment.DepartmentID);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string keyword = SearchText.ToLower();
                query = query.Where(e =>
                    (e.FullName ?? "").ToLower().Contains(keyword) ||
                    (e.Email ?? "").ToLower().Contains(keyword) ||
                    (e.EmployeeID ?? "").ToLower().Contains(keyword));
            }

            Employees.Clear();
            foreach (var emp in query)
                Employees.Add(emp);
        }

        [RelayCommand]
        private void AddEmployee() => ShowAddEmployeeWindow(null);

        [RelayCommand]
        private void EditEmployee(Employee emp)
        {
            if (emp != null) ShowAddEmployeeWindow(emp);
        }

        private void ShowAddEmployeeWindow(Employee existingEmp)
        {
            var addVM = existingEmp != null
                ? new AddEmployeeViewModel(existingEmp)
                : new AddEmployeeViewModel();

            var window = new AddEmployeeWindow { DataContext = addVM };

            if (window.ShowDialog() == true)
            {
                LoadDataFromDb();
            }
        }

        [RelayCommand]
        private async Task DeleteEmployee(Employee emp)
        {
            if (emp == null) return;

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa nhân viên {emp.FullName}?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using var context = new DataContext();
                var dbEmp = await context.Employees
                    .Include(e => e.Account)
                    .Include(e => e.WorkContracts)
                    .FirstOrDefaultAsync(e => e.EmployeeID == emp.EmployeeID);

                if (dbEmp != null)
                {
                    if (dbEmp.Account != null) context.Accounts.Remove(dbEmp.Account);
                    if (dbEmp.WorkContracts != null) context.WorkContracts.RemoveRange(dbEmp.WorkContracts);

                    var subs = context.Employees.Where(e => e.ManagerID == dbEmp.EmployeeID);
                    foreach (var sub in subs) sub.ManagerID = null;

                    context.ChangeHistories.Add(new ChangeHistory
                    {
                        LogID = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                        TableName = "Employees",
                        ActionType = "DELETE",
                        RecordID = dbEmp.EmployeeID,
                        ChangeByUserID = UserSession.CurrentEmployeeId ?? "ADMIN",
                        ChangeTime = DateTime.Now,
                        Details = $"Xóa nhân viên: {dbEmp.FullName}"
                    });

                    context.Employees.Remove(dbEmp);
                    await context.SaveChangesAsync();

                    LoadDataFromDb();
                    MessageBox.Show("Đã xóa thành công!", "Thông báo");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Lỗi CSDL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ViewDetail(Employee emp)
        {
            if (emp == null) return;

            var detailVM = new EmployeeDetailViewModel(emp);
            var detailWindow = new EmployeeDetailWindow
            {
                DataContext = detailVM
            };
            detailWindow.ShowDialog();
        }
        [RelayCommand]
        private void ExportToExcel()
        {
            if (Employees == null || Employees.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"DanhSachNhanVien_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (sfd.ShowDialog() != true) return;

            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Danh Sách Nhân Viên");

                    worksheet.Cells["A1:F1"].Merge = true;
                    worksheet.Cells["A1"].Value = "DANH SÁCH NHÂN VIÊN";
                    worksheet.Cells["A1"].Style.Font.Size = 18;
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Row(1).Height = 32;

                    worksheet.Cells["A2:F2"].Merge = true;
                    worksheet.Cells["A2"].Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    worksheet.Cells["A2"].Style.Font.Italic = true;
                    worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Row(2).Height = 22;

                    string[] headers = { "Mã NV", "Họ Tên", "Email", "Phòng Ban", "Chức Vụ", "Trạng Thái" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[3, i + 1].Value = headers[i];
                    }

                    using (var range = worksheet.Cells["A3:F3"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Font.Size = 12;
                        range.Style.Font.Color.SetColor(Color.White);

                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(
                            ColorTranslator.FromHtml("#22C55E"));

                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    worksheet.Row(3).Height = 26;

                    int rowIndex = 4;
                    foreach (var emp in Employees)
                    {
                        worksheet.Cells[rowIndex, 1].Value = emp.EmployeeID;
                        worksheet.Cells[rowIndex, 2].Value = emp.FullName;
                        worksheet.Cells[rowIndex, 3].Value = emp.Email;
                        worksheet.Cells[rowIndex, 4].Value = emp.Department?.DepartmentName ?? "N/A";
                        worksheet.Cells[rowIndex, 5].Value = emp.Position?.PositionName ?? "N/A";
                        worksheet.Cells[rowIndex, 6].Value = emp.Status;

                        if (emp.Status == "Chưa vào làm")
                        {
                            worksheet.Cells[rowIndex, 6].Style.Font.Color.SetColor(Color.Red);
                            worksheet.Cells[rowIndex, 6].Style.Font.Italic = true;
                        }
                        else if (emp.Status == "Đang làm việc")
                        {
                            worksheet.Cells[rowIndex, 6].Style.Font.Color.SetColor(Color.Green);
                        }

                        rowIndex++;
                    }

                    var dataRange = worksheet.Cells[3, 1, rowIndex - 1, 6];
                    var borderColor = ColorTranslator.FromHtml("#CBD5E1");

                    dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    dataRange.Style.Border.Top.Color.SetColor(borderColor);
                    dataRange.Style.Border.Bottom.Color.SetColor(borderColor);
                    dataRange.Style.Border.Left.Color.SetColor(borderColor);
                    dataRange.Style.Border.Right.Color.SetColor(borderColor);

                    worksheet.Cells.AutoFitColumns();
                    worksheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    FileInfo fileInfo = new FileInfo(sfd.FileName);
                    package.SaveAs(fileInfo);
                }

                MessageBox.Show("Xuất danh sách nhân viên sang Excel thành công!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}