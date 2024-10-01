using CanamDistributors.Data;
using CanamDistributors.Entity;
using CanamDistributors.Helper;
using CanamDistributors.Interfaces;
using CanamDistributors.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

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
        public async Task<List<ProductResponseModel>> GetProducts()
        {
            List<ProductResponseModel> productResponses = new List<ProductResponseModel>();

            var productsWithImages = await (from p in _context.Products.AsNoTracking()
                                            join pi in _context.ProductImages
                                            on p.Id equals pi.ProductId into productGroup // Create a group join
                                            from pi in productGroup.DefaultIfEmpty() // Perform left join
                                            select new
                                            {
                                                Product = p,
                                                Image = pi != null ? pi.Images : null, // Check for null
                                                DiscountPrice = pi != null ? pi.DiscountPrice : 0, // Default value if null
                                                IsEnabledForHOTDeal = pi != null ? pi.IsEnabledForHOTDeal : null
                                            })
                                            .ToListAsync();

            if (productsWithImages != null && productsWithImages.Count > 0)
            {
                productResponses = productsWithImages
                    .OrderBy(x => x.Product.Name)
                    .Select(x => new ProductResponseModel
                    {
                        Id = x.Product.Id, // Assuming the Id property exists on Product
                        SyncToken = x.Product.SyncToken, // Assuming SyncToken exists
                        Name = x.Product.Name,
                        Description = x.Product.Description,
                        Active = x.Product.Active,
                        SubItem = x.Product.SubItem,
                        ParentRefName = x.Product.ParentRefName,
                        ParentRefText = x.Product.ParentRefText,
                        Level = x.Product.Level,
                        FullyQualifiedName = x.Product.FullyQualifiedName,
                        Taxable = x.Product.Taxable,
                        SalesTaxIncluded = x.Product.SalesTaxIncluded,
                        UnitPrice = x.Product.UnitPrice,
                        Type = x.Product.Type,
                        IncomeAccountRefName = x.Product.IncomeAccountRefName,
                        IncomeAccountRefText = x.Product.IncomeAccountRefText,
                        PurchaseTaxIncluded = x.Product.PurchaseTaxIncluded,
                        PurchaseCost = x.Product.PurchaseCost,
                        ExpenseAccountRefName = x.Product.ExpenseAccountRefName,
                        ExpenseAccountRefText = x.Product.ExpenseAccountRefText,
                        AssetAccountRefName = x.Product.AssetAccountRefName,
                        AssetAccountRefText = x.Product.AssetAccountRefText,
                        TrackQtyOnHand = x.Product.TrackQtyOnHand,
                        QtyOnHand = x.Product.QtyOnHand,
                        ReorderPoint = x.Product.ReorderPoint,
                        SalesTaxCodeRefName = x.Product.SalesTaxCodeRefName,
                        SalesTaxCodeRefText = x.Product.SalesTaxCodeRefText,
                        InvStartDate = x.Product.InvStartDate,
                        _domain = x.Product._domain,
                        _sparse = x.Product._sparse,
                        CreationDate = x.Product.CreationDate,
                        LastModifiedDate = x.Product.LastModifiedDate,
                        IsAdminAdded = x.Product.IsAdminAdded,
                        DiscountPrice = x.DiscountPrice, // Use the discount price from ProductImages
                        Images = x.Image, // Use the image from ProductImages
                        IsEnabledForHOTDeal = x.IsEnabledForHOTDeal,
                    })
                    .ToList();

                return productResponses;
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

        public string GenerateCsv(IEnumerable<ProductResponseModel> products)
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



        public async Task<Products> UpdateHotDealProduct(string productId, decimal discountPrice, string active, List<IFormFile> CategoryImages, string description)
        {
            var base64Images = new List<string>();
            if (CategoryImages.Count() > 0)
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
            var productImages = await _context.ProductImages.Where(x => x.ProductId == productId).AsNoTracking().FirstOrDefaultAsync();
            if (product != null)
            {
                if (!string.IsNullOrEmpty(description))
                {
                    product.Description = description;
                }
                else
                {
                    product.Description = product.Description;
                }
                _context.Update(product);
                await _context.SaveChangesAsync();

                if (productImages != null)
                {
                    productImages.Id = productImages.Id;
                    productImages.DiscountPrice = discountPrice;
                    productImages.ProductId = productId;
                    if (!string.IsNullOrEmpty(concatenatedImages))
                    {
                        productImages.Images = productImages.Images + concatenatedImages;
                    }
                    else
                    {
                        productImages.Images = productImages.Images;
                    }
                    productImages.DtUpdated = DateTime.Now;
                    productImages.IsEnabledForHOTDeal = active;
                    _context.Update(productImages);
                }
                else
                {
                    var productImageEntity = new ProductImages
                    {
                        DiscountPrice = discountPrice,
                        ProductId = productId,
                        Images = concatenatedImages,
                        DtCreated = DateTime.Now,
                        DtUpdated = DateTime.Now,
                        IsEnabledForHOTDeal = active
                    };
                    _context.Add(productImageEntity);
                }
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

        public string GenerateCustomerCsv(IEnumerable<Customer> customers)
        {
            var csv = new StringBuilder();
            csv.AppendLine("First Name,Last Name,Title,Mobile Phone,Email, Trade Name,Legal Name, Business Type,Tax Id,PST Number");
            foreach (var product in customers)
            {
                csv.AppendLine($"{product.FirstName},{product.LastName},{product.Title},{product.MobilePhone},{product.EmailAddress},{product.TradeName},{product.LegalName},{product.BusinessType},{product.TaxID},{product.PSTNumber}");
            }
            return csv.ToString();
        }
    }
}
