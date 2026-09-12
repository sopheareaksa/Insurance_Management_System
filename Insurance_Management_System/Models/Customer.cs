using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insurance_Management_System.Models
{
    [Table("Customer")]
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        [Required,StringLength(50)]
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public UserAccount? UserAccount { get; set; }
        [Required(ErrorMessage = "Fullname is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Fullname must be between 3 and 50 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Fullname can only contain letters and underscores.")]
        public string Fullname { get; set; } = string.Empty;
        [Required, StringLength(200)]
        public string Address { get; set; } = string.Empty;
        [Required]
        public DateTime? DateOfBirth { get; set; }
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "National ID must contain numbers only.")]
        [StringLength(20, MinimumLength = 9, ErrorMessage = "National ID must be between 9 and 20 digits.")]
        [Display(Name = "National ID")]
        public string? NationalId { get; set; } = string.Empty;

    }
}
