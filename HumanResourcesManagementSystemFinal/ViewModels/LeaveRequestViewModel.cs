using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
        private string _currentUserRole;
        private string? _editingRequestId;

        [ObservableProperty] private string _currentUserId;
        [ObservableProperty] private bool _isManager;
        [ObservableProperty] private ObservableCollection<LeaveRequestItem> _requestItems = new();
        [ObservableProperty] private string _leaveType = "Nghỉ Phép Năm";
        [ObservableProperty] private DateTime _startDate = DateTime.Today;
        [ObservableProperty] private DateTime _endDate = DateTime.Today;
        [ObservableProperty] private string _reason;
        [ObservableProperty] private string _submitButtonContent = "Gửi Đơn";

        public LeaveRequestViewModel(LeaveRequestService leaveService, string userId, string role)
        {
            _leaveService = leaveService;
            CurrentUserId = userId;
            _currentUserRole = role;
            IsManager = (_currentUserRole == "Admin" || _currentUserRole == "Manager");
            _ = LoadDataAsync();
        }

        public LeaveRequestViewModel()
        {
            var context = new DataContext();
            _leaveService = new LeaveRequestService(context);

            if (AppSession.CurrentUser != null)
            {
                CurrentUserId = AppSession.CurrentUser.EmployeeID;
                _currentUserRole = AppSession.CurrentRole;
                IsManager = (_currentUserRole == "Admin" || _currentUserRole == "Manager");
            }
            else
            {
                CurrentUserId = "";
            }

            _ = LoadDataAsync();
        }

        private string GenerateRequestID()
        {
            using var context = new DataContext();

            string lastID = context.LeaveRequests
                .OrderByDescending(r => r.RequestID)
                .Select(r => r.RequestID)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(lastID)) return "LR001";

            string numPart = lastID.Substring(2);
            if (int.TryParse(numPart, out int num))
                return "LR" + (num + 1).ToString("D3");

            return "LR" + new Random().Next(100, 999);
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
        public async Task LoadDataAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentUserId))
                {
                    MessageBox.Show("Mã nhân viên không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var list = await _leaveService.GetRequestsByRoleAsync(CurrentUserId, _currentUserRole);
                var viewList = new List<LeaveRequestItem>();

                foreach (var req in list)
                {
                    bool canApprove = IsManager && (req.EmployeeID != CurrentUserId);
                    bool canEdit = (req.EmployeeID == CurrentUserId) &&
                                   (req.Status == "Pending" || req.Status == "Đang chờ");

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
                MessageBox.Show(
                    "Lỗi tải dữ liệu:\n" + GetDeepErrorMessage(ex),
                    "Lỗi hệ thống",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task SubmitRequestAsync()
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
                bool success;

                if (_editingRequestId == null)
                {
                    string newReqID = GenerateRequestID();

                    var newRequest = new LeaveRequest
                    {
                        RequestID = newReqID,
                        EmployeeID = CurrentUserId,
                        LeaveType = LeaveType,
                        StartDate = StartDate,
                        EndDate = EndDate,
                        Reason = Reason,
                        Status = "Pending"
                    };

                    success = await _leaveService.AddRequestAsync(newRequest);

                    if (success)
                        MessageBox.Show("Gửi đơn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var updateRequest = new LeaveRequest
                    {
                        RequestID = _editingRequestId,
                        LeaveType = LeaveType,
                        StartDate = StartDate,
                        EndDate = EndDate,
                        Reason = Reason
                    };

                    success = await _leaveService.UpdateRequestAsync(updateRequest);

                    if (success)
                    {
                        MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        _editingRequestId = null;
                        SubmitButtonContent = "Gửi Đơn";
                    }
                }

                if (success)
                {
                    Reason = string.Empty;
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi xử lý đơn:\n" + GetDeepErrorMessage(ex),
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task ApproveAsync(LeaveRequestItem item)
        {
            if (item == null) return;

            var request = item.Request;
            string empName = request.Requester?.FullName ?? "Nhân viên";

            if (MessageBox.Show($"Duyệt đơn của {empName}?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    if (!string.IsNullOrEmpty(request.RequestID))
                    {
                        if (await _leaveService.UpdateStatusAsync(request.RequestID, "Thông qua"))
                            await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show("Mã đơn không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Lỗi duyệt đơn:\n" + GetDeepErrorMessage(ex),
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        public async Task RejectAsync(LeaveRequestItem item)
        {
            if (item == null) return;

            var request = item.Request;
            string empName = request.Requester?.FullName ?? "Nhân viên";

            if (MessageBox.Show($"Từ chối đơn của {empName}?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    if (!string.IsNullOrEmpty(request.RequestID))
                    {
                        if (await _leaveService.UpdateStatusAsync(request.RequestID, "Từ chối"))
                            await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show("Mã đơn không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Lỗi từ chối đơn:\n" + GetDeepErrorMessage(ex),
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        public void PrepareEdit(LeaveRequestItem item)
        {
            if (item == null) return;

            var req = item.Request;

            LeaveType = req.LeaveType;
            StartDate = req.StartDate ?? DateTime.Now;
            EndDate = req.EndDate ?? DateTime.Now;
            Reason = req.Reason;
            _editingRequestId = req.RequestID;
            SubmitButtonContent = "Cập Nhật";
        }

        [RelayCommand]
        public async Task DeleteRequestAsync(LeaveRequestItem item)
        {
            if (item == null) return;

            var req = item.Request;

            if (MessageBox.Show(
                "Bạn có chắc chắn muốn xóa đơn này?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    if (!string.IsNullOrEmpty(req.RequestID))
                    {
                        if (await _leaveService.DeleteRequestAsync(req.RequestID))
                        {
                            if (_editingRequestId == req.RequestID)
                            {
                                _editingRequestId = null;
                                Reason = string.Empty;
                                SubmitButtonContent = "Gửi Đơn";
                            }

                            await LoadDataAsync();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Mã đơn không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Lỗi xóa đơn:\n" + GetDeepErrorMessage(ex),
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}
