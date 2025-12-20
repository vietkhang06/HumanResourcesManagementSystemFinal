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

            var employees = await context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Include(e => e.WorkContracts)
                .Where(e => e.IsActive)
                .ToListAsync();

            var startOfMonth = new DateTime(SelectedYear, SelectedMonth, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var allTimeSheets = await context.TimeSheets
                .AsNoTracking()
                .Where(t => t.Date >= startOfMonth && t.Date <= endOfMonth)
                .ToListAsync();

            var resultList = new ObservableCollection<PayrollDTO>();

            foreach (var emp in employees)
            {
                var contract = emp.WorkContracts
                    .OrderByDescending(c => c.StartDate)
                    .FirstOrDefault();

                decimal baseSalary = contract?.Salary ?? 0;

                var empTimeSheets = allTimeSheets.Where(t => t.EmployeeId == emp.Id).ToList();
                double workDays = empTimeSheets.Count;
                double totalHours = empTimeSheets.Sum(t => t.HoursWorked);

                decimal finalSalary = 0;
                if (baseSalary > 0)
                {
                    finalSalary = (baseSalary / 26m) * (decimal)workDays;
                }

                resultList.Add(new PayrollDTO
                {
                    EmployeeId = emp.Id,
                    FullName = $"{emp.LastName} {emp.FirstName}",
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