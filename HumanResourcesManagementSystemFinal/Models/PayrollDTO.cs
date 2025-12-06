namespace HumanResourcesManagementSystemFinal.Models
{
    public class PayrollDTO
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public decimal ContractSalary { get; set; }
        public double ActualWorkDays { get; set; } 
        public double TotalHoursWorked { get; set; } 
        public decimal NetSalary { get; set; }
    }
}