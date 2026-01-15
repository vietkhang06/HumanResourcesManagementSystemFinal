using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("ChangeHistory")]
public class ChangeHistory
{
    [Key]
    [Column(TypeName = "char(8)")]
    [StringLength(8)]
    public string LogID { get; set; } = string.Empty;

    [StringLength(40)]
    public string? TableName { get; set; }

    [StringLength(10)]
    public string? RecordID { get; set; }

    [StringLength(20)]
    public string? ActionType { get; set; }

    public DateTime ChangeTime { get; set; } = DateTime.Now;

    [Column(TypeName = "char(5)")]
    [StringLength(5)]
    public string? ChangeByUserID { get; set; }

    [ForeignKey(nameof(ChangeByUserID))]
    public virtual Employee? ChangeByUser { get; set; }

    [StringLength(200)]
    public string? Details { get; set; }
}
