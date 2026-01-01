using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Messages;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class HomeViewModel : ObservableObject, IRecipient<ReloadRequestMessage>
{
    [ObservableProperty] private int _totalEmployees;
    [ObservableProperty] private int _totalDepartments;
    [ObservableProperty] private int _activeContracts;
    [ObservableProperty] private string _attendanceStatus = "0 / 0";
    [ObservableProperty] private string _greetingMessage = "Xin chào";

    private DispatcherTimer _timer;

    public ObservableCollection<Employee> RecentEmployees { get; set; } = new();
    public ObservableCollection<Employee> ExpiringContractEmployees { get; set; } = new();
    public ObservableCollection<DepartmentStat> DepartmentStats { get; set; } = new();
    public ObservableCollection<LeaveRequest> PendingLeavesList { get; set; } = new();

    public HomeViewModel()
    {
        if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;

        WeakReferenceMessenger.Default.Register<ReloadRequestMessage>(this);

        _ = LoadDashboardDataAsync();

        UpdateGreeting();
        StartClock();
    }

    public void Receive(ReloadRequestMessage message)
    {
        Application.Current.Dispatcher.Invoke(() => _ = LoadDashboardDataAsync());
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
    private async Task LoadDashboardDataAsync()
    {
        try
        {
            using var context = new DataContext();

            // 1. Tổng nhân viên (Sửa IsActive -> Status)
            var totalEmp = await context.Employees.CountAsync(e => e.Status == "Active");
            var totalDept = await context.Departments.CountAsync();
            var activeCont = await context.WorkContracts.CountAsync(c => c.EndDate > DateTime.Now);

            // 2. Chấm công hôm nay (Sửa Date -> WorkDate, CheckInTime -> TimeIn)
            var today = DateTime.Today;
            var checkedInCount = await context.TimeSheets
                .AsNoTracking()
                .Where(t => t.WorkDate.Date == today && t.TimeIn != null)
                .CountAsync();

            TotalEmployees = totalEmp;
            TotalDepartments = totalDept;
            ActiveContracts = activeCont;
            AttendanceStatus = $"{checkedInCount} / {TotalEmployees}";

            // 3. Nhân viên mới (Giữ nguyên logic HireDate)
            var newHires = await context.Employees
                .AsNoTracking()
                .OrderByDescending(e => e.EmployeeID) // Lưu ý: Model mới bạn có thể đã bỏ HireDate hoặc đổi tên, nếu lỗi dòng này hãy kiểm tra lại Model Employee xem có trường HireDate không. Nếu không có thể dùng EmployeeID để sort.
                .Take(5)
                .ToListAsync();

            RecentEmployees.Clear();
            foreach (var item in newHires) RecentEmployees.Add(item);

            // 4. Hợp đồng sắp hết hạn
            var thirtyDaysLater = DateTime.Now.AddDays(30);
            var expiringList = await context.WorkContracts
                .AsNoTracking()
                .Include(c => c.Employee)
                .Where(c => c.EndDate > DateTime.Now && c.EndDate <= thirtyDaysLater)
                .Select(c => c.Employee)
                .ToListAsync();

            ExpiringContractEmployees.Clear();
            foreach (var item in expiringList) ExpiringContractEmployees.Add(item);

            // 5. Thống kê theo phòng ban
            var deptStats = await context.Employees
                .AsNoTracking()
                .Where(e => e.Department != null) // Đảm bảo không lỗi null
                .GroupBy(e => e.Department.DepartmentName)
                .Select(g => new DepartmentStat { DepartmentName = g.Key, Count = g.Count() })
                .ToListAsync();

            DepartmentStats.Clear();
            foreach (var item in deptStats) DepartmentStats.Add(item);

            // 6. Đơn nghỉ phép chờ duyệt (Sửa Employee -> Requester)
            var pendings = await context.LeaveRequests
                .AsNoTracking()
                .Include(l => l.Requester) // Model mới quan hệ là Requester
                .Where(l => l.Status == "Đang chờ" || l.Status == "Pending")
                .OrderByDescending(l => l.StartDate)
                .ToListAsync();

            PendingLeavesList.Clear();
            foreach (var item in pendings) PendingLeavesList.Add(item);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tải dữ liệu Dashboard:\n" + GetDeepErrorMessage(ex), "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ApproveLeaveAsync(LeaveRequest request) => await UpdateLeaveStatusAsync(request, "Thông qua");

    [RelayCommand]
    private async Task RejectLeaveAsync(LeaveRequest request) => await UpdateLeaveStatusAsync(request, "Từ chối");

    private async Task UpdateLeaveStatusAsync(LeaveRequest request, string newStatus)
    {
        if (request == null) return;
        try
        {
            using var context = new DataContext();
            // Sửa Id -> RequestID
            var item = await context.LeaveRequests.FindAsync(request.RequestID);
            if (item != null)
            {
                item.Status = newStatus;

                // Cập nhật người duyệt (Nếu có thông tin UserSession)
                // string currentAdminID = UserSession.CurrentUserID;
                // item.ApproverID = currentAdminID;

                await context.SaveChangesAsync();

                PendingLeavesList.Remove(request);
                MessageBox.Show($"Đã {newStatus} đơn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                WeakReferenceMessenger.Default.Send(new ReloadRequestMessage("Reload"));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể cập nhật trạng thái:\n" + GetDeepErrorMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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

public class DepartmentStat
{
    public string DepartmentName { get; set; }
    public int Count { get; set; }
}