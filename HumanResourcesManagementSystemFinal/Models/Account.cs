using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("Accounts")]
public class Account
{
    [Key]
    [Column(TypeName = "char(5)")]
    [StringLength(5)]
    public string UserID { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string Password { get; set; } = string.Empty;

    [StringLength(20)]
    public string IsActive { get; set; } = "Active";

    [Column(TypeName = "char(5)")]
    [StringLength(5)]
    public string? EmployeeID { get; set; }

    [ForeignKey(nameof(EmployeeID))]
    public virtual Employee? Employee { get; set; }

    [Column(TypeName = "char(5)")]
    [StringLength(5)]
    public string? RoleID { get; set; }

    [ForeignKey(nameof(RoleID))]
    public virtual Role? Role { get; set; }

    public virtual ICollection<ChangeHistory> ChangeHistories { get; set; } = new HashSet<ChangeHistory>();
}
