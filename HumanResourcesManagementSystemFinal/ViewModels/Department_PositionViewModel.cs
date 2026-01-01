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

        // --- HÀM SINH MÃ TỰ ĐỘNG (PB001, CV001) ---
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
        // -------------------------------------------

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
            // Since Position does not have DepartmentID, we cannot filter by DepartmentID.
            // Instead, load all positions.
            try
            {
                using var context = new DataContext();
                var list = await context.Positions
                    .AsNoTracking()
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

                    // Sinh mã mới
                    string newID = GenerateID(context, "Department");

                    var newDept = new Department
                    {
                        DepartmentID = newID,
                        DepartmentName = addWindow.DeptName,
                        Location = addWindow.DeptLocation
                    };

                    context.Departments.Add(newDept);
                    
                    // Ghi Log
                    string currentAdminID = UserSession.CurrentEmployeeId.ToString();
                    if (string.IsNullOrEmpty(currentAdminID) || currentAdminID == "0")
                    {
                        currentAdminID = "ADMIN";
                    }
                    // Lưu ý: AuditService cần được cập nhật để nhận string ID, hoặc bạn tự insert vào ChangeHistory tại đây
                    // Giả sử AuditService đã update hoặc ta gọi ChangeHistory trực tiếp:
                    context.ChangeHistories.Add(new ChangeHistory 
                    {
                        LogID = GenerateID(context, "Log"), // Cần đảm bảo hàm GenerateID hỗ trợ Log hoặc bạn tự sinh
                        ActionType = "CREATE",
                        TableName = "Departments",
                        RecordID = newDept.DepartmentID,
                        ChangeByUserID = currentAdminID,
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
                    // Tìm theo DepartmentID
                    var dbDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentID == dept.DepartmentID);
                    if (dbDept == null)
                    {
                        MessageBox.Show("Phòng ban không còn tồn tại.", "Lỗi dữ liệu");
                        return;
                    }

                    dbDept.DepartmentName = editWindow.DeptName;
                    dbDept.Location = editWindow.DeptLocation;

                    // Log
                    string currentAdminID = UserSession.CurrentEmployeeId.ToString();
                    if (string.IsNullOrEmpty(currentAdminID) || currentAdminID == "0")
                    {
                        currentAdminID = "ADMIN";
                    }
                    // Tự insert Log nếu AuditService chưa hỗ trợ string
                    context.ChangeHistories.Add(new ChangeHistory
                    {
                        LogID = Guid.NewGuid().ToString().Substring(0, 5).ToUpper(), // Fallback sinh ID nhanh
                        ActionType = "UPDATE",
                        TableName = "Departments",
                        RecordID = dbDept.DepartmentID,
                        ChangeByUserID = currentAdminID,
                        ChangeTime = DateTime.Now,
                        Details = $"Sửa phòng: {dbDept.DepartmentName}"
                    });

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Cập nhật UI
                    dept.DepartmentName = editWindow.DeptName;
                    dept.Location = editWindow.DeptLocation;
                    
                    // Refresh UI List
                    int index = -1;
                    for(int i=0; i<Departments.Count; i++) 
                        if(Departments[i].DepartmentID == dept.DepartmentID) index = i;
                    
                    if (index >= 0) Departments[index] = dept;
                    SelectedDepartment = dept;
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

            try
            {
                using var context = new DataContext();

                // Kiểm tra ràng buộc khóa ngoại (String ID)
                bool hasEmployees = await context.Employees.AnyAsync(e => e.DepartmentID == dept.DepartmentID);
                // Position trong model mới không có DepartmentID (độc lập), nhưng nếu bạn vẫn giữ quan hệ thì check:
                // bool hasPositions = await context.Positions.AnyAsync(p => p.DepartmentID == dept.DepartmentID); 
                // (Tạm bỏ check Position nếu model mới của bạn Position không nối với Department)

                if (hasEmployees)
                {
                    MessageBox.Show($"Không thể xóa phòng '{dept.DepartmentName}' vì đang có Nhân viên trực thuộc!", "Ràng buộc dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show($"Xóa phòng '{dept.DepartmentName}'?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var dbDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentID == dept.DepartmentID);
                    if (dbDept != null)
                    {
                        string currentAdminID = UserSession.CurrentEmployeeId.ToString();
                        if (string.IsNullOrEmpty(currentAdminID) || currentAdminID == "0")
                        {
                            currentAdminID = "ADMIN";
                        }
                        context.ChangeHistories.Add(new ChangeHistory
                        {
                            LogID = Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                            ActionType = "DELETE",
                            TableName = "Departments",
                            RecordID = dept.DepartmentID,
                            ChangeByUserID = currentAdminID,
                            ChangeTime = DateTime.Now,
                            Details = $"Xóa phòng: {dept.DepartmentName}"
                        });

                        context.Departments.Remove(dbDept);
                        await context.SaveChangesAsync();
                    }

                    Departments.Remove(dept);
                    if (Departments.Count > 0) SelectedDepartment = Departments[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa:\n" + GetDeepErrorMessage(ex), "Lỗi");
            }
        }

        [RelayCommand]
        private async Task AddPositionAsync()
        {
            // Trong Model mới, Position có thể độc lập, nhưng nếu UI bạn bắt chọn Phòng ban thì giữ check này
            /*
            if (SelectedDepartment == null)
            {
                MessageBox.Show("Vui lòng chọn một phòng ban trước!", "Cảnh báo");
                return;
            }
            */

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
                        PositionName = addWindow.PosTitle, // Map từ Title của Window sang PositionName
                        JobDescription = addWindow.JobDescription,
                        // DepartmentID = SelectedDepartment.DepartmentID // Bỏ comment nếu Position có DepartmentID
                    };

                    context.Positions.Add(newPos);
                    
                    string currentAdminID = UserSession.CurrentEmployeeId.ToString();
                    if (string.IsNullOrEmpty(currentAdminID) || currentAdminID == "0")
                    {
                        currentAdminID = "ADMIN";
                    }
                    context.ChangeHistories.Add(new ChangeHistory
                    {
                        LogID = Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                        ActionType = "CREATE",
                        TableName = "Positions",
                        RecordID = newPos.PositionID,
                        ChangeByUserID = currentAdminID,
                        ChangeTime = DateTime.Now,
                        Details = $"Thêm chức vụ: {newPos.PositionName}"
                    });

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Positions.Add(newPos);
                    MessageBox.Show($"Đã thêm chức vụ '{newPos.PositionName}'!");
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
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    var dbPos = await context.Positions.FirstOrDefaultAsync(p => p.PositionID == pos.PositionID);
                    if (dbPos == null) return;

                    dbPos.PositionName = editWindow.PosTitle;
                    dbPos.JobDescription = editWindow.JobDescription;

                    string currentAdminID = UserSession.CurrentEmployeeId.ToString();
                    if (string.IsNullOrEmpty(currentAdminID) || currentAdminID == "0")
                    {
                        currentAdminID = "ADMIN";
                    }
                    context.ChangeHistories.Add(new ChangeHistory
                    {
                        LogID = Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                        ActionType = "UPDATE",
                        TableName = "Positions",
                        RecordID = pos.PositionID,
                        ChangeByUserID = currentAdminID,
                        ChangeTime = DateTime.Now,
                        Details = $"Sửa chức vụ: {dbPos.PositionName}"
                    });

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    pos.PositionName = editWindow.PosTitle;
                    pos.JobDescription = editWindow.JobDescription;
                    
                    // Refresh List
                    int index = -1;
                    for(int i=0; i<Positions.Count; i++) 
                        if(Positions[i].PositionID == pos.PositionID) index = i;
                    if (index >= 0) Positions[index] = pos;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    MessageBox.Show("Lỗi cập nhật:\n" + GetDeepErrorMessage(ex), "Lỗi");
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
                // Check khóa ngoại Employee
                bool hasEmp = await context.Employees.AnyAsync(e => e.PositionID == pos.PositionID);

                if (hasEmp)
                {
                    MessageBox.Show($"Không thể xóa chức vụ '{pos.PositionName}' vì đang có nhân viên nắm giữ!", "Ràng buộc", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show($"Xóa chức vụ '{pos.PositionName}'?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var dbPos = await context.Positions.FirstOrDefaultAsync(p => p.PositionID == pos.PositionID);
                    if (dbPos != null)
                    {
                        context.Positions.Remove(dbPos);
                        
                        string currentAdminID = UserSession.CurrentEmployeeId.ToString();
                        if (string.IsNullOrEmpty(currentAdminID) || currentAdminID == "0")
                        {
                            currentAdminID = "ADMIN";
                        }
                        context.ChangeHistories.Add(new ChangeHistory
                        {
                            LogID = Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                            ActionType = "DELETE",
                            TableName = "Positions",
                            RecordID = pos.PositionID,
                            ChangeByUserID = currentAdminID,
                            ChangeTime = DateTime.Now,
                            Details = $"Xóa chức vụ: {pos.PositionName}"
                        });

                        await context.SaveChangesAsync();
                    }
                    Positions.Remove(pos);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa:\n" + GetDeepErrorMessage(ex), "Lỗi");
            }
        }
    }
}