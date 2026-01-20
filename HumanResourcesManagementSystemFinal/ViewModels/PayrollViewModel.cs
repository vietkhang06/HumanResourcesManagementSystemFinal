using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        [ObservableProperty] private ObservableCollection<PayrollDTO> _payrollList;
        [ObservableProperty] private ObservableCollection<int> _months;
        [ObservableProperty] private ObservableCollection<int> _years;
        [ObservableProperty] private decimal _totalSalaryFund;
        [ObservableProperty] private string _payrollStatus;
        [ObservableProperty] private string _statusColor;

        [ObservableProperty] private bool _isAdmin = true;
        [ObservableProperty] private PayrollDTO _currentEmployeePayroll;
        [ObservableProperty] private decimal _advancePayment = 0;

        [ObservableProperty] private LiveCharts.SeriesCollection _incomeSeries;
        [ObservableProperty] private string[] _incomeLabels;
        public PayrollViewModel() : this(null) { }

        public PayrollViewModel(string employeeId)
        {
            _targetEmployeeId = employeeId;
            IsAdmin = string.IsNullOrEmpty(employeeId);

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

        private bool _isFilterPopupOpen;
        public bool IsFilterPopupOpen
        {
            get => _isFilterPopupOpen;
            set => SetProperty(ref _isFilterPopupOpen, value);
        }

        private string _minSalary;
        public string MinSalary
        {
            get => _minSalary;
            set => SetProperty(ref _minSalary, value);
        }

        private string _maxSalary;
        public string MaxSalary
        {
            get => _maxSalary;
            set => SetProperty(ref _maxSalary, value);
        }

        // Lưu trữ danh sách gốc để phục vụ lọc dữ liệu
        private List<PayrollDTO> _allPayrolls = new List<PayrollDTO>();


        [RelayCommand]
        private void ToggleFilter()
        {
            IsFilterPopupOpen = !IsFilterPopupOpen;
        }

        [RelayCommand]
        private void ApplyFilter()
        {
            if (decimal.TryParse(MinSalary, out decimal min) && decimal.TryParse(MaxSalary, out decimal max))
            {
                var filtered = _allPayrolls.Where(p => p.NetSalary >= min && p.NetSalary <= max).ToList();
                PayrollList = new ObservableCollection<PayrollDTO>(filtered);
            }
            IsFilterPopupOpen = false;
        }

        [RelayCommand]
        private void ResetFilter()
        {
            MinSalary = string.Empty;
            MaxSalary = string.Empty;
            PayrollList = new ObservableCollection<PayrollDTO>(_allPayrolls);
            IsFilterPopupOpen = false;
        }

        [RelayCommand]
        private void PreviousMonth()
        {
            if (SelectedMonth > 1)
                SelectedMonth--;
            else
            {
                SelectedMonth = 12;
                SelectedYear--;
            }
        }

        [RelayCommand]
        private void NextMonth()
        {
            if (SelectedMonth < 12)
                SelectedMonth++;
            else
            {
                SelectedMonth = 1;
                SelectedYear++;
            }
        }

        private async Task LoadChartDataAsync()
        {
            if (string.IsNullOrEmpty(_targetEmployeeId)) return;

            var values = new LiveCharts.ChartValues<decimal>();
            var labels = new List<string>();

            using var context = new DataContext();

            for (int i = 4; i >= 0; i--)
            {
                DateTime targetDate = new DateTime(SelectedYear, SelectedMonth, 1).AddMonths(-i);
                int m = targetDate.Month;
                int y = targetDate.Year;

                var monthData = await context.Payrolls
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.EmployeeID == _targetEmployeeId
                                         && p.Month == m
                                         && p.Year == y);

                values.Add(monthData?.NetSalary ?? 0);
                labels.Add($"T{m}/{y}");
            }

            IncomeSeries = new LiveCharts.SeriesCollection
        {
            new LiveCharts.Wpf.ColumnSeries {
                Title = "Thực lĩnh",
                Values = values,
                Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#67E8F9"),
                MaxColumnWidth = 25
            }
        };
            IncomeLabels = labels.ToArray();
        }
        public Func<double, string> YFormatter { get; set; } = value => value.ToString("N0") + " đ";

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

        private readonly string _targetEmployeeId;


        private async Task CalculatePayrollAsync()
        {
            try
            {
                int targetMonth = SelectedMonth;
                int targetYear = SelectedYear;
                string empId = _targetEmployeeId?.ToUpper(); 

                using var context = new DataContext();

                var dbPayrolls = await context.Payrolls
                    .AsNoTracking()
                    .Where(p => p.Month == targetMonth && p.Year == targetYear)
                    .ToListAsync();

                var resultList = new List<PayrollDTO>();
                decimal grandTotal = 0;

                foreach (var p in dbPayrolls)
                {
                    var emp = await context.Employees
                        .Include(e => e.Account)
                        .Include(e => e.Department)
                        .Include(e => e.Position)
                        .FirstOrDefaultAsync(e => e.EmployeeID == p.EmployeeID);

                    if (emp == null) continue;
                    var dto = new PayrollDTO
                    {
                        EmployeeID = p.EmployeeID,
                        FullName = emp.FullName,
                        ContractSalary = p.BasicSalary,
                        ActualWorkDays = p.WorkingDays,
                        AllowanceAndBonus = p.Allowance + p.Bonus,

                        TotalDeduction = p.Deductions,
                        TotalIncome = Math.Round((p.BasicSalary / 26m * (decimal)p.WorkingDays) + p.Allowance + p.Bonus, 0),
                        NetSalary = Math.Round(((p.BasicSalary / 26m * (decimal)p.WorkingDays) + p.Allowance + p.Bonus) - p.Deductions - AdvancePayment, 0),

                        AvatarData = emp.Account?.AvatarData
                    };
                    resultList.Add(dto);
                    grandTotal += p.NetSalary;
                }

                // 3. Cập nhật danh sách hiển thị cho Admin
                _allPayrolls = resultList;
                PayrollList = new ObservableCollection<PayrollDTO>(resultList);
                TotalSalaryFund = grandTotal;

                // 4. LIÊN KẾT DATA VÀO PHIẾU LƯƠNG CÁ NHÂN (CurrentEmployeePayroll)
                if (!string.IsNullOrEmpty(_targetEmployeeId))
                {
                    var myPayroll = resultList.FirstOrDefault(p => p.EmployeeID == _targetEmployeeId);

                    CurrentEmployeePayroll = myPayroll ?? new PayrollDTO
                    {
                        EmployeeID = _targetEmployeeId,
                        FullName = "N/A",
                        NetSalary = 0,
                        ContractSalary = 0,
                        TotalIncome = 0,
                        TotalDeduction = 0,
                        ActualWorkDays = 0
                    };
                }
                else if (resultList.Count > 0)
                {
                    // Nếu là Admin, xem mặc định người đầu tiên
                    CurrentEmployeePayroll = resultList[0];
                }

                // 5. Cập nhật trạng thái
                PayrollStatus = dbPayrolls.Any() ? "Đã chốt lương" : "Dự tính (Chưa nạp)";
                StatusColor = dbPayrolls.Any() ? "#3B82F6" : "#F59E0B";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hiển thị lương: " + ex.Message);
            }
            await LoadChartDataAsync();
        }


        [RelayCommand]
        private void ExportToExcel()
        {
            if (PayrollList == null || PayrollList.Count == 0) return;
            // Giữ nguyên logic export cũ...
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