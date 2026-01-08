public class PayrollDTO
{
    public string EmployeeID { get; set; } 
    public string FullName { get; set; }
    public string PositionName { get; set; }
    public string DepartmentName { get; set; }
    public double ActualWorkDays { get; set; }
    public decimal ContractSalary { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetSalary { get; set; }
}