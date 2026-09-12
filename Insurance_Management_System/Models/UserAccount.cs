using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Insurance_Management_System.Models
{
    [Table("UserAccount")]
    public class UserAccount
    {
        [Key]
        [Column("userId")]
        public int userId { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50,MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Username can only contain letters and underscores.")]
        [Column("username")]
        public string username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail.com$", ErrorMessage = "Invalid email address.")]
        [Column("email")]
        public string email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        [Column("passwordHash")]
        public string passwordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [Column("role")]
        public string role { get; set; } = string.Empty;

        [Required, StringLength(20)]
        [Column("status")]
        public string status { get; set; } = "Active";

        [Column("lastLogin")]
        public DateTime? lastLogin { get; set; } = DateTime.UtcNow;

        [Column("createdAt")]
        public DateTime createdAt { get; set; } = DateTime.UtcNow;

    }
}
