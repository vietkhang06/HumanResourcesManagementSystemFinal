using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("Departments")]
public class Department
{
    [Key]
    [Column(TypeName = "char(5)")]
    [StringLength(5)]
    public string DepartmentID { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string DepartmentName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Location { get; set; }

    [StringLength(5)]
    public string? ManagerID { get; set; }

    public virtual ICollection<Position> Positions { get; set; } = new HashSet<Position>();

    public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
}
