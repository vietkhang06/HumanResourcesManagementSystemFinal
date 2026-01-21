using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public class LeaveRequestItem
    {
        public LeaveRequest Request { get; set; }
        public bool CanApprove { get; set; }
        public bool CanEdit { get; set; }
    }

    public partial class LeaveRequestViewModel : RealTimeViewModel
    {
        private readonly LeaveRequestService _leaveService;
        private string _currentUserRole;
        private string? _editingRequestId;

        private string _originalLeaveType;
        private DateTime _originalStartDate;
        private DateTime _originalEndDate;
        private string _originalReason;

        [ObservableProperty] private string _currentUserId;
        [ObservableProperty] private bool _isManager;
        [ObservableProperty] private ObservableCollection<LeaveRequestItem> _requestItems = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        private string _leaveType = "Nghỉ Phép Năm";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        private DateTime _startDate = DateTime.Today;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        private DateTime _endDate = DateTime.Today;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        private string _reason;

        [ObservableProperty] private string _submitButtonContent = "Gửi Đơn";

        public bool CanSubmit
        {
            get
            {
                if (string.IsNullOrEmpty(_editingRequestId))
                {
                    return true;
                }

                bool isChanged = LeaveType != _originalLeaveType ||
                                 StartDate != _originalStartDate ||
                                 EndDate != _originalEndDate ||
                                 Reason != _originalReason;

                return isChanged;
            }
        }

        public LeaveRequestViewModel(LeaveRequestService leaveService, string userId, string role) : base()
        {
            _leaveService = leaveService;
            CurrentUserId = userId;
            _currentUserRole = role;
            IsManager = (_currentUserRole == "Admin" || _currentUserRole == "Manager");
            _ = LoadDataAsync();
        }

        public LeaveRequestViewModel() : base()
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
            string lastID = context.LeaveRequests.OrderByDescending(r => r.RequestID).Select(r => r.RequestID).FirstOrDefault();
            if (string.IsNullOrEmpty(lastID)) return "LR001";
            string numPart = lastID.Substring(2);
            if (int.TryParse(numPart, out int num)) return "LR" + (num + 1).ToString("D3");
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
                list = list.OrderBy(r => (r.Status == "Pending" || r.Status == "Đang chờ") ? 0 : 1)
                           .ThenByDescending(r => r.StartDate)
                           .ToList();

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
                        OnPropertyChanged(nameof(CanSubmit));
                    }
                }

                if (success)
                {
                    Reason = string.Empty;
                    StartDate = DateTime.Today;
                    EndDate = DateTime.Today;
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
            string message = (request.Status == "Rejected" || request.Status == "Từ chối")
                ? $"Thay đổi quyết định: CHẤP THUẬN đơn của {empName}?"
                : $"Duyệt đơn của {empName}?";

            if (MessageBox.Show(message, "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    if (string.IsNullOrEmpty(request.RequestID)) return;
                    if (await _leaveService.UpdateStatusAsync(request.RequestID, "Approved"))
                    {
                        await LoadDataAsync();
                    }
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
            string empName = request.Requester?.FullName ?? "Nhân viên";
            string message = (request.Status == "Approved" || request.Status == "Đã duyệt")
                ? $"Thay đổi quyết định: TỪ CHỐI đơn của {empName}?"
                : $"Từ chối đơn của {empName}?";

            if (MessageBox.Show(message, "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    if (string.IsNullOrEmpty(request.RequestID)) return;
                    if (await _leaveService.UpdateStatusAsync(request.RequestID, "Rejected"))
                    {
                        await LoadDataAsync();
                    }
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
            StartDate = req.StartDate ?? DateTime.Now;
            EndDate = req.EndDate ?? DateTime.Now;
            Reason = req.Reason;
            _editingRequestId = req.RequestID;
            SubmitButtonContent = "Cập Nhật";

            _originalLeaveType = req.LeaveType;
            _originalStartDate = req.StartDate ?? DateTime.Now;
            _originalEndDate = req.EndDate ?? DateTime.Now;
            _originalReason = req.Reason;

            OnPropertyChanged(nameof(CanSubmit));
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
                                OnPropertyChanged(nameof(CanSubmit));
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

        protected override bool CanAutoRefresh()
        {
            if (!string.IsNullOrEmpty(_editingRequestId) ||
                SubmitButtonContent == "Cập Nhật" ||
                !string.IsNullOrWhiteSpace(Reason))
            {
                return false;
            }
            return true;
        }

        [RelayCommand]
        public void ExportToWord()
        {
            if (string.IsNullOrWhiteSpace(Reason))
            {
                MessageBox.Show("Vui lòng nhập lý do trước khi xuất đơn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                FileName = $"Don_Xin_Nghi_{DateTime.Now:yyyyMMdd}_{CurrentUserId}.docx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var doc = DocX.Create(saveFileDialog.FileName))
                    {
                        doc.InsertParagraph("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM")
                           .FontSize(14).Bold().Alignment = Alignment.center;
                        doc.InsertParagraph("Độc lập - Tự do - Hạnh phúc")
                           .FontSize(14).Bold().UnderlineStyle(UnderlineStyle.singleLine).Alignment = Alignment.center;
                        doc.InsertParagraph("").SpacingAfter(20);
                        doc.InsertParagraph("ĐƠN XIN NGHỈ PHÉP")
                           .FontSize(20).Bold().Alignment = Alignment.center;
                        doc.InsertParagraph("").SpacingAfter(20);
                        doc.InsertParagraph($"Kính gửi: Ban Giám Đốc Công ty")
                           .FontSize(14).SpacingAfter(10);
                        var pInfo = doc.InsertParagraph();
                        pInfo.FontSize(14).SpacingBefore(10);
                        pInfo.Append($"Chức vụ: Nhân viên\n");
                        pInfo.Append($"Nay tôi làm đơn này để xin phép được nghỉ loại: {LeaveType}\n");
                        pInfo.Append($"Từ ngày: {StartDate:dd/MM/yyyy}   Đến ngày: {EndDate:dd/MM/yyyy}\n");
                        pInfo.Append($"Lý do: {Reason}\n");
                        doc.InsertParagraph("").SpacingAfter(20);
                        var table = doc.AddTable(1, 2);
                        table.Design = TableDesign.None;
                        table.Alignment = Alignment.center;
                        table.Rows[0].Cells[0].Paragraphs[0].Append("").Alignment = Alignment.center;
                        table.Rows[0].Cells[1].Paragraphs[0].Append($"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}\nNgười làm đơn")
                            .FontSize(14).Italic().Alignment = Alignment.center;
                        doc.InsertTable(table);
                        doc.Save();
                    }

                    MessageBox.Show("Xuất đơn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected override async Task PerformSilentUpdateAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentUserId)) return;
                var list = await _leaveService.GetRequestsByRoleAsync(CurrentUserId, _currentUserRole);
                list = list.OrderBy(r => (r.Status == "Pending" || r.Status == "Đang chờ") ? 0 : 1)
                           .ThenByDescending(r => r.StartDate)
                           .ToList();

                if (list.Count == RequestItems.Count)
                {
                    bool hasChange = false;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].Status != RequestItems[i].Request.Status ||
                            list[i].RequestID != RequestItems[i].Request.RequestID)
                        {
                            hasChange = true;
                            break;
                        }
                    }
                    if (!hasChange) return;
                }

                var viewList = new List<LeaveRequestItem>();
                foreach (var req in list)
                {
                    bool canApprove = IsManager && (req.EmployeeID != CurrentUserId);
                    bool canEdit = (req.EmployeeID == CurrentUserId) && (req.Status == "Pending" || req.Status == "Đang chờ");
                    viewList.Add(new LeaveRequestItem { Request = req, CanApprove = canApprove, CanEdit = canEdit });
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RequestItems = new ObservableCollection<LeaveRequestItem>(viewList);
                });
            }
            catch { }
        }
    }
}