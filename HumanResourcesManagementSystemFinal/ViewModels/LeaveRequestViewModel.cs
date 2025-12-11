using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class LeaveRequestViewModel : ObservableObject
    {
        private readonly LeaveRequestService _leaveService;
        private readonly int _currentUserId;
        private readonly string _currentUserRole;

        [ObservableProperty]
        private ObservableCollection<LeaveRequest> _leaveRequests;

        [ObservableProperty]
        private string _leaveType = "Annual";

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today;

        [ObservableProperty]
        private DateTime _endDate = DateTime.Today;

        [ObservableProperty]
        private string _reason;

        [ObservableProperty]
        private bool _isManager;

        public LeaveRequestViewModel(LeaveRequestService leaveService, int userId, string role)
        {
            _leaveService = leaveService;
            _currentUserId = userId;
            _currentUserRole = role;

            IsManager = (role == "Admin" || role == "Manager");

            _leaveRequests = new ObservableCollection<LeaveRequest>();
            LoadDataCommand.Execute(null);
        }

        [RelayCommand]
        public async Task LoadData()
        {
            try
            {
                var list = await _leaveService.GetRequestsByRoleAsync(_currentUserId, _currentUserRole);
                LeaveRequests = new ObservableCollection<LeaveRequest>(list);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        [RelayCommand]
        public async Task SubmitRequest()
        {
            if (StartDate > EndDate)
            {
                MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc!", "Lỗi ngày tháng", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Reason))
            {
                MessageBox.Show("Vui lòng nhập lý do nghỉ phép.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newRequest = new LeaveRequest
            {
                EmployeeId = _currentUserId,
                LeaveType = LeaveType,
                StartDate = StartDate,
                EndDate = EndDate,
                Reason = Reason,
                Status = "Đang chờ"
            };

            try
            {
                bool success = await _leaveService.AddRequestAsync(newRequest);

                if (success)
                {
                    MessageBox.Show("Gửi đơn thành công!", "Thông báo");
                    Reason = string.Empty;
                    await LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chi tiết: {ex.Message}", "Báo Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task Approve(LeaveRequest request)
        {
            if (request == null) return;

            var result = MessageBox.Show($"Bạn muốn DUYỆT đơn của {request.Employee?.FirstName}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                bool success = await _leaveService.UpdateStatusAsync(request.Id, "Thông qua");
                if (success) await LoadData();
            }
        }

        [RelayCommand]
        public async Task Reject(LeaveRequest request)
        {
            if (request == null) return;

            var result = MessageBox.Show($"Bạn muốn TỪ CHỐI đơn của {request.Employee?.FirstName}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                bool success = await _leaveService.UpdateStatusAsync(request.Id, "Từ chối");
                if (success) await LoadData();
            }
        }
    }
}