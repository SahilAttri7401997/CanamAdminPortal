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
            List<CategoryEntity> category = await _context.Category.Where(x => x.IsDeleted == false).AsNoTracking().ToListAsync();
            if (category != null && category.Count > 0)
            {
                return category;
            }
            return null;
        }

        public async Task<List<CategoryEntity>> GetDeletedCollections()
        {
            List<CategoryEntity> category = await _context.Category.Where(x => x.IsDeleted == true).AsNoTracking().ToListAsync();
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

        public async Task<Products> UpdateHotDealProduct(string productId, decimal discountPrice, string active, string IsProductEnabled, List<IFormFile> categoryImages, string description, List<string> currentImages)
        {
            var newBase64Images = new List<string>();

            // Convert uploaded files to Base64
            if (categoryImages != null && categoryImages.Count > 0)
            {
                foreach (var file in categoryImages)
                {
                    if (file != null && file.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            var imageBytes = memoryStream.ToArray();
                            var base64Image = Convert.ToBase64String(imageBytes);
                            newBase64Images.Add(base64Image);
                        }
                    }
                }
            }

            // Trim existing images to remove data:image/png;base64, if present
            var trimmedCurrentImages = currentImages.Select(image =>
                image.StartsWith("data:image/png;base64,") ? image.Substring("data:image/png;base64,".Length) : image).ToList();

            // Concatenate new and existing Base64 images with a separator
            var concatenatedImages = string.Join("|", newBase64Images);
            if (trimmedCurrentImages != null && trimmedCurrentImages.Count > 0)
            {
                concatenatedImages = string.IsNullOrEmpty(concatenatedImages)
                    ? string.Join("|", trimmedCurrentImages)
                    : concatenatedImages + "|" + string.Join("|", trimmedCurrentImages);
            }

            // Fetch the existing product and its images
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == productId);
            var productImages = await _context.ProductImages.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == productId);

            if (product != null)
            {
                // Update product details
                product.Description = description;
                product.Active = IsProductEnabled;
                _context.Update(product);
                await _context.SaveChangesAsync();

                if (productImages != null)
                {
                    // Clear the existing images first
                    productImages.Images = string.Empty;

                    // Update product images
                    if (active == "true")
                    {
                        productImages.DiscountPrice = discountPrice;
                    }
                    // Set new images
                    productImages.Images = concatenatedImages; // Assign new images directly
                    productImages.DtUpdated = DateTime.Now;
                    productImages.IsEnabledForHOTDeal = active;

                    // Update the product images in the database
                    _context.Update(productImages);
                }
                else
                {
                    // Create new product images entry
                    var newProductImageEntity = new ProductImages
                    {
                        DiscountPrice = discountPrice,
                        ProductId = productId,
                        Images = concatenatedImages,
                        DtCreated = DateTime.Now,
                        DtUpdated = DateTime.Now,
                        IsEnabledForHOTDeal = active
                    };
                    _context.Add(newProductImageEntity);
                }

                await _context.SaveChangesAsync();
                return product;
            }

            return null; // Return null if product not found
        }

        public async Task<List<CustomerResponseModel>> GetCustomers()
        {
            var customerData = await (
                from customer in _context.Customer
                join supplier in _context.Suppliers
                on customer.Id equals supplier.CustomerId into customerSupplierGroup
                from supplier in customerSupplierGroup.DefaultIfEmpty()  // Left join with DefaultIfEmpty
                select new CustomerResponseModel
                {
                    LegalName = customer.LegalName,
                    TradeName = customer.TradeName,
                    BusinessType = customer.BusinessType,
                    PhoneNumber = customer.PhoneNumber,
                    TaxID = customer.TaxID,
                    PSTNumber = customer.PSTNumber,
                    BillingAddress = customer.BillingAddress,
                    BillingAddress2 = customer.BillingAddress2,
                    BillingCity = customer.BillingCity,
                    BillingState = customer.BillingState,
                    BillingCountry = customer.BillingCountry,
                    BillingZip = customer.BillingZip,
                    ShippingAddress = customer.ShippingAddress,
                    ShippingAddress2 = customer.ShippingAddress2,
                    ShippingCity = customer.ShippingCity,
                    ShippingState = customer.ShippingState,
                    ShippingCountry = customer.ShippingCountry,
                    ShippingZipcode = customer.ShippingZipcode,
                    BankName = customer.BankName,
                    BankContactName = customer.BankContactName,
                    BankAddress = customer.BankAddress,
                    BankPhoneNumber = customer.BankPhoneNumber,
                    TransitNo = customer.TransitNo,
                    InstNo = customer.InstNo,
                    AccountNo = customer.AccountNo,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Title = customer.Title,
                    MobilePhone = customer.MobilePhone,
                    EmailAddress = customer.EmailAddress,
                    Fax = customer.Fax,
                    MobileNumber = customer.MobileNumber,
                    Password = customer.Password,
                    CCFirstName = customer.CCFirstName,
                    CCLastName = customer.CCLastName,
                    CCNumber = customer.CCNumber,
                    CCExpiryMonth = customer.CCExpiryMonth,
                    CCExpiryYear = customer.CCExpiryYear,
                    CVV = customer.CVV,
                    BusinessStatus = customer.BusinessStatus,
                    PSTCertificate = customer.PSTCertificate,
                    // Supplier details from the Suppliers table, default to null if no match
                    SupplierName1 = supplier.SupplierName1,
                    SupplierCity1 = supplier.SupplierCity1,
                    SupplierPhone1 = supplier.SupplierPhone1,
                    SupplierName2 = supplier.SupplierName2,
                    SupplierCity2 = supplier.SupplierCity2,
                    SupplierPhone2 = supplier.SupplierPhone2,
                    SupplierName3 = supplier.SupplierName3,
                    SupplierCity3 = supplier.SupplierCity3,
                    SupplierPhone3 = supplier.SupplierPhone3
                }
            ).AsNoTracking().ToListAsync();

            return customerData ?? new List<CustomerResponseModel>();
        }


        public string GenerateCustomerCsv(IEnumerable<CustomerResponseModel> customers)
        {
            var csv = new StringBuilder();
            csv.AppendLine("First Name,Last Name,Title,Mobile Phone,Email, Trade Name,Legal Name, Business Type,Tax Id,PST Number");
            foreach (var product in customers)
            {
                csv.AppendLine($"{product.FirstName},{product.LastName},{product.Title},{product.MobilePhone},{product.EmailAddress},{product.TradeName},{product.LegalName},{product.BusinessType},{product.TaxID},{product.PSTNumber}");
            }
            return csv.ToString();
        }

        public async Task<bool> DeleteCollection(int categoryId)
        {
            // Find the category by ID
            var category = await _context.Category.FindAsync(categoryId);

            if (category == null)
            {
                // If the category doesn't exist, return false
                return false;
            }

            category.IsDeleted = true;
            category.DtUpdated = DateTime.Now;
            // Remove the category from the DbSet
            _context.Category.Update(category);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return true to indicate successful deletion
            return true;
        }

        public async Task<bool> UpdateCollectionStatus(bool IsCollectionStatus, int categoryId)
        {
            // Find the category by ID
            var category = await _context.Category.FindAsync(categoryId);

            if (category == null)
            {
                // If the category doesn't exist, return false
                return false;
            }
            category.IsActive = IsCollectionStatus;
            category.DtUpdated = DateTime.Now;
            // Remove the category from the DbSet
            _context.Category.Update(category);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return true to indicate successful deletion
            return true;
        }

        public async Task<CategoryEntity> ResetCollection(int categoryId)
        {
            // Find the category by ID
            var category = await _context.Category.FindAsync(categoryId);

            if (category == null)
            {
                // If the category doesn't exist, return false
                return null;
            }
            category.IsDeleted = false;
            category.DtUpdated = DateTime.Now;
            // Remove the category from the DbSet
            _context.Category.Update(category);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return true to indicate successful deletion
            return category;
        }
    }
}
