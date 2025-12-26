using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("TimeSheets")]
public class TimeSheet
{
    [Key]
    [Column(TypeName = "char(5)")]
    [StringLength(5)]
    public string TimeSheetID { get; set; } = string.Empty;

    [Column(TypeName = "char(5)")]
    [StringLength(5)]
    public string? EmployeeID { get; set; }
    [ForeignKey("EmployeeID")]
    public virtual Employee? Employee { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime WorkDate { get; set; }

    public TimeSpan? TimeIn { get; set; }
    public TimeSpan? TimeOut { get; set; }

    public double? ActualHours { get; set; }
}