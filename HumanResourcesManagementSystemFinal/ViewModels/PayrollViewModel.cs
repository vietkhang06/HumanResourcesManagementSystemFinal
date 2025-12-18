using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models; 
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using HumanResourcesManagementSystemFinal.Data;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class PayrollViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<PayrollDTO> _payrollList;

        [ObservableProperty]
        private ObservableCollection<int> _months;

        [ObservableProperty]
        private ObservableCollection<int> _years;

        [ObservableProperty]
        private int _selectedMonth;

        [ObservableProperty]
        private int _selectedYear;

        [ObservableProperty]
        private decimal _totalSalaryFund;

        public PayrollViewModel()
        {
            PayrollList = new ObservableCollection<PayrollDTO>();
            Months = new ObservableCollection<int>(Enumerable.Range(1, 12));
            Years = new ObservableCollection<int>(Enumerable.Range(2023, 5));
            SelectedMonth = DateTime.Now.Month;
            SelectedYear = DateTime.Now.Year;
        }

        [RelayCommand]
        private void CalculatePayroll()
        {
            try
            {
                using (var context = new DataContext())
                {
                    var resultList = new ObservableCollection<PayrollDTO>();
                    var employees = context.Employees.Include(e => e.Department).Where(e => e.IsActive).ToList();
                    foreach (var emp in employees)
                    {
                        var contract = context.WorkContracts.Where(c => c.EmployeeId == emp.Id).OrderByDescending(c => c.StartDate).FirstOrDefault();
                        decimal baseSalary = contract != null ? contract.Salary : 0;
                        var timesheets = context.TimeSheets.Where(t => t.EmployeeId == emp.Id&& t.Date.Month == SelectedMonth&& t.Date.Year == SelectedYear).ToList();
                        double workDays = timesheets.Count;
                        double totalHours = timesheets.Sum(t => t.HoursWorked);
                        decimal finalSalary = 0;
                        if (baseSalary > 0)
                        {
                            finalSalary = (baseSalary / 26m) * (decimal)workDays;
                        }
                        resultList.Add(new PayrollDTO
                        {
                            EmployeeId = emp.Id,
                            FullName = $"{emp.LastName} {emp.FirstName}", // Hien thi ten
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tính lương: " + ex.Message);
            }
        }
    }
}