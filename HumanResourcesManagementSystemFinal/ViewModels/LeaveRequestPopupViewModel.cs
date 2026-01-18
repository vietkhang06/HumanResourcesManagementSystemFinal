using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class LeaveRequestPopupViewModel : ObservableObject
    {
        private readonly string _employeeId;
        public Action? CloseAction { get; set; } // Allows VM to close the Window

        [ObservableProperty] private DateTime _startDate = DateTime.Today;
        [ObservableProperty] private DateTime _endDate = DateTime.Today;
        [ObservableProperty] private string _reason = string.Empty;
        [ObservableProperty] private string _selectedLeaveType;

        public ObservableCollection<string> LeaveTypes { get; } = new()
        {
            "Nghỉ Phép Năm", "Nghỉ Đau Ốm", "Nghỉ Không Lương", "Nghỉ Thai Sản"
        };

        public LeaveRequestPopupViewModel(string employeeId)
        {
            _employeeId = employeeId;
            SelectedLeaveType = LeaveTypes[0];
        }

        [RelayCommand]
        private async Task SubmitAsync()
        {
            if (string.IsNullOrWhiteSpace(Reason))
            {
                MessageBox.Show("Vui lòng nhập lý do.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (EndDate < StartDate)
            {
                MessageBox.Show("Ngày kết thúc không hợp lệ.", "Lỗi ngày", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new DataContext();
                var newRequest = new LeaveRequest
                {
                    RequestID = "LR" + DateTime.Now.Ticks.ToString().Substring(12),
                    EmployeeID = _employeeId,
                    LeaveType = SelectedLeaveType,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    Reason = Reason,
                    Status = "Pending"
                };

                context.LeaveRequests.Add(newRequest);
                await context.SaveChangesAsync();

                MessageBox.Show("Gửi đơn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke(); // Close the window
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel() => CloseAction?.Invoke();
    }
}