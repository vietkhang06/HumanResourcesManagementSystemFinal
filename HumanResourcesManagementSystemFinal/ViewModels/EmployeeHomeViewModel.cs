using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class EmployeeHomeViewModel : ObservableObject
    {
        [ObservableProperty] private string _welcomeMessage;
        [ObservableProperty] private int _daysWorkedThisMonth;
        [ObservableProperty] private double _remainingLeaveDays = 12;
        [ObservableProperty] private string _todayCheckInTime = "--:--";
        [ObservableProperty] private string _nextHoliday = "25/12";
        public ObservableCollection<LeaveRequest> MyLeaveRequests { get; set; } = new();
        private int _currentEmployeeId;

        public EmployeeHomeViewModel(int employeeId)
        {
            _currentEmployeeId = employeeId;
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
            LoadEmployeeData();
        }

        // Constructor mặc định cho XAML Designer
        public EmployeeHomeViewModel() { }

        [RelayCommand]
        private void LoadEmployeeData()
        {
            using var context = new DataContext();

            // 1. Lấy thông tin nhân viên
            var emp = context.Employees.Find(_currentEmployeeId);
            if (emp != null)
            {
                WelcomeMessage = $"Xin chào, {emp.LastName} {emp.FirstName}!";
            }

            // 2. Đếm số ngày công tháng này
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DaysWorkedThisMonth = context.TimeSheets
                .Count(t => t.EmployeeId == _currentEmployeeId && t.Date >= startOfMonth);


            // 3. Kiểm tra giờ Check-in hôm nay
            var todaySheet = context.TimeSheets
                .FirstOrDefault(t => t.EmployeeId == _currentEmployeeId && t.Date.Date == DateTime.Today);

            if (todaySheet != null)
            {
                TodayCheckInTime = todaySheet.CheckInTime.HasValue
                    ? todaySheet.CheckInTime.Value.ToString(@"hh\:mm")
                    : "--:--";
            }

            // 4. Lấy danh sách đơn xin nghỉ của tôi
            var myRequests = context.LeaveRequests
                .Where(l => l.EmployeeId == _currentEmployeeId)
                .OrderByDescending(l => l.StartDate)
                .Take(5)
                .ToList();

            MyLeaveRequests.Clear();
            foreach (var req in myRequests) MyLeaveRequests.Add(req);
        }
    }
}