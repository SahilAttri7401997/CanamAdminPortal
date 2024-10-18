using System.ComponentModel.DataAnnotations;

namespace CanamDistributors.Entity
{
    public class Suppliers
    {
        [Key]
        public int Id { get; set; }  // Unique identifier for the supplier entry
        public int CustomerId { get; set; }  // Foreign key to reference the customer
        public string SupplierName1 { get; set; }  // Name of the first supplier
        public string SupplierCity1 { get; set; }  // City of the first supplier
        public string SupplierPhone1 { get; set; }  // Phone number of the first supplier
        public string SupplierName2 { get; set; }  // Name of the second supplier
        public string SupplierCity2 { get; set; }  // City of the second supplier
        public string SupplierPhone2 { get; set; }  // Phone number of the second supplier
        public string SupplierName3 { get; set; }  // Name of the third supplier
        public string SupplierCity3 { get; set; }  // City of the third supplier
        public string SupplierPhone3 { get; set; }  // Phone number of the third supplier
    }

}
