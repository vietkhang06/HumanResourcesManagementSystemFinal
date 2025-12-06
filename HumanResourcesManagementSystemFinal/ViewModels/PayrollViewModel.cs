using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models; 
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using HumanResourcesManagementSystemFinal.Data;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class PayrollViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<PayrollDTO> _payrollList;

        [ObservableProperty]
        private ObservableCollection<int> _months;

        [ObservableProperty]
        private ObservableCollection<int> _years;

        [ObservableProperty]
        private int _selectedMonth;

        [ObservableProperty]
        private int _selectedYear;

        [ObservableProperty]
        private decimal _totalSalaryFund;

        public PayrollViewModel()
        {
            PayrollList = new ObservableCollection<PayrollDTO>();

            // Khởi tạo dữ liệu cho ComboBox
            Months = new ObservableCollection<int>(Enumerable.Range(1, 12));
            Years = new ObservableCollection<int>(Enumerable.Range(2023, 5)); // 2023 -> 2027

            // Mặc định chọn tháng hiện tại
            SelectedMonth = DateTime.Now.Month;
            SelectedYear = DateTime.Now.Year;
        }

        [RelayCommand]
        private void CalculatePayroll()
        {
            try
            {
                // 1. Kết nối DB (Sửa lại DB Context theo tên class của bạn)
                using (var context = new DataContext())
                {
                    var resultList = new ObservableCollection<PayrollDTO>();

                    // 2. Lấy danh sách nhân viên đang hoạt động
                    var employees = context.Employees
                                           .Include(e => e.Department)
                                           .Where(e => e.IsActive)
                                           .ToList();

                    foreach (var emp in employees)
                    {
                        // 3. Lấy Lương cơ bản từ Hợp đồng mới nhất
                        // Giả sử bảng WorkContracts có EmployeeId
                        var contract = context.WorkContracts
                                              .Where(c => c.EmployeeId == emp.Id)
                                              .OrderByDescending(c => c.StartDate)
                                              .FirstOrDefault();

                        decimal baseSalary = contract != null ? contract.Salary : 0;

                        // 4. Đếm số ngày công trong tháng đã chọn từ TimeSheets
                        // Giả sử bảng TimeSheets có: EmployeeId, Date (DateTime), HoursWorked
                        var timesheets = context.TimeSheets
                                                .Where(t => t.EmployeeId == emp.Id
                                                         && t.Date.Month == SelectedMonth
                                                         && t.Date.Year == SelectedYear)
                                                .ToList();

                        // Cách tính: Đếm số bản ghi chấm công (mỗi bản ghi là 1 ngày)
                        double workDays = timesheets.Count;
                        double totalHours = timesheets.Sum(t => t.HoursWorked);

                        // 5. Áp dụng công thức: (Lương / 26) * Ngày công
                        decimal finalSalary = 0;
                        if (baseSalary > 0)
                        {
                            finalSalary = (baseSalary / 26m) * (decimal)workDays;
                        }

                        // 6. Tạo DTO
                        resultList.Add(new PayrollDTO
                        {
                            EmployeeId = emp.Id,
                            FullName = $"{emp.FirstName} {emp.LastName}",
                            DepartmentName = emp.Department?.DepartmentName ?? "N/A",
                            ContractSalary = baseSalary,
                            ActualWorkDays = workDays,
                            TotalHoursWorked = totalHours,
                            NetSalary = Math.Round(finalSalary, 0) // Làm tròn
                        });
                    }

                    // Cập nhật UI
                    PayrollList = resultList;
                    TotalSalaryFund = PayrollList.Sum(x => x.NetSalary);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tính lương: " + ex.Message);
            }
        }
    }
}