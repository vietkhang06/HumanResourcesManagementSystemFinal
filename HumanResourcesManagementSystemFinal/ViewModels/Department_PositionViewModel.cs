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

        // --- HÀM SINH MÃ TỰ ĐỘNG ---
        private string GenerateID(DataContext context, string type)
        {
            string lastID = "";
            string prefix = "";

            if (type == "Department")
            {
                prefix = "PB";
                lastID = context.Departments.OrderByDescending(d => d.DepartmentID).Select(d => d.DepartmentID).FirstOrDefault();
            }
            else if (type == "Position")
            {
                prefix = "CV";
                lastID = context.Positions.OrderByDescending(p => p.PositionID).Select(p => p.PositionID).FirstOrDefault();
            }

            if (string.IsNullOrEmpty(lastID)) return prefix + "001";

            string numPart = lastID.Substring(prefix.Length);
            if (int.TryParse(numPart, out int num))
            {
                return prefix + (num + 1).ToString("D3");
            }

            return prefix + new Random().Next(100, 999);
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

        // --- LOAD DATA ---
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
                MessageBox.Show("Lỗi tải danh sách phòng ban:\n" + GetDeepErrorMessage(ex), "Lỗi hệ thống");
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
                // Lọc chức vụ theo DepartmentID
                var list = await context.Positions
                    .Where(p => p.DepartmentID == SelectedDepartment.DepartmentID)
                    .AsNoTracking()
                    .ToListAsync();

                Positions = new ObservableCollection<Position>(list);
                OnPropertyChanged(nameof(Positions));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách chức vụ:\n" + GetDeepErrorMessage(ex), "Lỗi hệ thống");
            }
        }

        // --- CRUD PHÒNG BAN ---
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
                        MessageBox.Show($"Phòng ban '{addWindow.DeptName}' đã tồn tại!", "Trùng lặp");
                        return;
                    }

                    string newID = GenerateID(context, "Department");

                    var newDept = new Department
                    {
                        DepartmentID = newID,
                        DepartmentName = addWindow.DeptName,
                        Location = addWindow.DeptLocation,
                        ManagerID = null
                    };

                    context.Departments.Add(newDept);

                    // [ĐÃ SỬA LỖI CS0023 TẠI ĐÂY]
                    string adminID = UserSession.CurrentEmployeeId == 0 ? "ADMIN" : UserSession.CurrentEmployeeId.ToString();

                    context.ChangeHistories.Add(new ChangeHistory
                    {
                        LogID = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
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
                    MessageBox.Show("Lỗi thêm phòng ban:\n" + GetDeepErrorMessage(ex), "Lỗi");
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
                    var dbDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentID == dept.DepartmentID);
                    if (dbDept == null) return;

                    dbDept.DepartmentName = editWindow.DeptName;
                    dbDept.Location = editWindow.DeptLocation;

                    // [ĐÃ SỬA LỖI CS0023 TẠI ĐÂY]
                    string adminID = UserSession.CurrentEmployeeId == 0 ? "ADMIN" : UserSession.CurrentEmployeeId.ToString();

                    context.ChangeHistories.Add(new ChangeHistory
                    {
                        LogID = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                        ActionType = "UPDATE",
                        TableName = "Departments",
                        RecordID = dbDept.DepartmentID,
                        ChangeByUserID = adminID,
                        ChangeTime = DateTime.Now,
                        Details = $"Sửa phòng: {dbDept.DepartmentName}"
                    });

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    dept.DepartmentName = editWindow.DeptName;
                    dept.Location = editWindow.DeptLocation;

                    // Refresh UI
                    int index = Departments.IndexOf(dept);
                    if (index != -1)
                    {
                        Departments[index] = dept;
                        SelectedDepartment = dept;
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    MessageBox.Show("Lỗi cập nhật:\n" + GetDeepErrorMessage(ex), "Lỗi");
                }
            }
        }

        [RelayCommand]
        private async Task DeleteDepartmentAsync(Department dept)
        {
            if (dept == null) return;

            using var context = new DataContext();
            bool hasEmployees = await context.Employees.AnyAsync(e => e.DepartmentID == dept.DepartmentID);
            if (hasEmployees)
            {
                MessageBox.Show($"Không thể xóa '{dept.DepartmentName}' vì đang có nhân viên!", "Ràng buộc dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Bạn chắc chắn muốn xóa phòng '{dept.DepartmentName}'?\nTất cả chức vụ trong phòng này cũng sẽ bị xóa!", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var positionsToDelete = context.Positions.Where(p => p.DepartmentID == dept.DepartmentID);
                    context.Positions.RemoveRange(positionsToDelete);

                    var dbDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentID == dept.DepartmentID);
                    if (dbDept != null) context.Departments.Remove(dbDept);

                    // [ĐÃ SỬA LỖI CS0023 TẠI ĐÂY]
                    string adminID = UserSession.CurrentEmployeeId == 0 ? "ADMIN" : UserSession.CurrentEmployeeId.ToString();

                    context.ChangeHistories.Add(new ChangeHistory
                    {
                        LogID = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                        ActionType = "DELETE",
                        TableName = "Departments",
                        RecordID = dept.DepartmentID,
                        ChangeByUserID = adminID,
                        ChangeTime = DateTime.Now,
                        Details = $"Xóa phòng ban: {dept.DepartmentName}"
                    });

                    await context.SaveChangesAsync();

                    Departments.Remove(dept);
                    if (Departments.Count > 0) SelectedDepartment = Departments[0];
                    else Positions.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa:\n" + GetDeepErrorMessage(ex));
                }
            }
        }

        // --- CRUD CHỨC VỤ ---
        [RelayCommand]
        private async Task AddPositionAsync()
        {
            if (SelectedDepartment == null)
            {
                MessageBox.Show("Vui lòng chọn một phòng ban trước!", "Chưa chọn phòng ban", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var addWindow = new AddPositionWindow();
            if (addWindow.ShowDialog() == true)
            {
                using var context = new DataContext();
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    string newID = GenerateID(context, "Position");

                    var newPos = new Position
                    {
                        PositionID = newID,
                        PositionName = addWindow.PosTitle,
                        JobDescription = addWindow.JobDescription,
                        DepartmentID = SelectedDepartment.DepartmentID // Gán FK
                    };

                    context.Positions.Add(newPos);

                    // [ĐÃ SỬA LỖI CS0023 TẠI ĐÂY]
                    string adminID = UserSession.CurrentEmployeeId == 0 ? "ADMIN" : UserSession.CurrentEmployeeId.ToString();

                    context.ChangeHistories.Add(new ChangeHistory
                    {
                        LogID = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                        ActionType = "CREATE",
                        TableName = "Positions",
                        RecordID = newPos.PositionID,
                        ChangeByUserID = adminID,
                        ChangeTime = DateTime.Now,
                        Details = $"Thêm chức vụ: {newPos.PositionName} vào {SelectedDepartment.DepartmentName}"
                    });

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Positions.Add(newPos);
                    MessageBox.Show($"Đã thêm chức vụ '{newPos.PositionName}' thành công!");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    MessageBox.Show("Lỗi thêm chức vụ:\n" + GetDeepErrorMessage(ex), "Lỗi");
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
                try
                {
                    var dbPos = await context.Positions.FirstOrDefaultAsync(p => p.PositionID == pos.PositionID);
                    if (dbPos == null) return;

                    dbPos.PositionName = editWindow.PosTitle;
                    dbPos.JobDescription = editWindow.JobDescription;

                    // [ĐÃ SỬA LỖI CS0023 TẠI ĐÂY]
                    string adminID = UserSession.CurrentEmployeeId == 0 ? "ADMIN" : UserSession.CurrentEmployeeId.ToString();

                    context.ChangeHistories.Add(new ChangeHistory
                    {
                        LogID = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
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
                    if (index != -1) Positions[index] = pos;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi cập nhật chức vụ:\n" + GetDeepErrorMessage(ex), "Lỗi");
                }
            }
        }

        [RelayCommand]
        private async Task DeletePositionAsync(Position pos)
        {
            if (pos == null) return;

            using var context = new DataContext();
            bool hasEmp = await context.Employees.AnyAsync(e => e.PositionID == pos.PositionID);

            if (hasEmp)
            {
                MessageBox.Show($"Không thể xóa chức vụ '{pos.PositionName}' vì đang có nhân viên nắm giữ!", "Ràng buộc dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Xóa chức vụ '{pos.PositionName}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var dbPos = await context.Positions.FirstOrDefaultAsync(p => p.PositionID == pos.PositionID);
                    if (dbPos != null)
                    {
                        context.Positions.Remove(dbPos);

                        // [ĐÃ SỬA LỖI CS0023 TẠI ĐÂY]
                        string adminID = UserSession.CurrentEmployeeId == 0 ? "ADMIN" : UserSession.CurrentEmployeeId.ToString();

                        context.ChangeHistories.Add(new ChangeHistory
                        {
                            LogID = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
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
                    MessageBox.Show("Lỗi xóa chức vụ:\n" + GetDeepErrorMessage(ex), "Lỗi");
                }
            }
        }
    }
}