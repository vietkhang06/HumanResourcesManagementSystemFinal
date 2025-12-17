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

        // Biến để lưu ID của đơn đang được sửa (null = đang tạo mới)
        private int? _editingRequestId = null;

        [ObservableProperty]
        private string _submitButtonContent = "Gửi Đơn"; // Để thay đổi chữ nút bấm

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
            // 1. Validate dữ liệu cơ bản
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

            try
            {
                // 2. Kiểm tra xem đang ở chế độ nào
                if (_editingRequestId == null)
                {
                    // === CHẾ ĐỘ TẠO MỚI (ADD) ===
                    var newRequest = new LeaveRequest
                    {
                        EmployeeId = _currentUserId,
                        LeaveType = LeaveType,
                        StartDate = StartDate,
                        EndDate = EndDate,
                        Reason = Reason,
                        Status = "Pending" // Mặc định là chờ duyệt
                    };

                    bool success = await _leaveService.AddRequestAsync(newRequest);
                    if (success) MessageBox.Show("Gửi đơn thành công!", "Thông báo");
                }
                else
                {
                    // === CHẾ ĐỘ CHỈNH SỬA (UPDATE) ===
                    // Tạo object chứa thông tin cần update
                    var updateRequest = new LeaveRequest
                    {
                        Id = _editingRequestId.Value, // Lấy ID đang sửa
                        LeaveType = LeaveType,
                        StartDate = StartDate,
                        EndDate = EndDate,
                        Reason = Reason
                        // Lưu ý: Không update Status ở đây để tránh gian lận
                    };

                    bool success = await _leaveService.UpdateRequestAsync(updateRequest);
                    if (success) MessageBox.Show("Cập nhật đơn thành công!", "Thông báo");

                    // 3. Reset trạng thái về "Tạo mới" sau khi sửa xong
                    _editingRequestId = null;
                    SubmitButtonContent = "Gửi Đơn";
                }

                // 4. Dọn dẹp form và tải lại danh sách
                Reason = string.Empty;
                await LoadData();
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

        [RelayCommand]
        public void PrepareEdit(LeaveRequest request)
        {
            if (request == null) return;

            // Chỉ cho phép sửa khi trạng thái là "Pending" (hoặc "Đang chờ")
            if (request.Status != "Pending" && request.Status != "Đang chờ")
            {
                MessageBox.Show("Chỉ có thể sửa các đơn đang chờ duyệt!", "Không thể sửa", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Đưa dữ liệu từ dòng được chọn lên form nhập liệu
            LeaveType = request.LeaveType;
            StartDate = request.StartDate;
            EndDate = request.EndDate;
            Reason = request.Reason;

            // Lưu lại ID để biết đang sửa đơn nào
            _editingRequestId = request.Id;

            // Đổi giao diện nút bấm để người dùng biết đang ở chế độ Sửa
            SubmitButtonContent = "Cập Nhật";
        }

        [RelayCommand]
        public async Task DeleteRequest(LeaveRequest request)
        {
            if (request == null) return;

            // Chỉ cho phép xóa khi trạng thái là Pending
            if (request.Status != "Pending" && request.Status != "Đang chờ")
            {
                MessageBox.Show("Chỉ có thể xóa các đơn chưa được xử lý!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc muốn xóa đơn nghỉ phép này không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = await _leaveService.DeleteRequestAsync(request.Id);
                    if (success)
                    {
                        MessageBox.Show("Đã xóa đơn thành công.");
                        await LoadData(); // Tải lại danh sách
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa: " + ex.Message);
                }
            }
        }

    }
}