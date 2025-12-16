using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services; // Nhớ using AuditService
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class Department_PositionViewModel : ObservableObject
    {
        public ObservableCollection<Department> Departments { get; set; } = new();
        public ObservableCollection<Position> Positions { get; set; } = new();

        private Department _selectedDepartment;
        public Department SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                if (SetProperty(ref _selectedDepartment, value))
                {
                    LoadPositions();
                }
            }
        }

        public Department_PositionViewModel()
        {
            LoadDepartments();
        }

        private void LoadDepartments()
        {
            using (var context = new DataContext())
            {
                var dbDepts = context.Departments.ToList();
                Departments.Clear();
                foreach (var dept in dbDepts) Departments.Add(dept);

                if (Departments.Count > 0 && SelectedDepartment == null)
                    SelectedDepartment = Departments[0];
            }
        }

        private void LoadPositions()
        {
            Positions.Clear();
            if (SelectedDepartment == null) return;

            using (var context = new DataContext())
            {
                var dbPositions = context.Positions
                    .Where(p => p.Id == SelectedDepartment.Id)
                    .ToList();
                foreach (var pos in dbPositions) Positions.Add(pos);
            }
        }

        // ================== QUẢN LÝ PHÒNG BAN ==================

        [RelayCommand]
        private void AddDepartment()
        {
            var addWindow = new AddDepartmentWindow();
            if (addWindow.ShowDialog() == true)
            {
                using (var context = new DataContext())
                {
                    var newDept = new Department { DepartmentName = addWindow.DeptName, Location = addWindow.DeptLocation };
                    context.Departments.Add(newDept);

                    AuditService.LogChange(context, "Departments", "CREATE", 0, 1, $"Thêm phòng: {newDept.DepartmentName}");
                    context.SaveChanges();

                    Departments.Add(newDept);
                    SelectedDepartment = newDept;
                }
            }
        }

        [RelayCommand]
        private void EditDepartment(Department dept)
        {
            if (dept == null) return;
            var editWindow = new AddDepartmentWindow(dept);
            if (editWindow.ShowDialog() == true)
            {
                using (var context = new DataContext())
                {
                    var dbDept = context.Departments.Find(dept.Id);
                    if (dbDept != null)
                    {
                        dbDept.DepartmentName = editWindow.DeptName;
                        dbDept.Location = editWindow.DeptLocation;

                        AuditService.LogChange(context, "Departments", "UPDATE", dept.Id, 1, $"Sửa phòng: {dbDept.DepartmentName}");
                        context.SaveChanges();

                        // Cập nhật UI
                        dept.DepartmentName = editWindow.DeptName;
                        dept.Location = editWindow.DeptLocation;

                        // Hack nhỏ để ListBox cập nhật hiển thị
                        var index = Departments.IndexOf(dept);
                        Departments[index] = dept;
                        SelectedDepartment = dept;
                    }
                }
            }
        }

        [RelayCommand]
        private void DeleteDepartment(Department dept)
        {
            if (dept == null) return;

            // Kiểm tra ràng buộc trước khi xóa
            using (var context = new DataContext())
            {
                bool hasEmployees = context.Employees.Any(e => e.DepartmentId == dept.Id);
                bool hasPositions = context.Positions.Any(p => p.Id == dept.Id);

                if (hasEmployees || hasPositions)
                {
                    MessageBox.Show($"Không thể xóa phòng '{dept.DepartmentName}' vì đang có Nhân viên hoặc Chức vụ trực thuộc!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show($"Bạn có chắc chắn muốn xóa phòng '{dept.DepartmentName}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var dbDept = context.Departments.Find(dept.Id);
                    context.Departments.Remove(dbDept);

                    AuditService.LogChange(context, "Departments", "DELETE", dept.Id, 1, $"Xóa phòng: {dept.DepartmentName}");
                    context.SaveChanges();

                    Departments.Remove(dept);
                    if (Departments.Count > 0) SelectedDepartment = Departments[0];
                }
            }
        }

        // ================== QUẢN LÝ CHỨC VỤ ==================

        [RelayCommand]
        private void AddPosition()
        {
            if (SelectedDepartment == null)
            {
                MessageBox.Show("Vui lòng chọn một phòng ban trước!", "Cảnh báo");
                return;
            }

            var addWindow = new AddPositionWindow();
            if (addWindow.ShowDialog() == true)
            {
                using (var context = new DataContext())
                {
                    // 1. Tự tính ID mới (Giữ nguyên cái này để không bị lỗi UNIQUE ID)
                    int newId = 1;
                    if (context.Positions.Any())
                    {
                        newId = context.Positions.Max(p => p.Id) + 1;
                    }

                    // 2. Tạo chức vụ mới
                    var newPos = new Position
                    {
                        Id = newId,
                        Title = addWindow.PosTitle,
                        JobDescription = addWindow.JobDescription,

                        // Gắn ID phòng ban vào (Vì là int? nên gán int vào vẫn nhận bình thường)
                        DepartmentId = SelectedDepartment.Id
                    };

                    context.Positions.Add(newPos);

                    // Ghi lịch sử
                    AuditService.LogChange(context, "Positions", "CREATE", newPos.Id, 1, $"Thêm chức vụ: {newPos.Title}");

                    context.SaveChanges();

                    // Cập nhật giao diện
                    Positions.Add(newPos);
                    MessageBox.Show($"Đã thêm chức vụ '{newPos.Title}' vào phòng '{SelectedDepartment.DepartmentName}'!", "Thông báo");
                }
            }
        }

        [RelayCommand]
        private void EditPosition(Position pos)
        {
            if (pos == null) return;
            var editWindow = new AddPositionWindow(pos);
            if (editWindow.ShowDialog() == true)
            {
                using (var context = new DataContext())
                {
                    var dbPos = context.Positions.Find(pos.Id);
                    if (dbPos != null)
                    {
                        dbPos.Title = editWindow.PosTitle;
                        dbPos.JobDescription = editWindow.JobDescription;

                        AuditService.LogChange(context, "Positions", "UPDATE", pos.Id, 1, $"Sửa chức vụ: {dbPos.Title}");
                        context.SaveChanges();

                        // Cập nhật UI
                        pos.Title = editWindow.PosTitle;
                        pos.JobDescription = editWindow.JobDescription;
                        LoadPositions(); // Refresh lại list
                    }
                }
            }
        }

        [RelayCommand]
        private void DeletePosition(Position pos)
        {
            if (pos == null) return;
            using (var context = new DataContext())
            {
                bool hasEmp = context.Employees.Any(e => e.PositionId == pos.Id);
                if (hasEmp)
                {
                    MessageBox.Show($"Không thể xóa chức vụ '{pos.Title}' vì đang có nhân viên nắm giữ!", "Cảnh báo");
                    return;
                }

                if (MessageBox.Show($"Xóa chức vụ '{pos.Title}'?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var dbPos = context.Positions.Find(pos.Id);
                    context.Positions.Remove(dbPos);

                    AuditService.LogChange(context, "Positions", "DELETE", pos.Id, 1, $"Xóa chức vụ: {pos.Title}");
                    context.SaveChanges();

                    Positions.Remove(pos);
                }
            }
        }
    }
}