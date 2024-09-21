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
                CategoryEntity category = new CategoryEntity();
                category.ProoductConditions = requestModel.ProoductConditions;
                category.Category = requestModel.Category;
                category.CategoryImg = requestModel.CategoryImage;
                category.Products = requestModel.ProductId;
                category.DtCreated = DateTime.Now;
                category.DtUpdated = DateTime.Now;
                category.IsActive = true;
                await _context.Category.AddAsync(category);
                await _context.SaveChangesAsync();
                return category;


            }

        public async Task<CreditAccountFormRequestModel> SaveCustomer(CreditAccountFormRequestModel model)
        {
            var request = new Customer
            {
                LegalName = model.LegalName,
                TradeName = model.TradeName,
                BusinessType = model.BusinessType,
                PhoneNumber = model.PhoneNumber,
                TaxID = model.TaxID,
                IsRegisteredBusiness = model.IsRegisteredBusiness,
                PSTNumber = model.PSTNumber,
                BillingAddress = model.BillingAddress,
                BillingAddress2 = model.BillingAddress2,
                BillingCity = model.BillingCity,
                BillingState = model.BillingState,
                BillingCountry = model.BillingCountry,
                BillingZip = model.BillingZip,
                ShippingAddress = model.ShippingAddress,
                ShippingAddress2 = model.ShippingAddress2,
                ShippingCity = model.ShippingCity,
                ShippingState = model.ShippingState,
                ShippingCountry = model.ShippingCountry,
                ShippingZipcode = model.ShippingZipcode,
                BankName = model.BankName,
                BankContactName = model.BankContactName,
                BankAddress = model.BankAddress,
                BankPhoneNumber = model.BankPhoneNumber,
                TransitNo = model.TransitNo,
                InstNo = model.InstNo,
                AccountNo = model.AccountNo,
                SupplierName = model.SupplierName,
                SupplierCity = model.SupplierCity,
                SupplierPhone = model.SupplierPhone,
                IsContactAuthorized = model.IsContactAuthorized,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Title = model.Title,
                MobilePhone = model.MobilePhone,
                EmailAddress = model.EmailAddress,
                Fax = model.Fax,
                IsTwoFactorAuthEnabled = model.IsTwoFactorAuthEnabled,
                MobileNumber = model.MobileNumber,
                IsAuthorizedToViewInvoices = model.IsAuthorizedToViewInvoices,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
                IsAuthorizedForPurchases = model.IsAuthorizedForPurchases,
                AgreesToTerms = model.AgreesToTerms,
                ReceiveEmails = model.ReceiveEmails,
                Guarantee = model.Guarantee,
                AuthorizationForVerification = model.AuthorizationForVerification,
                AccuracyConfirmation = model.AccuracyConfirmation,
                TermsAcknowledgement = model.TermsAcknowledgement
            };
            _context.Add(request);
            await _context.SaveChangesAsync();
            return model;
        }
    }
}
