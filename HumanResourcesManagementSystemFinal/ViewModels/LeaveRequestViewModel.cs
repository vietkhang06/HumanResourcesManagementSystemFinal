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

        // --- Danh sách hiển thị lên DataGrid ---
        [ObservableProperty]
        private ObservableCollection<LeaveRequest> _leaveRequests;

        // --- Các trường nhập liệu (Binding với View) ---
        [ObservableProperty]
        private string _leaveType = "Annual"; // Mặc định

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today;

        [ObservableProperty]
        private DateTime _endDate = DateTime.Today;

        [ObservableProperty]
        private string _reason;

        // --- Kiểm tra quyền để ẩn/hiện nút Duyệt ---
        public bool IsManager => _currentUserRole == "Admin" || _currentUserRole == "Manager";

        public LeaveRequestViewModel(LeaveRequestService service, int userId, string role)
        {
            _leaveService = service;
            _currentUserId = userId;
            _currentUserRole = role;
            _leaveRequests = new ObservableCollection<LeaveRequest>();

            // Load dữ liệu ngay khi mở
            LoadDataCommand.Execute(null);
        }

        [RelayCommand]
        public async Task LoadData()
        {
            var list = await _leaveService.GetRequestsByRoleAsync(_currentUserId, _currentUserRole);
            LeaveRequests = new ObservableCollection<LeaveRequest>(list);
        }

        [RelayCommand]
        public async Task SubmitRequest()
        {
            // Validate dữ liệu
            if (StartDate > EndDate)
            {
                MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc!");
                return;
            }
            if (string.IsNullOrWhiteSpace(Reason))
            {
                MessageBox.Show("Vui lòng nhập lý do!");
                return;
            }

            var newRequest = new LeaveRequest
            {
                EmployeeId = _currentUserId,
                LeaveType = LeaveType,
                StartDate = StartDate,
                EndDate = EndDate,
                Reason = Reason,
                Status = "Pending"
            };

            bool success = await _leaveService.AddRequestAsync(newRequest);
            if (success)
            {
                MessageBox.Show("Gửi đơn thành công!");
                await LoadData(); // Refresh lại lưới
                Reason = string.Empty; // Reset form
            }
            else
            {
                MessageBox.Show("Lỗi khi gửi đơn.");
            }
        }

        // Logic Duyệt đơn (Chỉ Manager/Admin bấm được)
        [RelayCommand]
        public async Task Approve(LeaveRequest request)
        {
            if (request == null) return;
            await _leaveService.UpdateStatusAsync(request.Id, "Approved");
            await LoadData();
        }

        [RelayCommand]
        public async Task Reject(LeaveRequest request)
        {
            if (request == null) return;
            await _leaveService.UpdateStatusAsync(request.Id, "Rejected");
            await LoadData();
        }
    }
}