namespace Sahafa.Models
{
    public class BookViewModel
    {
        public string BookID { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string SupplierName { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }

        public string Description { get; set; }
        public int PublicationYear { get; set; }

        public int Status { get; set; }

        public List<int> BookTypeIDs { get; set; } = new List<int>();
        public List<string> BookTypes { get; set; } = new List<string>();

        public string AuthorID { get; set; }
        public string SupplierID { get; set; }
    }
}
