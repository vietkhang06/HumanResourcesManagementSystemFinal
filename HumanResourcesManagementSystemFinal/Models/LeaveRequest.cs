using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("LeaveRequests")]
public class LeaveRequest
{
    [Key]
    [Column(TypeName = "char(5)")]
    [StringLength(5)]
    public string RequestID { get; set; } = string.Empty;
    [Column(TypeName = "char(5)")]
    [StringLength(5)]
    public string? EmployeeID { get; set; }
    [ForeignKey("EmployeeID")]
    public virtual Employee? Requester { get; set; }

    [StringLength(40)]
    public string? LeaveType { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? EndDate { get; set; }

    public int? TotalDays { get; set; }

    [StringLength(200)] 
    public string? Reason { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Pending";
    [Column(TypeName = "char(5)")]
    [StringLength(5)]
    public string? ApproverID { get; set; }
    [ForeignKey("ApproverID")]
    public virtual Employee? Approver { get; set; }

    [StringLength(200)]
    public string? ManagerComment { get; set; }
}