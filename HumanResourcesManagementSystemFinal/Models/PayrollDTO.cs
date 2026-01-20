using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HumanResourcesManagementSystemFinal.Models
{
    public partial class PayrollDTO : ObservableObject
    {
        public string EmployeeID { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public double ActualWorkDays { get; set; } 
        public decimal ContractSalary { get; set; }

        public decimal AllowanceAndBonus { get; set; } // Phụ cấp & Thưởng
        public decimal TotalIncome { get; set; }       // Tổng thu nhập (Lương công + Phụ cấp)
        public decimal TotalDeduction { get; set; }    // Tổng chi (Bảo hiểm, phạt...)
        public decimal NetSalary { get; set; }         // Thực lĩnh

        public byte[] AvatarData { get; set; }
    }
}