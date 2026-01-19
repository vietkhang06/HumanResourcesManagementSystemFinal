using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
// Dòng này để tránh lỗi trùng tên Department với thư viện OpenXML
using Department = HumanResourcesManagementSystemFinal.Models.Department;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class AddNotificationViewModel : ObservableObject
    {
        [ObservableProperty] private string _title;
        [ObservableProperty] private string _content;
        [ObservableProperty] private string _selectedType = "Chung";
        [ObservableProperty] private string _selectedDepartment = "Tất cả";

        // Danh sách phòng ban tải từ DB
        [ObservableProperty] private ObservableCollection<Department> _departments;

        public Action RequestClose;

        public AddNotificationViewModel()
        {
            Departments = new ObservableCollection<Department>();
            LoadDepartments();
        }

        private void LoadDepartments()
        {
            try
            {
                using var context = new DataContext();
                var dbList = context.Departments.ToList();

                var displayList = new ObservableCollection<Department>();
                displayList.Add(new Department { DepartmentName = "Tất cả" }); // Thêm thủ công

                foreach (var d in dbList) displayList.Add(d);

                Departments = displayList;
            }
            catch
            {
                // Fallback nếu lỗi DB
                Departments = new ObservableCollection<Department> { new Department { DepartmentName = "Tất cả" } };
            }
        }

        [RelayCommand]
        public void Save()
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Content))
            {
                MessageBox.Show("Vui lòng nhập đủ thông tin!");
                return;
            }

            try
            {
                using var context = new DataContext();
                var notif = new Notification
                {
                    Title = Title,
                    Content = Content,
                    Date = DateTime.Now,
                    Type = SelectedType,
                    Department = SelectedDepartment,
                    SenderID = "Admin" // Có thể sửa thành User ID hiện tại
                };
                context.Notifications.Add(notif);
                context.SaveChanges();

                MessageBox.Show("Thêm thành công!");
                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show("Lỗi lưu DB: " + msg);
            }
        }

        [RelayCommand]
        public void Cancel(Window w) => w?.Close();

        [RelayCommand]
        public void Minimize(Window w)
        {
            if (w != null) w.WindowState = WindowState.Minimized;
        }
    }
}