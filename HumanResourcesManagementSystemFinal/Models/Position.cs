using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("Positions")]
public class Position
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    public string? JobDescription { get; set; }
    public int? DepartmentId { get; set; }
    public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
}