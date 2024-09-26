using System.ComponentModel.DataAnnotations;

namespace CanamDistributors.Entity
{
    public class CategoryProducts
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string ProductName { get; set; }

    }
}
