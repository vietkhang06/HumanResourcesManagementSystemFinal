using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public class PayrollDisplayItem
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public decimal ContractSalary { get; set; } // Map với cột Salary
        public double TotalHoursWorked { get; set; } // Tổng giờ làm
        public double ActualWorkDays { get; set; }   // Quy đổi ra ngày công (Giờ / 8)
        public decimal NetSalary
        {
            get
            {
                if (ActualWorkDays <= 0) return 0;
                return (ContractSalary / 26m) * (decimal)ActualWorkDays;
            }
        }
    }
}