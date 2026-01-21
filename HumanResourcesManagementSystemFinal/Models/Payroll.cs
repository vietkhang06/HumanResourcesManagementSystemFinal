using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models
{
    public class Payroll
    {
        [Key]
        public string PayrollID { get; set; }

        [Required]
        public string EmployeeID { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicSalary { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal Allowance { get; set; }  

        [Column(TypeName = "decimal(18,2)")]
        public decimal Bonus { get; set; }     

        [Column(TypeName = "decimal(18,2)")]
        public decimal Deductions { get; set; }  

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; }  

        public double WorkingDays { get; set; }  

        [ForeignKey("EmployeeID")]
        public virtual Employee Employee { get; set; }
    }
}