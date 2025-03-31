using Microsoft.Identity.Client;

namespace Sahafa.Models
{
    public class Supplier
    {
        public string SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }

        public ICollection<Stationery> Stationery { get; set; }
        public ICollection<Book> Book { get; set; }
    }
}
