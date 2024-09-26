using System.ComponentModel.DataAnnotations;

namespace CanamDistributors.Models
{
    public class CollectionRequestModel
    {
        [Required(ErrorMessage = "Category is required.")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Product Conditions is required.")]
        public string ProoductConditions { get; set; }

        [Required(ErrorMessage = "Image is required.")]
        public List<IFormFile> CategoryImages { get; set; } = new List<IFormFile>();
        public List<string> ProductList { get; set; } = new List<string>();
    }
}

