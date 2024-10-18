using System.ComponentModel.DataAnnotations;

namespace CanamDistributors.Entity
{
    public class CategoryEntity
    {
        [Key]
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public string CategoryImg { get; set; }
        public int Products { get; set; }
        public DateTime DtCreated { get; set; }
        public DateTime DtUpdated { get; set; }
        public string ProoductConditions { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

    }
}
