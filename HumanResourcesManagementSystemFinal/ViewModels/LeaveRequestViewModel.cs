using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public class LeaveRequestItem
    {
        public LeaveRequest Request { get; set; }
        public bool CanApprove { get; set; }
        public bool CanEdit { get; set; }
    }

    public partial class LeaveRequestViewModel : ObservableObject
    {
        private readonly LeaveRequestService _leaveService;

        // Biến này sẽ lấy từ AppSession.CurrentRole
        private string _currentUserRole;

        [ObservableProperty]
        private int _currentUserId;

        [ObservableProperty]
        private bool _isManager;

        [ObservableProperty]
        private ObservableCollection<LeaveRequestItem> _requestItems = new();

        [ObservableProperty] private string _leaveType = "Nghỉ Phép Năm";
        [ObservableProperty] private DateTime _startDate = DateTime.Today;
        [ObservableProperty] private DateTime _endDate = DateTime.Today;
        [ObservableProperty] private string _reason;
        [ObservableProperty] private string _submitButtonContent = "Gửi Đơn";

        private int? _editingRequestId = null;

        // Constructor 1: Dùng khi truyền tham số
        public LeaveRequestViewModel(LeaveRequestService leaveService, int userId, string role)
        {
            _leaveService = leaveService;
            CurrentUserId = userId;
            _currentUserRole = role; // Lấy role được truyền vào

            // Logic check quyền
            IsManager = (_currentUserRole == "Admin" || _currentUserRole == "Manager");

            LoadDataCommand.Execute(null);
        }

        // Constructor 2: Dùng cho XAML
        public LeaveRequestViewModel()
        {
            var context = new DataContext();
            _leaveService = new LeaveRequestService(context);

            if (AppSession.CurrentUser != null)
            {
                // Lấy ID từ Employee
                CurrentUserId = AppSession.CurrentUser.Id;

                // --- SỬA Ở ĐÂY: Lấy Role từ biến riêng trong AppSession ---
                _currentUserRole = AppSession.CurrentRole;

                IsManager = (_currentUserRole == "Admin" || _currentUserRole == "Manager");
            }

            LoadData();
        }

        [RelayCommand]
        public async Task LoadData()
        {
            try
            {
                // Gọi Service
                var list = await _leaveService.GetRequestsByRoleAsync(CurrentUserId, _currentUserRole);

                var viewList = new List<LeaveRequestItem>();

                foreach (var req in list)
                {
                    // Logic hiển thị nút
                    bool canApprove = IsManager && (req.EmployeeId != CurrentUserId);
                    bool canEdit = (req.EmployeeId == CurrentUserId) && (req.Status == "Pending" || req.Status == "Đang chờ");

                    viewList.Add(new LeaveRequestItem
                    {
                        Request = req,
                        CanApprove = canApprove,
                        CanEdit = canEdit
                    });
                }

                RequestItems = new ObservableCollection<LeaveRequestItem>(viewList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        [RelayCommand]
        public async Task SubmitRequest()
        {
            if (StartDate > EndDate)
            {
                MessageBox.Show("Ngày kết thúc không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(Reason))
            {
                MessageBox.Show("Vui lòng nhập lý do.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool success = false;
                if (_editingRequestId == null)
                {
                    var newRequest = new LeaveRequest
                    {
                        EmployeeId = CurrentUserId,
                        LeaveType = LeaveType,
                        StartDate = StartDate,
                        EndDate = EndDate,
                        Reason = Reason,
                        Status = "Pending"
                    };
                    success = await _leaveService.AddRequestAsync(newRequest);
                    if (success) MessageBox.Show("Gửi đơn thành công!", "Thông báo");
                }
                else
                {
                    var updateRequest = new LeaveRequest
                    {
                        Id = _editingRequestId.Value,
                        LeaveType = LeaveType,
                        StartDate = StartDate,
                        EndDate = EndDate,
                        Reason = Reason
                    };
                    success = await _leaveService.UpdateRequestAsync(updateRequest);
                    if (success) MessageBox.Show("Cập nhật thành công!", "Thông báo");

                    _editingRequestId = null;
                    SubmitButtonContent = "Gửi Đơn";
                }

                if (success)
                {
                    Reason = string.Empty;
                    await LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task Approve(LeaveRequestItem item)
        {
            if (item == null) return;
            var request = item.Request;

            if (MessageBox.Show($"Duyệt đơn của {request.Employee?.FirstName}?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (await _leaveService.UpdateStatusAsync(request.Id, "Thông qua")) await LoadData();
            }
        }

        [RelayCommand]
        public async Task Reject(LeaveRequestItem item)
        {
            if (item == null) return;
            var request = item.Request;

            if (MessageBox.Show($"Từ chối đơn của {request.Employee?.FirstName}?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (await _leaveService.UpdateStatusAsync(request.Id, "Từ chối")) await LoadData();
            }
        }

        [RelayCommand]
        public void PrepareEdit(LeaveRequestItem item)
        {
            if (item == null) return;
            var req = item.Request;

            LeaveType = req.LeaveType;
            StartDate = req.StartDate;
            EndDate = req.EndDate;
            Reason = req.Reason;
            _editingRequestId = req.Id;
            SubmitButtonContent = "Cập Nhật";
        }

        [RelayCommand]
        public async Task DeleteRequest(LeaveRequestItem item)
        {
            if (item == null) return;
            var req = item.Request;

            if (MessageBox.Show("Xóa đơn này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (await _leaveService.DeleteRequestAsync(req.Id))
                {
                    if (_editingRequestId == req.Id)
                    {
                        _editingRequestId = null;
                        Reason = "";
                        SubmitButtonContent = "Gửi Đơn";
                    }
                    await LoadData();
                }
            }
        }
    }
}