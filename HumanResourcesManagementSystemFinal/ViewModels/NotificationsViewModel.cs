using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Department = HumanResourcesManagementSystemFinal.Models.Department;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class NotificationsViewModel : ObservableObject
    {
        private readonly bool _isAdmin;
        private ObservableCollection<Notification> _allNotifications;

        [ObservableProperty] private ObservableCollection<Notification> _filteredNotifications;
        [ObservableProperty] private ObservableCollection<Department> _departments;
        [ObservableProperty] private bool _isAdminView;

        [ObservableProperty] private bool _isFilterOpen;
        [ObservableProperty] private DateTime? _filterDate;
        [ObservableProperty] private string _selectedType = "Chung";
        [ObservableProperty] private string _selectedDepartment = "Tất cả";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteNotificationCommand))]
        [NotifyCanExecuteChangedFor(nameof(ExportWordCommand))]
        private Notification _selectedNotification;

        public NotificationsViewModel(bool isAdmin)
        {
            _isAdmin = isAdmin;
            IsAdminView = isAdmin;
            _allNotifications = new ObservableCollection<Notification>();
            FilteredNotifications = new ObservableCollection<Notification>();
            Departments = new ObservableCollection<Department>();

            LoadDepartments();
            LoadDataAsync();
        }

        public async void LoadDataAsync()
        {
            try
            {
                using var context = new DataContext();
                var list = await context.Notifications
                                        .OrderByDescending(n => n.Date)
                                        .ToListAsync();

                _allNotifications = new ObservableCollection<Notification>(list);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                _allNotifications = new ObservableCollection<Notification>();
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void LoadDepartments()
        {
            try
            {
                using var context = new DataContext();
                var dbList = context.Departments.AsNoTracking().ToList();

                var displayList = new ObservableCollection<Department>();
                displayList.Add(new Department { DepartmentName = "Tất cả" });

                foreach (var d in dbList)
                {
                    displayList.Add(d);
                }

                Departments = displayList;
            }
            catch
            {
                Departments = new ObservableCollection<Department>
                {
                    new Department { DepartmentName = "Tất cả" }
                };
            }
        }

        [RelayCommand]
        public void ApplyFilter()
        {
            if (_allNotifications == null) return;

            var query = _allNotifications.AsEnumerable();

            if (FilterDate.HasValue)
                query = query.Where(n => n.Date.Date == FilterDate.Value.Date);

            if (!string.IsNullOrEmpty(SelectedType) && SelectedType != "Chung")
                query = query.Where(n => n.Type == SelectedType);

            if (!string.IsNullOrEmpty(SelectedDepartment) && SelectedDepartment != "Tất cả")
                query = query.Where(n => n.Department == SelectedDepartment);

            FilteredNotifications = new ObservableCollection<Notification>(query);
            IsFilterOpen = false;
        }

        [RelayCommand]
        public void ShowAll()
        {
            FilterDate = null;
            SelectedType = "Chung";
            SelectedDepartment = "Tất cả";
            FilteredNotifications = new ObservableCollection<Notification>(_allNotifications);
        }

        [RelayCommand]
        public void AddNotification()
        {
            try
            {
                var addWindow = new AddNotificationWindow();
                bool? result = addWindow.ShowDialog();

                if (result == true)
                {
                    LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở cửa sổ thêm: " + ex.Message);
            }
        }

        private bool CanAction() => SelectedNotification != null;

        [RelayCommand(CanExecute = nameof(CanAction))]
        public void DeleteNotification()
        {
            if (SelectedNotification == null) return;

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa thông báo:\n{SelectedNotification.Title}?",
                                         "Xác nhận xóa",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new DataContext();
                    var itemToDelete = context.Notifications.FirstOrDefault(x => x.NotificationID == SelectedNotification.NotificationID);

                    if (itemToDelete != null)
                    {
                        context.Notifications.Remove(itemToDelete);
                        context.SaveChanges();
                    }

                    _allNotifications.Remove(SelectedNotification);
                    ApplyFilter();

                    MessageBox.Show("Đã xóa thành công!", "Thông báo");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi");
                }
            }
        }

        [RelayCommand(CanExecute = nameof(CanAction))]
        public void ExportWord()
        {
            if (SelectedNotification == null) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Document (*.doc)|*.doc",
                FileName = $"ThongBao_{SelectedNotification.Date:yyyyMMdd}_{SelectedNotification.NotificationID}.doc"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.Append("<html><head><meta charset='UTF-8'></head><body>");

                    sb.Append("<h3 style='color:blue; margin:0'>Hệ Thống HRMS PRO</h3>");
                    sb.Append("<p style='margin:0; font-size:13px'>Phòng Hành chính - Nhân sự</p>");
                    sb.Append($"<p style='margin:0; font-size:13px'><i>Ngày xuất: {DateTime.Now:dd/MM/yyyy}</i></p>");
                    sb.Append("<hr/>");

                    sb.Append("<div style='text-align:center; margin-top:20px'>");
                    sb.Append("<h1 style='color:red; margin:0'>THÔNG BÁO NỘI BỘ</h1>");
                    sb.Append($"<h3 style='margin-top:5px'><i>Tiêu đề: {SelectedNotification.Title}</i></h3>");
                    sb.Append("</div>");

                    sb.Append("<table border='1' style='border-collapse:collapse; width:100%; margin-top:15px'>");

                    void AddRow(string label, string value, string color = "black")
                    {
                        sb.Append("<tr>");
                        sb.Append($"<td style='padding:8px; width:30%; font-weight:bold'>{label}</td>");
                        sb.Append($"<td style='padding:8px; color:{color}'>{value}</td>");
                        sb.Append("</tr>");
                    }

                    AddRow("Người thông báo:", SelectedNotification.SenderID ?? "Admin");
                    AddRow("Phòng ban:", SelectedNotification.Department);

                    string typeColor = SelectedNotification.Type == "Khẩn cấp" ? "red" : "black";
                    AddRow("Loại thông báo:", SelectedNotification.Type, typeColor);

                    AddRow("Ngày đăng:", SelectedNotification.Date.ToString("dd/MM/yyyy HH:mm"));

                    sb.Append("</table>");

                    sb.Append("<h3 style='margin-top:20px'>Nội dung chi tiết:</h3>");
                    string contentHtml = SelectedNotification.Content.Replace("\n", "<br>");
                    sb.Append($"<p style='font-size:14px; line-height:1.5'>{contentHtml}</p>");

                    sb.Append("<br><br>");
                    sb.Append("<table style='width:100%'><tr>");
                    sb.Append("<td style='width:50%'></td>");
                    sb.Append("<td style='text-align:center'><strong>Người lập phiếu</strong><br><i>(Ký và ghi rõ họ tên)</i></td>");
                    sb.Append("</tr></table>");

                    sb.Append("</body></html>");

                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);

                    var result = MessageBox.Show("Xuất file thành công! Bạn muốn mở ngay không?",
                                                 "Thành công", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        new Process
                        {
                            StartInfo = new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true }
                        }.Start();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất file: " + ex.Message);
                }
            }
        }

        [RelayCommand]
        public void ItemDoubleClick(Notification item)
        {
            if (item != null)
            {
                var detailWindow = new NotificationsDetailWindow(item);
                detailWindow.ShowDialog();
            }
        }
    }
}