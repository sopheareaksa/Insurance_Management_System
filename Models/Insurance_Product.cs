using Insurance_Management_System.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceManagement.Models
{
    [Table("InsuranceProduct")]
    public class InsuranceProduct
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("inProId")]
        public int InProId { get; set; }

        [Required]
        [Column("companyId")]
        public int CompanyId { get; set; }

        [Required]
        [Column("inTypeId")]
        public int InTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("basePremium", TypeName = "decimal(12, 2)")]
        public decimal BasePremium { get; set; }

        [MaxLength(255)]
        [Column("detail")]
        public string? Detail { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "Active";

        // Navigation Properties
        [ForeignKey(nameof(CompanyId))]
        public virtual Company? Company { get; set; }

        [ForeignKey(nameof(InTypeId))]
        public virtual InsuranceType? InsuranceType { get; set; }
    }
}