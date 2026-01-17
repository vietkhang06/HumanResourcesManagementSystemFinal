using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
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

        // --- BỔ SUNG: PHÂN QUYỀN VÀ DỮ LIỆU NHÂN VIÊN ---
        [ObservableProperty] private bool _isAdmin = true; // Fix lỗi IsAdmin
        [ObservableProperty] private PayrollDTO _currentEmployeePayroll; // Fix lỗi CurrentEmployeePayroll
        [ObservableProperty] private decimal _advancePayment = 1000000; // Tạm ứng mặc định

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

        private readonly string _targetEmployeeId; // Lưu ID nhân viên cần xem

        // --- 3. CONSTRUCTOR ---

        // Constructor mặc định cho Admin
        public PayrollViewModel() : this(null) { }

        // Constructor cho Nhân viên cụ thể
        public PayrollViewModel(string employeeId)
        {
            _targetEmployeeId = employeeId;
            IsAdmin = string.IsNullOrEmpty(employeeId); // Nếu có ID truyền vào thì IsAdmin = false

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

        // --- 4. HÀM TÍNH LƯƠNG ---
        private async Task CalculatePayrollAsync()
        {
            try
            {
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

                // Lọc nhân viên: Nếu có ID thì chỉ lấy 1 người, nếu không lấy tất cả (Admin)
                var query = context.Employees.AsNoTracking()
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .Include(e => e.WorkContracts)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(_targetEmployeeId)) // Fix lỗi so sánh string/int
                {
                    query = query.Where(e => e.EmployeeID == _targetEmployeeId);
                }

                var employees = await query.ToListAsync();

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

                    decimal actualSalary = baseSalary > 0 ? (baseSalary / 26m) * (decimal)workDays : 0;
                    decimal totalIncome = Math.Round(actualSalary, 0);
                    decimal netSalary = totalIncome; // Giả định chưa có khấu trừ

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
                        NetSalary = netSalary
                    });
                }

                PayrollList = resultList;
                TotalSalaryFund = grandTotal;

                // Cập nhật dữ liệu cho giao diện nhân viên
                if (!IsAdmin && resultList.Count > 0)
                {
                    CurrentEmployeePayroll = resultList[0];
                }

                // Cập nhật trạng thái
                if (PayrollList.Count > 0)
                {
                    PayrollStatus = (SelectedMonth == now.Month && SelectedYear == now.Year) ? "Dự tính" : "Đã tính toán";
                    StatusColor = (SelectedMonth == now.Month && SelectedYear == now.Year) ? "#10B981" : "#3B82F6";
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

        // --- COMMANDS ĐIỀU HƯỚNG THÁNG ---
        [RelayCommand]
        private void PreviousMonth()
        {
            if (SelectedMonth == 1) { SelectedMonth = 12; SelectedYear--; }
            else SelectedMonth--;
        }

        [RelayCommand]
        private void NextMonth()
        {
            if (SelectedMonth == 12) { SelectedMonth = 1; SelectedYear++; }
            else SelectedMonth++;
        }

        [RelayCommand]
        private void ExportToExcel()
        {
            if (PayrollList == null || PayrollList.Count == 0) return;

            SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = $"Payroll_{SelectedMonth}_{SelectedYear}.csv" };
            if (sfd.ShowDialog() == true)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Mã NV,Họ Tên,Phòng Ban,Ngày Công,Thực Lĩnh");
                foreach (var item in PayrollList)
                {
                    sb.AppendLine($"{item.EmployeeID},{item.FullName},{item.DepartmentName},{item.ActualWorkDays},{item.NetSalary}");
                }
                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Xuất thành công!");
            }
        }
    }
}