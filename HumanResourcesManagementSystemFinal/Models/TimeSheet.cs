using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("TimeSheets")]
public class TimeSheet
{
    [Key]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    public string TimeSheetID { get; set; } = string.Empty;

    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    public string? EmployeeID { get; set; }

    [ForeignKey(nameof(EmployeeID))]
    public virtual Employee? Employee { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime WorkDate { get; set; }

    public TimeSpan? TimeIn { get; set; }
    public TimeSpan? TimeOut { get; set; }

    public double? ActualHours { get; set; }
}
