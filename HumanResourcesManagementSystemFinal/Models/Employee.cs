using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("Employees")]
public class Employee
{
    [Key]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    public string EmployeeID { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "nvarchar(60)")]
    [StringLength(60)]
    public string FullName { get; set; } = string.Empty;

    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    public string? CCCD { get; set; }

    [Column(TypeName = "date")]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(10)]
    public string Gender { get; set; } = "Other";

    [StringLength(100)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Active";

    [Column(TypeName = "char(10)")]
    [StringLength(10)]
    public string? DepartmentID { get; set; }

    [ForeignKey(nameof(DepartmentID))]
    public virtual Department? Department { get; set; }

    [Column(TypeName = "char(10)")]
    [StringLength(10)]
    public string? PositionID { get; set; }

    [ForeignKey(nameof(PositionID))]
    public virtual Position? Position { get; set; }

    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    public string? ManagerID { get; set; }

    [ForeignKey(nameof(ManagerID))]
    public virtual Employee? Manager { get; set; }

    public virtual Account? Account { get; set; }

    public virtual ICollection<WorkContract> WorkContracts { get; set; } = new HashSet<WorkContract>();

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new HashSet<LeaveRequest>();

    public virtual ICollection<TimeSheet> TimeSheets { get; set; } = new HashSet<TimeSheet>();

    public virtual ICollection<ChangeHistory> ChangeHistories { get; set; } = new HashSet<ChangeHistory>();
}
