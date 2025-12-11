using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows; 

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty] private int _totalEmployees;
    [ObservableProperty] private int _totalDepartments;
    [ObservableProperty] private int _activeContracts;
    [ObservableProperty] private string _attendanceStatus;
    public ObservableCollection<Employee> RecentEmployees { get; set; } = new();
    public ObservableCollection<Employee> ExpiringContractEmployees { get; set; } = new();
    public ObservableCollection<DepartmentStat> DepartmentStats { get; set; } = new();
    public ObservableCollection<LeaveRequest> PendingLeavesList { get; set; } = new();

    public HomeViewModel()
    {
        if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
        LoadDashboardData();
    }

    [RelayCommand]
    private void LoadDashboardData()
    {
        using var context = new DataContext();
        _totalEmployees = context.Employees.Count(e => e.IsActive);
        _totalDepartments = context.Departments.Count();
        _activeContracts = context.WorkContracts.Count(c => c.EndDate > DateTime.Now);
        var today = DateTime.Today;
        var newHires = context.Employees.OrderByDescending(e => e.HireDate) .Take(5).ToList();
        int checkedInCount = context.TimeSheets.Where(t => t.Date.Date == today && t.CheckInTime != null).Count();
        RecentEmployees.Clear();
        foreach (var item in newHires) RecentEmployees.Add(item);
        var thirtyDaysLater = DateTime.Now.AddDays(30);
        var expiringList = context.WorkContracts.Include(c => c.Employee).Where(c => c.EndDate > DateTime.Now && c.EndDate <= thirtyDaysLater).Select(c => c.Employee) .ToList();
        ExpiringContractEmployees.Clear();
        foreach (var item in expiringList) ExpiringContractEmployees.Add(item);
        var deptStats = context.Employees.GroupBy(e => e.Department.DepartmentName).Select(g => new DepartmentStat{DepartmentName = g.Key, Count = g.Count()}).ToList();
        DepartmentStats.Clear();
        foreach (var item in deptStats) DepartmentStats.Add(item);
        var pendings = context.LeaveRequests.Include(l => l.Employee) .Where(l => l.Status == "Đang chờ").OrderByDescending(l => l.StartDate).ToList();
        PendingLeavesList.Clear();
        foreach (var item in pendings) PendingLeavesList.Add(item);
        _attendanceStatus = $"{checkedInCount} / {_totalEmployees}";
    }
    [RelayCommand]
    private void ApproveLeave(LeaveRequest request)
    {
        UpdateLeaveStatus(request, "Thông qua");
    }

    [RelayCommand]
    private void RejectLeave(LeaveRequest request)
    {
        UpdateLeaveStatus(request, "Từ chối");
    }

    private void UpdateLeaveStatus(LeaveRequest request, string newStatus)
    {
        if (request == null) return;

        using var context = new DataContext();
        var item = context.LeaveRequests.Find(request.Id);
        if (item != null)
        {
            item.Status = newStatus;
            context.SaveChanges();

            // Xóa khỏi danh sách hiển thị (vì đã xử lý xong)
            PendingLeavesList.Remove(request);

            MessageBox.Show($"Đã {newStatus} đơn thành công!", "Thông báo");
        }
    }
}
public class DepartmentStat
{
    public string DepartmentName { get; set; }
    public int Count { get; set; }
}