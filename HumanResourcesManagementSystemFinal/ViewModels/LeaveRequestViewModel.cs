using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels;

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

    [ObservableProperty] private int _currentUserId;
    [ObservableProperty] private bool _isManager;
    [ObservableProperty] private ObservableCollection<LeaveRequestItem> _requestItems = new();
    [ObservableProperty] private string _leaveType = "Nghỉ Phép Năm";
    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;
    [ObservableProperty] private string _reason;
    [ObservableProperty] private string _submitButtonContent = "Gửi Đơn";

    private int? _editingRequestId = null;

    public LeaveRequestViewModel(LeaveRequestService leaveService, int userId, string role)
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
            CurrentUserId = AppSession.CurrentUser.Id;
            _currentUserRole = AppSession.CurrentRole;
            IsManager = (_currentUserRole == "Admin" || _currentUserRole == "Manager");
        }

        _ = LoadDataAsync();
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
            var list = await _leaveService.GetRequestsByRoleAsync(CurrentUserId, _currentUserRole);
            var viewList = new List<LeaveRequestItem>();

            foreach (var req in list)
            {
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
            MessageBox.Show("Lỗi tải dữ liệu:\n" + GetDeepErrorMessage(ex), "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (success) MessageBox.Show("Gửi đơn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
            MessageBox.Show($"Lỗi xử lý đơn:\n{GetDeepErrorMessage(ex)}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task ApproveAsync(LeaveRequestItem item)
    {
        if (item == null) return;
        var request = item.Request;

        if (MessageBox.Show($"Duyệt đơn của {request.Employee?.FirstName}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            try
            {
                if (await _leaveService.UpdateStatusAsync(request.Id, "Thông qua")) await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi duyệt đơn:\n" + GetDeepErrorMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    public async Task RejectAsync(LeaveRequestItem item)
    {
        if (item == null) return;
        var request = item.Request;

        if (MessageBox.Show($"Từ chối đơn của {request.Employee?.FirstName}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            try
            {
                if (await _leaveService.UpdateStatusAsync(request.Id, "Từ chối")) await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi từ chối đơn:\n" + GetDeepErrorMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
    public async Task DeleteRequestAsync(LeaveRequestItem item)
    {
        if (item == null) return;
        var req = item.Request;

        if (MessageBox.Show("Bạn có chắc chắn muốn xóa đơn này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            try
            {
                if (await _leaveService.DeleteRequestAsync(req.Id))
                {
                    if (_editingRequestId == req.Id)
                    {
                        _editingRequestId = null;
                        Reason = "";
                        SubmitButtonContent = "Gửi Đơn";
                    }
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa đơn:\n" + GetDeepErrorMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}