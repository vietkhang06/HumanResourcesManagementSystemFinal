using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("WorkContracts")]
public class WorkContract
{
    [Key]
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; } = null!;
    public string ContractType { get; set; } = "Full-time";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; }
}