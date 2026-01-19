using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("WorkContracts")]
public class WorkContract
{
    [Key]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    public string ContractID { get; set; } = string.Empty;

    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    public string? EmployeeID { get; set; }

    [ForeignKey(nameof(EmployeeID))]
    public virtual Employee? Employee { get; set; }

    [StringLength(40)]
    public string? ContractType { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? EndDate { get; set; }

    [Column(TypeName = "money")]
    public decimal? Salary { get; set; }
}
