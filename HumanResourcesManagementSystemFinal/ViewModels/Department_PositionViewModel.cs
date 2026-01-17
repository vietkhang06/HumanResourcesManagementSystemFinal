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

        public Department_PositionViewModel()
        {
            _ = LoadDepartmentsAsync();
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                using var context = new DataContext();
                var list = await context.Departments
                    .AsNoTracking()
                    .ToListAsync();

                Departments.Clear();
                foreach (var dept in list) Departments.Add(dept);

                if (Departments.Count > 0 && SelectedDepartment == null)
                    SelectedDepartment = Departments[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách phòng ban:\n" + GetDeepErrorMessage(ex));
            }
        }

        private async Task LoadPositionsAsync()
        {
            try
            {
                if (SelectedDepartment == null)
                {
                    Positions.Clear();
                    return;
                }

                using var context = new DataContext();
                var list = await context.Positions
                    .Where(p => p.DepartmentID == SelectedDepartment.DepartmentID)
                    .AsNoTracking()
                    .ToListAsync();

                Positions.Clear();
                foreach (var pos in list) Positions.Add(pos);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách chức vụ:\n" + GetDeepErrorMessage(ex));
            }
        }

        [RelayCommand]
        private async Task AddDepartmentAsync()
        {
            var addVM = new AddDepartmentViewModel();
            var addWindow = new AddDepartmentWindow { DataContext = addVM };

            if (addWindow.ShowDialog() != true) return;

            using var context = new DataContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                if (await context.Departments.AnyAsync(d => d.DepartmentName == addVM.DeptName))
                {
                    MessageBox.Show($"Phòng ban '{addVM.DeptName}' đã tồn tại!");
                    return;
                }

                var newDept = new Department
                {
                    DepartmentID = GenerateID(context, "Department"),
                    DepartmentName = addVM.DeptName,
                    Location = addVM.DeptLocation,
                    ManagerID = addVM.SelectedManagerID
                };

                context.Departments.Add(newDept);

                string currentUserId = UserSession.CurrentEmployeeId;
                bool userExists = !string.IsNullOrEmpty(currentUserId) &&
                                  await context.Employees.AnyAsync(e => e.EmployeeID == currentUserId);
                string logUserId = userExists ? currentUserId : null;

                context.ChangeHistories.Add(new ChangeHistory
                {
                    LogID = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    ActionType = "CREATE",
                    TableName = "Departments",
                    RecordID = newDept.DepartmentID,
                    ChangeByUserID = logUserId,
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

            var editVM = new AddDepartmentViewModel(dept);
            var editWindow = new AddDepartmentWindow { DataContext = editVM };

            if (editWindow.ShowDialog() != true) return;

            using var context = new DataContext();

            try
            {
                var dbDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentID == dept.DepartmentID);
                if (dbDept == null) return;

                dbDept.DepartmentName = editVM.DeptName;
                dbDept.Location = editVM.DeptLocation;
                dbDept.ManagerID = editVM.SelectedManagerID;

                string currentUserId = UserSession.CurrentEmployeeId;
                bool userExists = !string.IsNullOrEmpty(currentUserId) &&
                                  await context.Employees.AnyAsync(e => e.EmployeeID == currentUserId);
                string logUserId = userExists ? currentUserId : null;

                context.ChangeHistories.Add(new ChangeHistory
                {
                    LogID = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    ActionType = "UPDATE",
                    TableName = "Departments",
                    RecordID = dbDept.DepartmentID,
                    ChangeByUserID = logUserId,
                    ChangeTime = DateTime.Now,
                    Details = $"Sửa phòng: {dbDept.DepartmentName}"
                });

                await context.SaveChangesAsync();

                dept.DepartmentName = editVM.DeptName;
                dept.Location = editVM.DeptLocation;
                dept.ManagerID = editVM.SelectedManagerID;

                int index = Departments.IndexOf(dept);
                if (index != -1)
                {
                    Departments[index] = null;
                    Departments[index] = dept;
                    SelectedDepartment = dept;
                }
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
                MessageBox.Show($"Không thể xóa phòng '{dept.DepartmentName}' vì đang có nhân viên trực thuộc!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa phòng '{dept.DepartmentName}'?\nTất cả chức vụ thuộc phòng này cũng sẽ bị xóa.",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                var childPositions = context.Positions.Where(p => p.DepartmentID == dept.DepartmentID);
                context.Positions.RemoveRange(childPositions);

                var dbDept = await context.Departments.FindAsync(dept.DepartmentID);
                if (dbDept != null) context.Departments.Remove(dbDept);

                string currentUserId = UserSession.CurrentEmployeeId;
                bool userExists = !string.IsNullOrEmpty(currentUserId) &&
                                  await context.Employees.AnyAsync(e => e.EmployeeID == currentUserId);
                string logUserId = userExists ? currentUserId : null;

                context.ChangeHistories.Add(new ChangeHistory
                {
                    LogID = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    ActionType = "DELETE",
                    TableName = "Departments",
                    RecordID = dept.DepartmentID,
                    ChangeByUserID = logUserId,
                    ChangeTime = DateTime.Now,
                    Details = $"Xóa phòng: {dept.DepartmentName}"
                });

                await context.SaveChangesAsync();

                Departments.Remove(dept);
                SelectedDepartment = Departments.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa phòng ban:\n" + GetDeepErrorMessage(ex));
            }
        }

        [RelayCommand]
        private async Task AddPositionAsync()
        {
            if (SelectedDepartment == null)
            {
                MessageBox.Show("Vui lòng chọn phòng ban trước khi thêm chức vụ!");
                return;
            }

            var addVM = new AddPositionViewModel();
            var addWindow = new AddPositionWindow { DataContext = addVM };

            if (addWindow.ShowDialog() != true) return;

            using var context = new DataContext();
            try
            {
                var newPos = new Position
                {
                    PositionID = GenerateID(context, "Position"),
                    PositionName = addVM.PosTitle,
                    JobDescription = addVM.JobDescription,
                    DepartmentID = SelectedDepartment.DepartmentID
                };

                context.Positions.Add(newPos);

                string currentUserId = UserSession.CurrentEmployeeId;
                bool userExists = !string.IsNullOrEmpty(currentUserId) &&
                                  await context.Employees.AnyAsync(e => e.EmployeeID == currentUserId);
                string logUserId = userExists ? currentUserId : null;

                context.ChangeHistories.Add(new ChangeHistory
                {
                    LogID = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    ActionType = "CREATE",
                    TableName = "Positions",
                    RecordID = newPos.PositionID,
                    ChangeByUserID = logUserId,
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

            if (string.IsNullOrEmpty(pos.PositionID))
            {
                MessageBox.Show("Lỗi dữ liệu: Chức vụ này không có ID hợp lệ. Hãy làm mới danh sách.",
                                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var editVM = new AddPositionViewModel(pos);
            var editWindow = new AddPositionWindow { DataContext = editVM };

            if (editWindow.ShowDialog() != true) return;

            using var context = new DataContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var dbPos = await context.Positions.FirstOrDefaultAsync(p => p.PositionID == pos.PositionID);

                if (dbPos == null)
                {
                    MessageBox.Show("Không tìm thấy chức vụ này trong cơ sở dữ liệu (có thể đã bị xóa).",
                                    "Lỗi đồng bộ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                dbPos.PositionName = editVM.PosTitle;
                dbPos.JobDescription = editVM.JobDescription;

                string currentUserId = UserSession.CurrentEmployeeId;
                bool userExists = !string.IsNullOrEmpty(currentUserId) &&
                                  await context.Employees.AnyAsync(e => e.EmployeeID == currentUserId);
                string logUserId = userExists ? currentUserId : null;

                context.ChangeHistories.Add(new ChangeHistory
                {
                    LogID = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    ActionType = "UPDATE",
                    TableName = "Positions",
                    RecordID = dbPos.PositionID,
                    ChangeByUserID = logUserId,
                    ChangeTime = DateTime.Now,
                    Details = $"Sửa chức vụ: {dbPos.PositionName}"
                });

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                var updatedPosUI = new Position
                {
                    PositionID = pos.PositionID,
                    PositionName = editVM.PosTitle,
                    JobDescription = editVM.JobDescription,
                    DepartmentID = pos.DepartmentID,
                    Department = pos.Department
                };

                int index = Positions.IndexOf(pos);
                if (index != -1)
                {
                    Positions[index] = updatedPosUI;
                }
                else
                {
                    _ = LoadPositionsAsync();
                }
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                MessageBox.Show($"Lỗi CSDL khi lưu:\n{innerMessage}", "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                MessageBox.Show($"Lỗi không xác định:\n{ex.Message}", "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeletePositionAsync(Position pos)
        {
            if (pos == null) return;

            using var context = new DataContext();

            if (await context.Employees.AnyAsync(e => e.PositionID == pos.PositionID))
            {
                MessageBox.Show("Không thể xóa chức vụ này vì đang có nhân viên đảm nhận!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc muốn xóa chức vụ '{pos.PositionName}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                var dbPos = await context.Positions.FindAsync(pos.PositionID);
                if (dbPos != null) context.Positions.Remove(dbPos);

                string currentUserId = UserSession.CurrentEmployeeId;
                bool userExists = !string.IsNullOrEmpty(currentUserId) &&
                                  await context.Employees.AnyAsync(e => e.EmployeeID == currentUserId);
                string logUserId = userExists ? currentUserId : null;

                context.ChangeHistories.Add(new ChangeHistory
                {
                    LogID = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    ActionType = "DELETE",
                    TableName = "Positions",
                    RecordID = pos.PositionID,
                    ChangeByUserID = logUserId,
                    ChangeTime = DateTime.Now,
                    Details = $"Xóa chức vụ: {pos.PositionName}"
                });

                await context.SaveChangesAsync();
                Positions.Remove(pos);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa chức vụ:\n" + GetDeepErrorMessage(ex));
            }
        }

        private string GenerateID(DataContext context, string type)
        {
            string prefix = type == "Department" ? "PB" : "CV";

            var lastID = type == "Department"
                ? context.Departments.OrderByDescending(d => d.DepartmentID).Select(d => d.DepartmentID).FirstOrDefault()
                : context.Positions.OrderByDescending(p => p.PositionID).Select(p => p.PositionID).FirstOrDefault();

            if (string.IsNullOrEmpty(lastID)) return prefix + "001";

            if (lastID.Length > prefix.Length && int.TryParse(lastID.Substring(prefix.Length), out int num))
            {
                return prefix + (num + 1).ToString("D3");
            }

            return prefix + DateTime.Now.Ticks.ToString()[^3..];
        }

        private string GetDeepErrorMessage(Exception ex)
        {
            var sb = new StringBuilder(ex.Message);
            var inner = ex.InnerException;
            while (inner != null)
            {
                sb.AppendLine("\nChi tiết: " + inner.Message);
                inner = inner.InnerException;
            }
            return sb.ToString();
        }
    }
}