using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class PayrollViewModel : ObservableObject
{
    public ObservableCollection<PayrollDisplayItem> PayrollList { get; set; } = new();

    [ObservableProperty] private int _selectedMonth = DateTime.Now.Month;
    [ObservableProperty] private int _selectedYear = DateTime.Now.Year;
    [ObservableProperty] private decimal _totalSalaryFund;

    public PayrollViewModel()
    {
        if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
        {
            return;
        }
        CalculatePayroll();
    }

    [RelayCommand]
    private void CalculatePayroll()
    {
        PayrollList.Clear();
        using var context = new DataContext();
        var contracts = context.WorkContracts
            .Include(c => c.Employee)
            .ThenInclude(e => e.Department)
            .ToList();

        var timeSheets = context.TimeSheets
            .Where(t => t.Date.Month == SelectedMonth && t.Date.Year == SelectedYear)
            .ToList();

        foreach (var contract in contracts)
        {
            double totalHours = timeSheets
                .Where(t => t.EmployeeId == contract.EmployeeId)
                .Sum(t => t.HoursWorked);

            double workDays = totalHours / 8.0;

            var payrollItem = new PayrollDisplayItem
            {
                EmployeeId = contract.EmployeeId,
                FullName = contract.Employee?.FullName ?? "Unknown",
                DepartmentName = contract.Employee?.Department?.DepartmentName ?? "N/A",
                ContractSalary = contract.Salary,
                TotalHoursWorked = totalHours,
                ActualWorkDays = Math.Round(workDays, 1) // Làm tròn 1 số lẻ
            };

            PayrollList.Add(payrollItem);
        }
        TotalSalaryFund = PayrollList.Sum(x => x.NetSalary);
    }
}