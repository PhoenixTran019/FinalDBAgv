namespace Sahafa.Models
{
    public class Book_BookType
    {
        public string BookID { get; set; }
        public Book Book { get; set; }
        public string BookTypeID { get; set; }
        public BookType BookType { get; set; }

       
    }
}
