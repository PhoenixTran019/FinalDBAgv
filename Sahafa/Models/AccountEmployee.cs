using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sahafa.Models
{
    public class AccountEmployee
    {
        [Key]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        // Khóa ngoại với Employees
        [ForeignKey("Employee")]
        public int EmployeeID { get; set; }

        public virtual Employees Employee { get; set; }
    }
}
