using CommunityToolkit.Mvvm.ComponentModel;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel; // Để chứa danh sách
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        [ObservableProperty] private int _totalEmployees;
        [ObservableProperty] private int _totalDepartments;
        [ObservableProperty] private int _pendingLeaves;
        [ObservableProperty] private int _expiringContracts;
        public ObservableCollection<Employee> RecentEmployees { get; set; } = new();
        public HomeViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                TotalEmployees = 100;
                TotalDepartments = 5;
                PendingLeaves = 3;
                ExpiringContracts = 2;
            }
            else
            {
                LoadDashboardData();
            }
        }
        private void LoadDashboardData()
        {
            using var context = new DataContext();
            _totalEmployees = context.Employees.Count(e => e.IsActive);
            _totalDepartments = context.Departments.Count();
            _pendingLeaves = 5;
            _expiringContracts = 2;
            var newHires = context.Employees.OrderByDescending(e => e.Id).Take(5).ToList();
            RecentEmployees.Clear();
            foreach(var emp in newHires)
            {
                RecentEmployees.Add(emp);
            }
        }

    }
}
