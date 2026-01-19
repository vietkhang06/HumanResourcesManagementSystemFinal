using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("Positions")]
public class Position
{
    [Key]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    public string PositionID { get; set; } = string.Empty;

    [Required]
    [StringLength(40)]
    public string PositionName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? JobDescription { get; set; }

    [Column(TypeName = "varchar(10)")]
    public string DepartmentID { get; set; } = string.Empty;

    [ForeignKey(nameof(DepartmentID))]
    public virtual Department? Department { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
}
