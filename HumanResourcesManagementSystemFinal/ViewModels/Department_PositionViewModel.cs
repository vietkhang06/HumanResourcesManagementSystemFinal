using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class Department_PositionViewModel : ObservableObject
{
    private Department _selectedDepartment;

    public ObservableCollection<Department> Departments { get; set; } = new();
    public ObservableCollection<Position> Positions { get; set; } = new();

    public Department SelectedDepartment
    {
        get => _selectedDepartment;
        set
        {
            if (SetProperty(ref _selectedDepartment, value))
            {
                _ = LoadPositionsAsync();
            }
        }
    }

    private int _currentAdminId = UserSession.CurrentEmployeeId != 0 ? UserSession.CurrentEmployeeId : 1;

    public Department_PositionViewModel()
    {
        _ = LoadDepartmentsAsync();
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

    private async Task LoadDepartmentsAsync()
    {
        try
        {
            using var context = new DataContext();
            var list = await context.Departments.AsNoTracking().ToListAsync();

            Departments = new ObservableCollection<Department>(list);
            OnPropertyChanged(nameof(Departments));

            if (Departments.Count > 0 && SelectedDepartment == null)
            {
                SelectedDepartment = Departments[0];
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tải danh sách phòng ban:\n" + GetDeepErrorMessage(ex), "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadPositionsAsync()
    {
        if (SelectedDepartment == null)
        {
            Positions = new ObservableCollection<Position>();
            OnPropertyChanged(nameof(Positions));
            return;
        }

        try
        {
            using var context = new DataContext();
            var list = await context.Positions
                .AsNoTracking()
                .Where(p => p.DepartmentId == SelectedDepartment.Id)
                .ToListAsync();

            Positions = new ObservableCollection<Position>(list);
            OnPropertyChanged(nameof(Positions));
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tải danh sách chức vụ:\n" + GetDeepErrorMessage(ex), "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task AddDepartmentAsync()
    {
        var addWindow = new AddDepartmentWindow();
        if (addWindow.ShowDialog() == true)
        {
            using var context = new DataContext();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                if (await context.Departments.AnyAsync(d => d.DepartmentName == addWindow.DeptName))
                {
                    MessageBox.Show($"Phòng ban '{addWindow.DeptName}' đã tồn tại!", "Trùng lặp", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newDept = new Department
                {
                    DepartmentName = addWindow.DeptName,
                    Location = addWindow.DeptLocation
                };

                context.Departments.Add(newDept);
                await context.SaveChangesAsync();

                AuditService.LogChange(context, "Departments", "CREATE", newDept.Id, _currentAdminId, $"Thêm phòng: {newDept.DepartmentName}");
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                Departments.Add(newDept);
                SelectedDepartment = newDept;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                MessageBox.Show("Không thể thêm phòng ban:\n" + GetDeepErrorMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task EditDepartmentAsync(Department dept)
    {
        if (dept == null) return;
        var editWindow = new AddDepartmentWindow(dept);

        if (editWindow.ShowDialog() == true)
        {
            using var context = new DataContext();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var dbDept = await context.Departments.FindAsync(dept.Id);
                if (dbDept == null)
                {
                    MessageBox.Show("Phòng ban không còn tồn tại trong hệ thống.", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dbDept.DepartmentName = editWindow.DeptName;
                dbDept.Location = editWindow.DeptLocation;

                AuditService.LogChange(context, "Departments", "UPDATE", dept.Id, _currentAdminId, $"Sửa phòng: {dbDept.DepartmentName}");
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                dept.DepartmentName = editWindow.DeptName;
                dept.Location = editWindow.DeptLocation;

                var index = Departments.IndexOf(dept);
                if (index >= 0) Departments[index] = dept;

                SelectedDepartment = dept;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                MessageBox.Show("Không thể cập nhật phòng ban:\n" + GetDeepErrorMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task DeleteDepartmentAsync(Department dept)
    {
        if (dept == null) return;

        try
        {
            using var context = new DataContext();

            bool hasEmployees = await context.Employees.AnyAsync(e => e.DepartmentId == dept.Id);
            bool hasPositions = await context.Positions.AnyAsync(p => p.DepartmentId == dept.Id);

            if (hasEmployees || hasPositions)
            {
                MessageBox.Show($"Không thể xóa phòng '{dept.DepartmentName}' vì đang có Nhân viên hoặc Chức vụ trực thuộc!", "Ràng buộc dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa phòng '{dept.DepartmentName}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var dbDept = await context.Departments.FindAsync(dept.Id);
                if (dbDept != null)
                {
                    context.Departments.Remove(dbDept);
                    AuditService.LogChange(context, "Departments", "DELETE", dept.Id, _currentAdminId, $"Xóa phòng: {dept.DepartmentName}");
                    await context.SaveChangesAsync();
                }

                Departments.Remove(dept);
                if (Departments.Count > 0) SelectedDepartment = Departments[0];
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể xóa phòng ban:\n" + GetDeepErrorMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task AddPositionAsync()
    {
        if (SelectedDepartment == null)
        {
            MessageBox.Show("Vui lòng chọn một phòng ban trước!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var addWindow = new AddPositionWindow();
        if (addWindow.ShowDialog() == true)
        {
            using var context = new DataContext();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var newPos = new Position
                {
                    Title = addWindow.PosTitle,
                    JobDescription = addWindow.JobDescription,
                    DepartmentId = SelectedDepartment.Id
                };

                context.Positions.Add(newPos);
                await context.SaveChangesAsync();

                AuditService.LogChange(context, "Positions", "CREATE", newPos.Id, _currentAdminId, $"Thêm chức vụ: {newPos.Title}");
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                Positions.Add(newPos);
                MessageBox.Show($"Đã thêm chức vụ '{newPos.Title}' vào phòng '{SelectedDepartment.DepartmentName}'!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                MessageBox.Show("Không thể thêm chức vụ:\n" + GetDeepErrorMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task EditPositionAsync(Position pos)
    {
        if (pos == null) return;
        var editWindow = new AddPositionWindow(pos);
        if (editWindow.ShowDialog() == true)
        {
            using var context = new DataContext();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var dbPos = await context.Positions.FindAsync(pos.Id);
                if (dbPos == null)
                {
                    MessageBox.Show("Chức vụ không còn tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dbPos.Title = editWindow.PosTitle;
                dbPos.JobDescription = editWindow.JobDescription;

                AuditService.LogChange(context, "Positions", "UPDATE", pos.Id, _currentAdminId, $"Sửa chức vụ: {dbPos.Title}");
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                pos.Title = editWindow.PosTitle;
                pos.JobDescription = editWindow.JobDescription;

                var index = Positions.IndexOf(pos);
                if (index >= 0) Positions[index] = pos;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                MessageBox.Show("Không thể cập nhật chức vụ:\n" + GetDeepErrorMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task DeletePositionAsync(Position pos)
    {
        if (pos == null) return;

        try
        {
            using var context = new DataContext();
            bool hasEmp = await context.Employees.AnyAsync(e => e.PositionId == pos.Id);

            if (hasEmp)
            {
                MessageBox.Show($"Không thể xóa chức vụ '{pos.Title}' vì đang có nhân viên nắm giữ!", "Ràng buộc dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa chức vụ '{pos.Title}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var dbPos = await context.Positions.FindAsync(pos.Id);
                if (dbPos != null)
                {
                    context.Positions.Remove(dbPos);
                    AuditService.LogChange(context, "Positions", "DELETE", pos.Id, _currentAdminId, $"Xóa chức vụ: {pos.Title}");
                    await context.SaveChangesAsync();
                }
                Positions.Remove(pos);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể xóa chức vụ:\n" + GetDeepErrorMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}