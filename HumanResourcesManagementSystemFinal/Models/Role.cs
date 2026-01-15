using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("Roles")]
public class Role
{
    [Key]
    [Column(TypeName = "char(5)")]
    [StringLength(5)]
    public string RoleID { get; set; } = string.Empty;

    [Required]
    [StringLength(40)]
    public string RoleName { get; set; } = string.Empty;

    public virtual ICollection<Account> Accounts { get; set; } = new HashSet<Account>();
}
