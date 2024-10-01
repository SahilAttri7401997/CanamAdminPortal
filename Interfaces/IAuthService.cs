using CanamDistributors.Entity;
using CanamDistributors.Models;

namespace CanamDistributors.Interfaces
{
    public interface IAuthService
    {
        Task<AdminEntity> ValidateUserAsync(LogInViewModel model);
        Task<List<ProductResponseModel>> GetProducts();
        Task<List<CategoryEntity>> GetCollections();
        string GenerateCsv(IEnumerable<ProductResponseModel> products);
        Task<CategoryEntity> SaveCollection(CollectionRequestModel requestModel);
        Task<Products> UpdateHotDealProduct(string productId, decimal discountPrice, string active, List<IFormFile> CategoryImages,string description);
        Task<List<Customer>> GetCustomers();
        string GenerateCustomerCsv(IEnumerable<Customer> customers);
    }
}
