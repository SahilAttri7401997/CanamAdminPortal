using CanamDistributors.Data;
using CanamDistributors.Entity;
using CanamDistributors.Helper;
using CanamDistributors.Interfaces;
using CanamDistributors.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CanamDistributors.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminEntity> ValidateUserAsync(LogInViewModel model)
        {
            var admin = await _context.Admin.AsNoTracking().FirstOrDefaultAsync(x => x.UserName == model.Email);
            if (admin != null && PasswordHelper.VerifyPassword(admin.Password, model.Password) == PasswordVerificationResult.Success)
            {
                return admin;
            }
            return null;
        }
        public async Task<List<Products>> GetProducts()
        {
            List<Products> products = await _context.Products.AsNoTracking().ToListAsync();
            if (products != null && products.Count > 0)
            {
                // Order the products by ParentRefName and convert to a list before returning
                return products.OrderBy(x => x.Name).ToList();
            }
            return null;
        }
        public async Task<List<CategoryEntity>> GetCollections()
        {
            List<CategoryEntity> category = await _context.Category.AsNoTracking().ToListAsync();
            if (category != null && category.Count > 0)
            {
                return category;
            }
            return null;
        }

        public string GenerateCsv(IEnumerable<Products> products)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Category,Name,SKU,Type,Sales Description,Sales Price,Cost,Qty On Hand,ReOrder Point");
            foreach (var product in products)
            {
                csv.AppendLine($"{product.ParentRefName},{product.Name},{product.Name},{product.Type},{product.Description},{product.UnitPrice},{product.PurchaseCost},{product.QtyOnHand},{product.ReorderPoint}");
            }
            return csv.ToString();
        }

        public async Task<CategoryEntity> SaveCollection(CollectionRequestModel requestModel)
        {
            var base64Images = new List<string>();
            foreach (var file in requestModel.CategoryImages)
            {
                if (file != null && file.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        var imageBytes = memoryStream.ToArray();
                        var base64Image = Convert.ToBase64String(imageBytes);
                        base64Images.Add(base64Image);
                    }
                }
            }

            // Concatenate Base64 strings with a separator (e.g., "|")
            var concatenatedImages = string.Join("|", base64Images);
            // Create a new category entity
            var category = new CategoryEntity
            {
                ProoductConditions = requestModel.ProoductConditions,
                Category = requestModel.Category,
                CategoryImg = concatenatedImages, // Use the Base64 string here
                Products = requestModel.ProductList.Count,
                DtCreated = DateTime.Now,
                DtUpdated = DateTime.Now,
                IsActive = true
            };

            // Add the category to the context and save changes
            await _context.Category.AddAsync(category);
            await _context.SaveChangesAsync();

            // Prepare to collect products
            var productEntities = new List<CategoryProducts>();

            foreach (var input in requestModel.ProductList)
            {
                // Validate and split input
                if (!string.IsNullOrWhiteSpace(input))
                {
                    var parts = input.Trim(' ', '[', ']', ' ').Split(',');

                    if (parts.Length == 2)
                    {
                        // Clean and extract ID and Name
                        var id = parts[0].Trim('\'', ' ').ToString();
                        var name = parts[1].Trim('\'', ' ').ToString();

                        // Create the product entity
                        var productEntity = new CategoryProducts
                        {
                            CategoryId = category.CategoryId, // Use the generated CategoryId
                            ProductId = Convert.ToInt32(id),
                            ProductName = name
                        };

                        productEntities.Add(productEntity);
                    }
                }
            }

            // Add all product entities to the context in a single call
            if (productEntities.Any())
            {
                await _context.CategoryProducts.AddRangeAsync(productEntities);
                await _context.SaveChangesAsync();
            }

            return category;
        }



        public async Task<Products> UpdateProduct(string productId, decimal discountPrice, string active, List<IFormFile> CategoryImages, string description)
        {
            var base64Images = new List<string>();
            if(CategoryImages.Count() > 0)
            {
                foreach (var file in CategoryImages)
                {
                    if (file != null && file.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            var imageBytes = memoryStream.ToArray();
                            var base64Image = Convert.ToBase64String(imageBytes);
                            base64Images.Add(base64Image);
                        }
                    }
                }
            }
            // Concatenate Base64 strings with a separator (e.g., "|")
            var concatenatedImages = string.Join("|", base64Images);
            var product = await _context.Products.Where(x => x.Id == productId).AsNoTracking().FirstOrDefaultAsync();
            if (product != null)
            {
                product.DiscountPrice = discountPrice;
                product.Active = active;
                product.Description = description;
                if(concatenatedImages != null)
                {
                    product.Images = concatenatedImages;
                }
                _context.Update(product);
                await _context.SaveChangesAsync();
                return product;
            }
            return null;
        }

		public async Task<List<Customer>> GetCustomers()
		{
			List<Customer> customers = await _context.Customer.AsNoTracking().ToListAsync();
			if (customers != null && customers.Count > 0)
			{
				return customers;
			}
			return null;
		}
	}
}
