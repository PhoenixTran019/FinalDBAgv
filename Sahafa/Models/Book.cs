using System.ComponentModel.DataAnnotations.Schema;

namespace Sahafa.Models
{
    public class Book
    {
        public string BookID { get; set; }
        public string Title { get; set; }
        
        public string AuthorID { get; set; }
        public Authors Authors { get; set; }

        public string SupplierID { get; set; }
        public Supplier Supplier { get; set; }

        public int PublicationYear { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int Status { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }



        [NotMapped]
        public string AuthorName => Authors?.FullName;

        [NotMapped]
        public string SupplierName => Supplier?.SupplierName;

        public List<BookType> BookType { get; set; }
    }
}
