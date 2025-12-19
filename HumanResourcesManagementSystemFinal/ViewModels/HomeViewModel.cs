using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Messages; // Đảm bảo đã có file này
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class HomeViewModel : ObservableObject, IRecipient<ReloadRequestMessage>
{
    // Các biến hiển thị lên giao diện
    [ObservableProperty] private int _totalEmployees;
    [ObservableProperty] private int _totalDepartments;
    [ObservableProperty] private int _activeContracts;
    [ObservableProperty] private string _attendanceStatus = "0 / 0"; // Mặc định tránh rỗng
    [ObservableProperty] private string _greetingMessage = "Xin chào";

    private DispatcherTimer _timer;

    // Các danh sách dữ liệu
    public ObservableCollection<Employee> RecentEmployees { get; set; } = new();
    public ObservableCollection<Employee> ExpiringContractEmployees { get; set; } = new();
    public ObservableCollection<DepartmentStat> DepartmentStats { get; set; } = new();
    public ObservableCollection<LeaveRequest> PendingLeavesList { get; set; } = new();

    public HomeViewModel()
    {
        // Kiểm tra Design Mode để tránh lỗi khi mở giao diện trong Visual Studio
        if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;

        // 1. Đăng ký nhận tin nhắn Reload
        try { WeakReferenceMessenger.Default.Register<ReloadRequestMessage>(this); } catch { }

        // 2. Tải dữ liệu NGAY LẬP TỨC
        LoadDashboardData();

        // 3. Chạy đồng hồ chào hỏi
        UpdateGreeting();
        StartClock();
    }

    // Hàm nhận tin nhắn từ các màn hình khác
    public void Receive(ReloadRequestMessage message)
    {
        Application.Current.Dispatcher.Invoke(LoadDashboardData);
    }

    [RelayCommand]
    private void LoadDashboardData()
    {
        try
        {
            using var context = new DataContext();

            // Tải số liệu thống kê
            TotalEmployees = context.Employees.Count(e => e.IsActive);
            TotalDepartments = context.Departments.Count();
            ActiveContracts = context.WorkContracts.Count(c => c.EndDate > DateTime.Now);

            var today = DateTime.Today;
            int checkedInCount = context.TimeSheets.Where(t => t.Date.Date == today && t.CheckInTime != null).Count();
            AttendanceStatus = $"{checkedInCount} / {TotalEmployees}";

            // Tải nhân viên mới (5 người)
            var newHires = context.Employees.OrderByDescending(e => e.HireDate).Take(5).ToList();
            RecentEmployees.Clear();
            foreach (var item in newHires) RecentEmployees.Add(item);

            // Tải hợp đồng sắp hết hạn (30 ngày)
            var thirtyDaysLater = DateTime.Now.AddDays(30);
            var expiringList = context.WorkContracts.Include(c => c.Employee)
                .Where(c => c.EndDate > DateTime.Now && c.EndDate <= thirtyDaysLater)
                .Select(c => c.Employee).ToList();
            ExpiringContractEmployees.Clear();
            foreach (var item in expiringList) ExpiringContractEmployees.Add(item);

            // Tải thống kê phòng ban
            var deptStats = context.Employees.GroupBy(e => e.Department.DepartmentName)
                .Select(g => new DepartmentStat { DepartmentName = g.Key, Count = g.Count() }).ToList();
            DepartmentStats.Clear();
            foreach (var item in deptStats) DepartmentStats.Add(item);

            // Tải đơn nghỉ phép (Quan trọng: Phải đúng trạng thái string trong DB)
            var pendings = context.LeaveRequests.Include(l => l.Employee)
                .Where(l => l.Status == "Đang chờ" || l.Status == "Pending") // Check cả tiếng Anh/Việt
                .OrderByDescending(l => l.StartDate).ToList();

            PendingLeavesList.Clear();
            foreach (var item in pendings) PendingLeavesList.Add(item);
        }
        catch (Exception ex)
        {
            // Bỏ qua lỗi kết nối tạm thời
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }

    [RelayCommand]
    private void ApproveLeave(LeaveRequest request) => UpdateLeaveStatus(request, "Thông qua");

    [RelayCommand]
    private void RejectLeave(LeaveRequest request) => UpdateLeaveStatus(request, "Từ chối");

    private void UpdateLeaveStatus(LeaveRequest request, string newStatus)
    {
        if (request == null) return;
        try
        {
            using var context = new DataContext();
            var item = context.LeaveRequests.Find(request.Id);
            if (item != null)
            {
                item.Status = newStatus;
                context.SaveChanges();
                PendingLeavesList.Remove(request);
                MessageBox.Show($"Đã {newStatus} đơn thành công!", "Thông báo");

                // Gửi tin nhắn reload (tự sướng)
                WeakReferenceMessenger.Default.Send(new ReloadRequestMessage("Reload"));
            }
        }
        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
    }

    private void StartClock()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        _timer.Tick += (s, e) => UpdateGreeting();
        _timer.Start();
    }

    private void UpdateGreeting()
    {
        var hour = DateTime.Now.Hour;
        if (hour >= 5 && hour < 11) GreetingMessage = "Chào buổi sáng";
        else if (hour >= 11 && hour < 13) GreetingMessage = "Chào buổi trưa";
        else if (hour >= 13 && hour < 18) GreetingMessage = "Chào buổi chiều";
        else GreetingMessage = "Chào buổi tối";
    }
}

public class DepartmentStat { public string DepartmentName { get; set; } public int Count { get; set; } }