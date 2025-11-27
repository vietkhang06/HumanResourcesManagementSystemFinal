using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models;

[Table("Employees")]
public class Employee
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [NotMapped] // Chỉ dùng để hiển thị, không lưu DB
    public string FullName => $"{LastName} {FirstName}";

    public DateTime? DateOfBirth { get; set; }

    [StringLength(10)]
    public string Gender { get; set; } = "Other";

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public DateTime HireDate { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;

    // --- Foreign Keys ---

    public int? PositionId { get; set; }
    [ForeignKey("PositionId")]
    public virtual Position? Position { get; set; }

    public int? DepartmentId { get; set; }
    [ForeignKey("DepartmentId")]
    public virtual Department? Department { get; set; }

    // Self-Referencing: Người quản lý (cũng là nhân viên)
    public int? ManagerId { get; set; }
    [ForeignKey("ManagerId")]
    public virtual Employee? Manager { get; set; }

    // --- Navigation Properties ---
    public virtual Account? Account { get; set; }
    public virtual ICollection<Employee> Subordinates { get; set; } = new HashSet<Employee>();
    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new HashSet<LeaveRequest>();
    public virtual ICollection<TimeSheet> TimeSheets { get; set; } = new HashSet<TimeSheet>();
    public virtual ICollection<WorkContract> WorkContracts { get; set; } = new HashSet<WorkContract>();
}