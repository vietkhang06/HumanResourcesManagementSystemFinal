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
        public decimal BasicSalary { get; set; } // Lương cơ bản từ hợp đồng

        [Column(TypeName = "decimal(18,2)")]
        public decimal Allowance { get; set; }   // Phụ cấp

        [Column(TypeName = "decimal(18,2)")]
        public decimal Bonus { get; set; }       // Thưởng

        [Column(TypeName = "decimal(18,2)")]
        public decimal Deductions { get; set; }  // Tổng chi (Bảo hiểm, phạt...)

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; }   // Thực lĩnh cuối cùng

        public double WorkingDays { get; set; }  // Số ngày công tính được

        [ForeignKey("EmployeeID")]
        public virtual Employee Employee { get; set; }
    }
}