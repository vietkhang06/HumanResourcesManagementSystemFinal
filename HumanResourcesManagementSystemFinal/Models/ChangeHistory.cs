using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.Models
{
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

        // --- SỬA ĐOẠN NÀY ---
        [Column(TypeName = "char(5)")]
        [StringLength(5)]
        public string? ChangeByUserID { get; set; }

        // Phải là Employee, KHÔNG PHẢI Account
        [ForeignKey("ChangeByUserID")]
        public virtual Employee? ChangeByUser { get; set; }
        // --------------------

        [StringLength(200)]
        public string? Details { get; set; }
    }
}