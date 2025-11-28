using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("Accounts")]
public class Account
{
    [Key]
    public int AccountId { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // --- Foreign Keys ---
    public int RoleId { get; set; }
    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; } = null!;

    public int? EmployeeId { get; set; }
    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; } = null!;
}