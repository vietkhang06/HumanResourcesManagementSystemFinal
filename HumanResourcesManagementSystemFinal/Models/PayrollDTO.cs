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

        public decimal AllowanceAndBonus { get; set; } 
        public decimal TotalIncome { get; set; }       
        public decimal TotalDeduction { get; set; }    
        public decimal NetSalary { get; set; }        

        public byte[] AvatarData { get; set; }
    }
}