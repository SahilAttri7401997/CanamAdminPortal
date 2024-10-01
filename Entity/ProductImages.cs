using System.ComponentModel.DataAnnotations;

namespace CanamDistributors.Entity
{
    public class ProductImages
    {
        [Key]
        public int Id { get; set; }
        public string? ProductId { get; set; }
        public string? Images { get; set; }
        public decimal? DiscountPrice { get; set; }
        public DateTime? DtCreated { get; set; }
        public DateTime? DtUpdated { get; set; }
        public string? IsEnabledForHOTDeal { get; set; }
    }
}
