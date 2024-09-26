using CanamDistributors.Entity;
using CanamDistributors.Models;

namespace CanamDistributors.Interfaces
{
    public interface IAuthService
    {
        Task<AdminEntity> ValidateUserAsync(LogInViewModel model);
        Task<List<Products>> GetProducts();
        Task<List<CategoryEntity>> GetCollections();
        string GenerateCsv(IEnumerable<Products> products);
        Task<CategoryEntity> SaveCollection(CollectionRequestModel requestModel);
        Task<Products> UpdateProduct(string productId, decimal discountPrice, string active, List<IFormFile> CategoryImages,string description);
		Task<List<Customer>> GetCustomers();
	}
}
