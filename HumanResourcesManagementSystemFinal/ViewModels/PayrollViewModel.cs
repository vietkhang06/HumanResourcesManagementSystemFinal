using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class PayrollViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<PayrollDTO> _payrollList;
    [ObservableProperty] private ObservableCollection<int> _months;
    [ObservableProperty] private ObservableCollection<int> _years;
    [ObservableProperty] private int _selectedMonth;
    [ObservableProperty] private int _selectedYear;
    [ObservableProperty] private decimal _totalSalaryFund;

    public PayrollViewModel()
    {
        PayrollList = new ObservableCollection<PayrollDTO>();
        Months = new ObservableCollection<int>(Enumerable.Range(1, 12));
        Years = new ObservableCollection<int>(Enumerable.Range(2023, 5));
        SelectedMonth = DateTime.Now.Month;
        SelectedYear = DateTime.Now.Year;
    }

    private string GetDeepErrorMessage(Exception ex)
    {
        var sb = new StringBuilder();
        sb.AppendLine(ex.Message);
        var inner = ex.InnerException;
        while (inner != null)
        {
            sb.AppendLine(inner.Message);
            inner = inner.InnerException;
        }
        return sb.ToString();
    }

    [RelayCommand]
    private async Task CalculatePayrollAsync()
    {
        try
        {
            using var context = new DataContext();

            // 1. Lọc nhân viên đang hoạt động (Status = "Active")
            var employees = await context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Include(e => e.WorkContracts)
                .Where(e => e.Status == "Active") // Sửa IsActive -> Status
                .ToListAsync();

            var startOfMonth = new DateTime(SelectedYear, SelectedMonth, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // 2. Lấy dữ liệu chấm công (WorkDate)
            var allTimeSheets = await context.TimeSheets
                .AsNoTracking()
                .Where(t => t.WorkDate >= startOfMonth && t.WorkDate <= endOfMonth) // Sửa Date -> WorkDate
                .ToListAsync();

            var resultList = new ObservableCollection<PayrollDTO>();

            foreach (var emp in employees)
            {
                // Lấy hợp đồng mới nhất
                var contract = emp.WorkContracts
                    .OrderByDescending(c => c.StartDate)
                    .FirstOrDefault();

                decimal baseSalary = contract?.Salary ?? 0;

                // 3. Tính toán công (EmployeeID string)
                var empTimeSheets = allTimeSheets.Where(t => t.EmployeeID == emp.EmployeeID).ToList();
                double workDays = empTimeSheets.Count;

                // 4. Tổng giờ làm (ActualHours)
                double totalHours = empTimeSheets.Sum(t => t.ActualHours ?? 0); // Sửa HoursWorked -> ActualHours

                decimal finalSalary = 0;
                if (baseSalary > 0)
                {
                    // Công thức ví dụ: (Lương / 26) * Ngày công thực tế
                    finalSalary = (baseSalary / 26m) * (decimal)workDays;
                }

                resultList.Add(new PayrollDTO
                {
                    EmployeeId = int.TryParse(emp.EmployeeID, out var empId) ? empId : 0,
                    FullName = emp.FullName, // Sửa LastName/FirstName -> FullName
                    DepartmentName = emp.Department?.DepartmentName ?? "N/A",
                    ContractSalary = baseSalary,
                    ActualWorkDays = workDays,
                    TotalHoursWorked = totalHours,
                    NetSalary = Math.Round(finalSalary, 0)
                });
            }

            PayrollList = resultList;
            TotalSalaryFund = PayrollList.Sum(x => x.NetSalary);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tính lương:\n" + GetDeepErrorMessage(ex), "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}