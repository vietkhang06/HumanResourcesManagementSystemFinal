using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class Department_PositionViewModel : ObservableObject
    {
        public ObservableCollection<Department> Departments { get; set; } = new();
        public ObservableCollection<Position> Positions { get; set; } = new();

        // Biến lưu phòng ban đang chọn
        private Department _selectedDepartment;
        public Department SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                if (SetProperty(ref _selectedDepartment, value))
                {
                    // Khi chọn phòng ban khác -> Load lại danh sách vị trí
                    LoadPositions(value?.Id);
                }
            }
        }

        public Department_PositionViewModel()
        {
            LoadDepartments();
        }

        // --- HÀM TẢI DỮ LIỆU PHÒNG BAN (Đã sửa theo Model mới) ---
        private void LoadDepartments()
        {
            // Dữ liệu giả lập (Sau này thay bằng kết nối DB)
            // Lưu ý: Đã đổi Description -> Location, DepartmentID -> Id
            Departments.Add(new Department { Id = 1, DepartmentName = "Phòng IT", Location = "Tầng 3 - Tòa nhà A" });
            Departments.Add(new Department { Id = 2, DepartmentName = "Phòng Nhân Sự", Location = "Tầng 2 - Tòa nhà A" });
            Departments.Add(new Department { Id = 3, DepartmentName = "Phòng Kinh Doanh", Location = "Tầng 1 - Showroom" });
            Departments.Add(new Department { Id = 4, DepartmentName = "Phòng Kho", Location = "Khu B - Nhà máy" });

            // Mặc định chọn phòng ban đầu tiên
            if (Departments.Count > 0) SelectedDepartment = Departments[0];
        }

        // --- HÀM TẢI DỮ LIỆU CHỨC VỤ (Đã sửa theo Model mới) ---
        private void LoadPositions(int? departmentId)
        {
            Positions.Clear();
            if (departmentId == null) return;

            // Lưu ý: Đã đổi PositionID -> Id, PositionName -> Title, Description -> JobDescription

            // Giả lập logic lọc theo phòng ban
            if (departmentId == 1) // Phòng IT
            {
                Positions.Add(new Position { Id = 1, Title = "Backend Developer", JobDescription = "Lập trình C# .NET, SQL Server" });
                Positions.Add(new Position { Id = 2, Title = "Frontend Developer", JobDescription = "Lập trình ReactJS, VueJS" });
                Positions.Add(new Position { Id = 3, Title = "IT Helpdesk", JobDescription = "Hỗ trợ phần cứng, mạng nội bộ" });
            }
            else if (departmentId == 2) // Phòng Nhân Sự
            {
                Positions.Add(new Position { Id = 4, Title = "HR Manager", JobDescription = "Quản lý tuyển dụng và nhân sự" });
                Positions.Add(new Position { Id = 5, Title = "Recruiter", JobDescription = "Chuyên viên tuyển dụng" });
            }
            else if (departmentId == 3) // Phòng Kinh Doanh
            {
                Positions.Add(new Position { Id = 6, Title = "Sales Manager", JobDescription = "Quản lý đội ngũ kinh doanh" });
                Positions.Add(new Position { Id = 7, Title = "Sales Executive", JobDescription = "Tìm kiếm khách hàng, bán hàng" });
            }
            // ... Các phòng khác
        }

        [RelayCommand]
        private void AddDepartment()
        {
            // Logic mở form thêm phòng ban
        }

        [RelayCommand]
        private void AddPosition()
        {
            if (SelectedDepartment == null) return;
            // Logic mở form thêm vị trí
        }
    }
}
