using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Insurance_Management_System.Models
{
    [Table("Company")]
    public class Company
    {
        [Key]
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public UserAccount? UserAccount { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required, StringLength(200)]
        public string Address { get; set; } = string.Empty;
        [Required, StringLength(50)]
        public string Phone { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string Email { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string RegNo { get; set; } = string.Empty;
        public bool IsHostCompany { get; set; } = false;
        [Required, StringLength(50)]
        [Column("approvalStatus")]
        public string ApproveStatus { get; set; } = "Pending";
        [Column("commissionRate", TypeName = "decimal(12, 2)")]
        public decimal CommissionRate { get; set; } = 0.0m;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    }
}
