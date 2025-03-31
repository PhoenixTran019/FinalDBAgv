using System.ComponentModel.DataAnnotations.Schema;

namespace Sahafa.Models
{
    public class Stationery
    {
        public string StationeryID { get; set; }
        public string Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        //Connect
        public string SupplierID { get; set; }
        public Supplier Supplier { get; set; }

        public int? TypeID { get; set; }
        public string TypeName { get; set; }


        public string Category { get; set;}
        public int Status { get; set; }
        public string Image { get; set; }
        public string SupplierName { get; set; }

        
    }
}
