using System.ComponentModel.DataAnnotations;
namespace Insurance_Management_System.Models
{
    public class InsuranceType
    {
        [Key]
        public int InTypeId { get; set; }
        [Required, StringLength(50)]
        public String Name { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public String Description { get; set; } = string.Empty;
    }
}
