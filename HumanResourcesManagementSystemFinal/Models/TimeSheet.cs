using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("TimeSheets")]
public class TimeSheet
{
    [Key]
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; } = null!;
    public DateTime Date { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public double HoursWorked { get; set; }
}