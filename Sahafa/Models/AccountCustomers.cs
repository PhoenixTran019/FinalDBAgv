using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sahafa.Models
{
    public class AccountCustomers
    {
        [Key]
        [StringLength(255)]
        public string Username { get; set; } = string.Empty; // Primary Key

        [ForeignKey("Customer")]
        public int CustomerID { get; set; } // Foreign Key từ bảng Customers

        [Required]
        [StringLength(50)]
        public string Password { get; set; } = string.Empty; // Không hash theo yêu cầu bài tập

        [Column("registrationdate")]
        public DateTime RegistrationDate { get; set; }

        // Navigation property
        public virtual Customers Customer { get; set; }
    }
}
