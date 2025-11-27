using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("Departments")]
public class Department
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string DepartmentName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Location { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
}