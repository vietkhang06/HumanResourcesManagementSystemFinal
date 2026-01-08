using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32; // Cần thêm cái này để dùng SaveFileDialog
using System;
using System.Collections.ObjectModel;
using System.IO;       // Cần thêm cái này để ghi file
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class PayrollViewModel : ObservableObject
    {
        // --- 1. CÁC BIẾN TỰ ĐỘNG ---
        [ObservableProperty] private ObservableCollection<PayrollDTO> _payrollList;
        [ObservableProperty] private ObservableCollection<int> _months;
        [ObservableProperty] private ObservableCollection<int> _years;
        [ObservableProperty] private decimal _totalSalaryFund;
        [ObservableProperty] private string _payrollStatus;
        [ObservableProperty] private string _statusColor;

        // --- 2. CÁC BIẾN THỦ CÔNG ---
        private int _selectedMonth;
        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (SetProperty(ref _selectedMonth, value))
                    _ = CalculatePayrollAsync();
            }
        }

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                    _ = CalculatePayrollAsync();
            }
        }

        // --- 3. CONSTRUCTOR ---
        public PayrollViewModel()
        {
            PayrollList = new ObservableCollection<PayrollDTO>();
            Months = new ObservableCollection<int>();
            Years = new ObservableCollection<int>();

            for (int i = 1; i <= 12; i++) Months.Add(i);

            int currentYear = DateTime.Now.Year;
            for (int i = currentYear; i >= 2015; i--) Years.Add(i);

            _selectedMonth = DateTime.Now.Month;
            _selectedYear = DateTime.Now.Year;

            _ = CalculatePayrollAsync();
        }

        // --- 4. HÀM TÍNH LƯƠNG (GIỮ NGUYÊN LOGIC CŨ) ---
        private async Task CalculatePayrollAsync()
        {
            try
            {
                // Logic kiểm tra tương lai
                var now = DateTime.Now;
                if (SelectedYear > now.Year || (SelectedYear == now.Year && SelectedMonth > now.Month))
                {
                    PayrollList = new ObservableCollection<PayrollDTO>();
                    TotalSalaryFund = 0;
                    PayrollStatus = "Chưa đến kỳ lương";
                    StatusColor = "#F59E0B";
                    return;
                }

                using var context = new DataContext();
                var startOfMonth = new DateTime(SelectedYear, SelectedMonth, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var employees = await context.Employees
                    .AsNoTracking()
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .Include(e => e.WorkContracts)
                    .ToListAsync();

                var allTimeSheets = await context.TimeSheets
                    .AsNoTracking()
                    .Where(t => t.WorkDate >= startOfMonth && t.WorkDate <= endOfMonth)
                    .ToListAsync();

                var resultList = new ObservableCollection<PayrollDTO>();
                decimal grandTotal = 0;

                foreach (var emp in employees)
                {
                    var historicalContract = emp.WorkContracts
                        .Where(c => c.StartDate <= endOfMonth)
                        .OrderByDescending(c => c.StartDate)
                        .FirstOrDefault();

                    if (historicalContract == null) continue;
                    if (historicalContract.EndDate.HasValue && historicalContract.EndDate.Value < startOfMonth) continue;

                    decimal baseSalary = historicalContract.Salary ?? 0;
                    var empTimeSheets = allTimeSheets.Where(t => t.EmployeeID == emp.EmployeeID).ToList();
                    double workDays = empTimeSheets.Count;

                    decimal actualSalary = 0;
                    if (baseSalary > 0)
                        actualSalary = (baseSalary / 26m) * (decimal)workDays;

                    decimal totalIncome = Math.Round(actualSalary, 0);
                    decimal deduction = 0;
                    decimal netSalary = totalIncome - deduction;

                    grandTotal += netSalary;

                    resultList.Add(new PayrollDTO
                    {
                        EmployeeID = emp.EmployeeID,
                        FullName = emp.FullName,
                        DepartmentName = emp.Department?.DepartmentName ?? "N/A",
                        PositionName = emp.Position?.PositionName ?? "N/A",
                        ContractSalary = Math.Round(baseSalary, 0),
                        ActualWorkDays = workDays,
                        TotalIncome = totalIncome,
                        TotalDeduction = deduction,
                        NetSalary = netSalary
                    });
                }

                PayrollList = resultList;
                TotalSalaryFund = grandTotal;

                if (PayrollList.Count > 0)
                {
                    if (SelectedMonth == now.Month && SelectedYear == now.Year)
                    {
                        PayrollStatus = "Dự tính";
                        StatusColor = "#10B981";
                    }
                    else
                    {
                        PayrollStatus = "Đã tính toán";
                        StatusColor = "#3B82F6";
                    }
                }
                else
                {
                    PayrollStatus = "Không có dữ liệu";
                    StatusColor = "#94A3B8";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        // --- 5. HÀM XUẤT EXCEL (CSV) ---
        [RelayCommand]
        private void ExportToExcel()
        {
            if (PayrollList == null || PayrollList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Mở hộp thoại chọn nơi lưu
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv",
                    FileName = $"BangLuong_Thang{SelectedMonth}_{SelectedYear}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var sb = new StringBuilder();

                    // 1. Tiêu đề cột
                    sb.AppendLine("Mã NV,Họ Tên,Phòng Ban,Chức Vụ,Ngày Công,Lương HĐ,Tổng Thu,Khấu Trừ,Thực Lĩnh");

                    // 2. Dữ liệu
                    foreach (var item in PayrollList)
                    {
                        // Lưu ý: Cần xử lý dấu phẩy trong tên nếu có (bằng cách đặt trong ngoặc kép)
                        sb.AppendLine($"{item.EmployeeID},\"{item.FullName}\",\"{item.DepartmentName}\",\"{item.PositionName}\",{item.ActualWorkDays},{item.ContractSalary},{item.TotalIncome},{item.TotalDeduction},{item.NetSalary}");
                    }

                    // 3. Ghi file với Encoding UTF8 (có BOM) để Excel hiển thị đúng tiếng Việt
                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}