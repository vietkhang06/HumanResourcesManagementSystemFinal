using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("ChangeHistories")]
public class ChangeHistory
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string TableName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ActionType { get; set; } = string.Empty; 

    public int RecordId { get; set; }

    public DateTime ChangeTime { get; set; } = DateTime.Now;

    public int? AccountId { get; set; }
    [ForeignKey("AccountId")]
    public virtual Account? Account { get; set; }

    public string? Details { get; set; }
}