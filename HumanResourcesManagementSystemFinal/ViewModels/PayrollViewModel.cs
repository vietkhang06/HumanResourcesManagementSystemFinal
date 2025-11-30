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

    // Các biến chọn tháng/năm
    [ObservableProperty] private int _selectedMonth = DateTime.Now.Month;
    [ObservableProperty] private int _selectedYear = DateTime.Now.Year;

    // Biến tổng hiển thị trên Dashboard lương
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

        // 1. Lấy tất cả Hợp đồng đang có hiệu lực
        // (Kèm thông tin Nhân viên và Phòng ban để hiển thị tên)
        var contracts = context.WorkContracts
            .Include(c => c.Employee)
            .ThenInclude(e => e.Department)
            .ToList();

        // 2. Lấy dữ liệu chấm công của tháng/năm được chọn
        var timeSheets = context.TimeSheets
            .Where(t => t.Date.Month == SelectedMonth && t.Date.Year == SelectedYear)
            .ToList();

        // 3. Duyệt từng hợp đồng để tính lương
        foreach (var contract in contracts)
        {
            // Tính tổng giờ làm của nhân viên này trong tháng
            // Lọc trong list timeSheets đã lấy ở trên
            double totalHours = timeSheets
                .Where(t => t.EmployeeId == contract.EmployeeId)
                .Sum(t => t.HoursWorked);

            // Quy đổi giờ sang ngày công (Giả sử 1 ngày làm 8 tiếng)
            double workDays = totalHours / 8.0;

            // Tạo dòng dữ liệu hiển thị
            var payrollItem = new PayrollDisplayItem
            {
                EmployeeId = contract.EmployeeId,
                FullName = contract.Employee?.FullName ?? "Unknown",
                DepartmentName = contract.Employee?.Department?.DepartmentName ?? "N/A",

                // Lấy lương từ WorkContract (Hình 1 của bạn)
                ContractSalary = contract.Salary,

                // Lấy giờ làm từ TimeSheet (Hình 2 của bạn)
                TotalHoursWorked = totalHours,
                ActualWorkDays = Math.Round(workDays, 1) // Làm tròn 1 số lẻ
            };

            PayrollList.Add(payrollItem);
        }

        // Cập nhật tổng quỹ lương
        TotalSalaryFund = PayrollList.Sum(x => x.NetSalary);
    }
}