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
        Task<Products> UpdateHotDealProduct(string productId, decimal discountPrice, string active, string IsProductEnabled, List<IFormFile> CategoryImages,string description, List<string> currentImages);
        Task<List<CustomerResponseModel>> GetCustomers();
        string GenerateCustomerCsv(IEnumerable<CustomerResponseModel> customers);
        Task<bool> DeleteCollection(int categoryId);
        Task<bool> UpdateCollectionStatus(bool IsCollectionStatus, int categoryId);
        Task<List<CategoryEntity>> GetDeletedCollections();
        Task<CategoryEntity> ResetCollection(int categoryId);
    }
}
