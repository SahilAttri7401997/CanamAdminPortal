using System.ComponentModel.DataAnnotations;

namespace CanamDistributors.Models
{
    public class CollectionRequestModel
    {
        [Required(ErrorMessage = "Category is required.")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Prooduct Conditions is required.")]
        public string ProoductConditions { get; set; }

        [Required(ErrorMessage = "Image is required.")]
        public string CategoryImage { get; set; }
        public int ProductId { get; set; }
    }
}
