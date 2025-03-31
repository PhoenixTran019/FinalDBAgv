

using System.ComponentModel.DataAnnotations;

namespace Sahafa.Models
{
    public class Authors
    {
        [Key]
        [StringLength(255)]
        public string AuthorID { get; set; }
        public string FullName { get; set; }
        public DateTime? DOB { get; set; }
        public string Bio { get; set; }

        public ICollection<Book> Book { get; set; }
    }
}
