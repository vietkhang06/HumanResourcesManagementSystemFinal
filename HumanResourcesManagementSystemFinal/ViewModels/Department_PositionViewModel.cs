using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class Department_PositionViewModel : ObservableObject
    {
        private Department selectedDepartment;

        public ObservableCollection<Department> Departments { get; set; } = new();
        public ObservableCollection<Position> Positions { get; set; } = new();

        public Department SelectedDepartment
        {
            get => selectedDepartment;
            set
            {
                if (SetProperty(ref selectedDepartment, value))
                {
                    _ = LoadPositionsAsync();
                }
            }
        }

        public Department_PositionViewModel()
        {
            _ = LoadDepartmentsAsync();
        }

        private string GenerateID(DataContext context, string type)
        {
            string prefix;
            string lastID;

            if (type == "Department")
            {
                prefix = "PB";
                lastID = context.Departments
                    .OrderByDescending(d => d.DepartmentID)
                    .Select(d => d.DepartmentID)
                    .FirstOrDefault();
            }
            else
            {
                prefix = "CV";
                lastID = context.Positions
                    .OrderByDescending(p => p.PositionID)
                    .Select(p => p.PositionID)
                    .FirstOrDefault();
            }

            if (string.IsNullOrEmpty(lastID)) return prefix + "001";

            string numPart = lastID.Substring(prefix.Length);

            if (int.TryParse(numPart, out int num)) return prefix + (num + 1).ToString("D3");

            return prefix + new Random().Next(100, 999);
        }

        private string GetDeepErrorMessage(Exception ex)
        {
            var sb = new StringBuilder(ex.Message);
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
                var list = await context.Departments
                    .AsNoTracking()
                    .ToListAsync();

                Departments = new ObservableCollection<Department>(list);
                OnPropertyChanged(nameof(Departments));

                if (Departments.Count > 0 && SelectedDepartment == null)
                    SelectedDepartment = Departments[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi tải danh sách phòng ban:\n" + GetDeepErrorMessage(ex),
                    "Lỗi hệ thống"
                );
            }
        }

        private async Task LoadPositionsAsync()
        {
            try
            {
                if (SelectedDepartment == null)
                {
                    Positions.Clear();
                    OnPropertyChanged(nameof(Positions));
                    return;
                }

                using var context = new DataContext();
                var list = await context.Positions
                    .Where(p => p.DepartmentID == SelectedDepartment.DepartmentID)
                    .AsNoTracking()
                    .ToListAsync();

                Positions = new ObservableCollection<Position>(list);
                OnPropertyChanged(nameof(Positions));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi tải danh sách chức vụ:\n" + GetDeepErrorMessage(ex),
                    "Lỗi hệ thống"
                );
            }
        }

        [RelayCommand]
        private async Task AddDepartmentAsync()
        {
            var addWindow = new AddDepartmentWindow();
            if (addWindow.ShowDialog() != true) return;

            using var context = new DataContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                if (await context.Departments.AnyAsync(d => d.DepartmentName == addWindow.DeptName))
                {
                    MessageBox.Show($"Phòng ban '{addWindow.DeptName}' đã tồn tại!");
                    return;
                }

                var newDept = new Department
                {
                    DepartmentID = GenerateID(context, "Department"),
                    DepartmentName = addWindow.DeptName,
                    Location = addWindow.DeptLocation
                };

                context.Departments.Add(newDept);

                string adminID = string.IsNullOrEmpty(UserSession.CurrentEmployeeId)
                    ? "ADMIN"
                    : UserSession.CurrentEmployeeId;

                context.ChangeHistories.Add(new ChangeHistory
                {
                    LogID = Guid.NewGuid().ToString()[..8].ToUpper(),
                    ActionType = "CREATE",
                    TableName = "Departments",
                    RecordID = newDept.DepartmentID,
                    ChangeByUserID = adminID,
                    ChangeTime = DateTime.Now,
                    Details = $"Thêm phòng: {newDept.DepartmentName}"
                });

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                Departments.Add(newDept);
                SelectedDepartment = newDept;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                MessageBox.Show("Lỗi thêm phòng ban:\n" + GetDeepErrorMessage(ex));
            }
        }

        [RelayCommand]
        private async Task EditDepartmentAsync(Department dept)
        {
            if (dept == null) return;

            var editWindow = new AddDepartmentWindow(dept);
            if (editWindow.ShowDialog() != true) return;

            using var context = new DataContext();

            try
            {
                var dbDept = await context.Departments
                    .FirstOrDefaultAsync(d => d.DepartmentID == dept.DepartmentID);

                if (dbDept == null) return;

                dbDept.DepartmentName = editWindow.DeptName;
                dbDept.Location = editWindow.DeptLocation;

                string adminID = string.IsNullOrEmpty(UserSession.CurrentEmployeeId)
                    ? "ADMIN"
                    : UserSession.CurrentEmployeeId;

                context.ChangeHistories.Add(new ChangeHistory
                {
                    LogID = Guid.NewGuid().ToString()[..8].ToUpper(),
                    ActionType = "UPDATE",
                    TableName = "Departments",
                    RecordID = dbDept.DepartmentID,
                    ChangeByUserID = adminID,
                    ChangeTime = DateTime.Now,
                    Details = $"Sửa phòng: {dbDept.DepartmentName}"
                });

                await context.SaveChangesAsync();

                dept.DepartmentName = editWindow.DeptName;
                dept.Location = editWindow.DeptLocation;

                int index = Departments.IndexOf(dept);
                if (index != -1)
                    Departments[index] = dept;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật:\n" + GetDeepErrorMessage(ex));
            }
        }

        [RelayCommand]
        private async Task DeleteDepartmentAsync(Department dept)
        {
            if (dept == null) return;

            using var context = new DataContext();

            if (await context.Employees.AnyAsync(e => e.DepartmentID == dept.DepartmentID))
            {
                MessageBox.Show("Không thể xóa vì đang có nhân viên!");
                return;
            }

            if (MessageBox.Show(
                $"Xóa phòng '{dept.DepartmentName}'?",
                "Xác nhận",
                MessageBoxButton.YesNo
            ) != MessageBoxResult.Yes) return;

            try
            {
                context.Positions.RemoveRange(
                    context.Positions.Where(p => p.DepartmentID == dept.DepartmentID)
                );

                var dbDept = await context.Departments
                    .FirstOrDefaultAsync(d => d.DepartmentID == dept.DepartmentID);

                if (dbDept != null)
                    context.Departments.Remove(dbDept);

                string adminID = string.IsNullOrEmpty(UserSession.CurrentEmployeeId)
                    ? "ADMIN"
                    : UserSession.CurrentEmployeeId;

                context.ChangeHistories.Add(new ChangeHistory
                {
                    LogID = Guid.NewGuid().ToString()[..8].ToUpper(),
                    ActionType = "DELETE",
                    TableName = "Departments",
                    RecordID = dept.DepartmentID,
                    ChangeByUserID = adminID,
                    ChangeTime = DateTime.Now,
                    Details = $"Xóa phòng: {dept.DepartmentName}"
                });

                await context.SaveChangesAsync();

                Departments.Remove(dept);
                SelectedDepartment = Departments.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa:\n" + GetDeepErrorMessage(ex));
            }
        }

        [RelayCommand]
        private async Task AddPositionAsync()
        {
            if (SelectedDepartment == null)
            {
                MessageBox.Show("Vui lòng chọn phòng ban!");
                return;
            }

            var addWindow = new AddPositionWindow();
            if (addWindow.ShowDialog() != true) return;

            using var context = new DataContext();

            try
            {
                var newPos = new Position
                {
                    PositionID = GenerateID(context, "Position"),
                    PositionName = addWindow.PosTitle,
                    JobDescription = addWindow.JobDescription,
                    DepartmentID = SelectedDepartment.DepartmentID
                };

                context.Positions.Add(newPos);

                string adminID = string.IsNullOrEmpty(UserSession.CurrentEmployeeId)
                    ? "ADMIN"
                    : UserSession.CurrentEmployeeId;

                context.ChangeHistories.Add(new ChangeHistory
                {
                    LogID = Guid.NewGuid().ToString()[..8].ToUpper(),
                    ActionType = "CREATE",
                    TableName = "Positions",
                    RecordID = newPos.PositionID,
                    ChangeByUserID = adminID,
                    ChangeTime = DateTime.Now,
                    Details = $"Thêm chức vụ: {newPos.PositionName}"
                });

                await context.SaveChangesAsync();
                Positions.Add(newPos);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm chức vụ:\n" + GetDeepErrorMessage(ex));
            }
        }

        [RelayCommand]
        private async Task EditPositionAsync(Position pos)
        {
            if (pos == null) return;

            var editWindow = new AddPositionWindow(pos);
            if (editWindow.ShowDialog() != true) return;

            using var context = new DataContext();

            try
            {
                var dbPos = await context.Positions
                    .FirstOrDefaultAsync(p => p.PositionID == pos.PositionID);

                if (dbPos == null) return;

                dbPos.PositionName = editWindow.PosTitle;
                dbPos.JobDescription = editWindow.JobDescription;

                string adminID = string.IsNullOrEmpty(UserSession.CurrentEmployeeId)
                    ? "ADMIN"
                    : UserSession.CurrentEmployeeId;

                context.ChangeHistories.Add(new ChangeHistory
                {
                    LogID = Guid.NewGuid().ToString()[..8].ToUpper(),
                    ActionType = "UPDATE",
                    TableName = "Positions",
                    RecordID = pos.PositionID,
                    ChangeByUserID = adminID,
                    ChangeTime = DateTime.Now,
                    Details = $"Sửa chức vụ: {dbPos.PositionName}"
                });

                await context.SaveChangesAsync();

                pos.PositionName = editWindow.PosTitle;
                pos.JobDescription = editWindow.JobDescription;

                int index = Positions.IndexOf(pos);
                if (index != -1)
                    Positions[index] = pos;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật chức vụ:\n" + GetDeepErrorMessage(ex));
            }
        }

        [RelayCommand]
        private async Task DeletePositionAsync(Position pos)
        {
            if (pos == null) return;

            using var context = new DataContext();

            if (await context.Employees.AnyAsync(e => e.PositionID == pos.PositionID))
            {
                MessageBox.Show("Không thể xóa vì đang có nhân viên!");
                return;
            }

            if (MessageBox.Show(
                $"Xóa chức vụ '{pos.PositionName}'?",
                "Xác nhận",
                MessageBoxButton.YesNo
            ) != MessageBoxResult.Yes) return;

            try
            {
                var dbPos = await context.Positions
                    .FirstOrDefaultAsync(p => p.PositionID == pos.PositionID);

                if (dbPos != null)
                {
                    context.Positions.Remove(dbPos);

                    string adminID = string.IsNullOrEmpty(UserSession.CurrentEmployeeId)
                        ? "ADMIN"
                        : UserSession.CurrentEmployeeId;

                    context.ChangeHistories.Add(new ChangeHistory
                    {
                        LogID = Guid.NewGuid().ToString()[..8].ToUpper(),
                        ActionType = "DELETE",
                        TableName = "Positions",
                        RecordID = pos.PositionID,
                        ChangeByUserID = adminID,
                        ChangeTime = DateTime.Now,
                        Details = $"Xóa chức vụ: {pos.PositionName}"
                    });

                    await context.SaveChangesAsync();
                    Positions.Remove(pos);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa chức vụ:\n" + GetDeepErrorMessage(ex));
            }
        }
    }
}
