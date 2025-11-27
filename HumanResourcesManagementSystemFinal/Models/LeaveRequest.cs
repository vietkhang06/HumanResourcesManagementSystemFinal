using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("LeaveRequests")]
public class LeaveRequest
{
    [Key]
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; } = null!;

    [Required]
    public string LeaveType { get; set; } = "Annual"; // Phép năm, nghỉ ốm...

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string? Reason { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
}