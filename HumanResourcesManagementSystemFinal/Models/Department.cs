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
    [StringLength(40)]
    public string DepartmentName { get; set; } = string.Empty;

    [Column(TypeName = "char(5)")]
    [StringLength(5)]
    public string? ManagerID { get; set; } 

    [ForeignKey("ManagerID")]
    public virtual Employee? Manager { get; set; }

    [StringLength(50)]
    public string? Location { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
}