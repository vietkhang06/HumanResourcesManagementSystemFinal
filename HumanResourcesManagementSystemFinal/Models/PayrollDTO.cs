using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManagementSystemFinal.Models;

public class PayrollDTO
{
    public string EmployeeID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public double ActualWorkDays { get; set; }
    public decimal ContractSalary { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetSalary { get; set; }
}
