using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using System;
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
                context.Database.EnsureCreated();

                var dbDepts = context.Departments.ToList();

                Departments.Clear();
                foreach (var dept in dbDepts)
                {
                    Departments.Add(dept);
                }

                if (Departments.Count > 0)
                {
                    SelectedDepartment = Departments[0];
                }
            }
        }

        private void LoadPositions()
        {
            Positions.Clear();
            if (SelectedDepartment == null) return;

            using (var context = new DataContext())
            {
                var dbPositions = context.Positions.ToList();

                foreach (var pos in dbPositions)
                {
                    Positions.Add(pos);
                }
            }
        }

        [RelayCommand]
        private void AddDepartment()
        {
            var addWindow = new AddDepartmentWindow();
            if (addWindow.ShowDialog() == true)
            {
                using (var context = new DataContext())
                {
                    var newDept = new Department
                    {
                        DepartmentName = addWindow.DeptName,
                        Location = "Chưa cập nhật"
                    };

                    context.Departments.Add(newDept);
                    context.SaveChanges();

                    Departments.Add(newDept);
                    SelectedDepartment = newDept;

                    MessageBox.Show("Thêm phòng ban thành công!", "Thông báo");
                }
            }
        }

        [RelayCommand]
        private void AddPosition()
        {
            if (SelectedDepartment == null)
            {
                MessageBox.Show("Vui lòng chọn một phòng ban trước khi thêm vị trí!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var addWindow = new AddPositionWindow();
            if (addWindow.ShowDialog() == true)
            {
                using (var context = new DataContext())
                {
                    var newPos = new Position
                    {
                        Title = addWindow.PosTitle,
                        JobDescription = "Mô tả công việc mặc định"
                    };

                    context.Positions.Add(newPos);
                    context.SaveChanges();

                    Positions.Add(newPos);

                    MessageBox.Show($"Đã thêm vị trí '{newPos.Title}' thành công!", "Thông báo");
                }
            }
        }
    }
}