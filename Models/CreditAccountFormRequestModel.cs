using System.ComponentModel.DataAnnotations;

namespace CanamDistributors.Models
{
    public class CreditAccountFormRequestModel
    {
        [Required(ErrorMessage = "*Legal Name of Business is required")]
        public string LegalName { get; set; }

        [Required(ErrorMessage = "*Trade Name is required")]
        public string TradeName { get; set; }

        [Required(ErrorMessage = "*Business Type is required")]
        public string BusinessType { get; set; }

        [Required(ErrorMessage = "*Phone Number is required")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "*Federal Tax ID is required")]
        public string TaxID { get; set; }
        public bool IsRegisteredBusiness { get; set; }
        //public IFormFile ExemptionTaxCertificate { get; set; }

        [Required(ErrorMessage = "*PST Number is required")]
        public string PSTNumber { get; set; }
        // Billing Address
        [Required(ErrorMessage = "Address is required.")]
        public string BillingAddress { get; set; }

        public string BillingAddress2 { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string BillingCity { get; set; }

        [Required(ErrorMessage = "State is required.")]
        public string BillingState { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        public string BillingCountry { get; set; }

        [Required(ErrorMessage = "ZIP Code is required.")]
        public string BillingZip { get; set; }

        // Shipping Address
        [Required(ErrorMessage = "Address is required.")]
        public string ShippingAddress { get; set; }

        public string ShippingAddress2 { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string ShippingCity { get; set; }

        [Required(ErrorMessage = "State is required.")]
        public string ShippingState { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        public string ShippingCountry { get; set; }

        [Required(ErrorMessage = "ZIP Code is required.")]
        public string ShippingZipcode { get; set; }

        // Business Bank Information
        [Required(ErrorMessage = "Bank Name is required.")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "Bank Contact Name is required.")]
        public string BankContactName { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string BankAddress { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        public string BankPhoneNumber { get; set; }

        [Required(ErrorMessage = "Transit No. is required.")]
        public string TransitNo { get; set; }

        [Required(ErrorMessage = "Inst No. is required.")]
        public string InstNo { get; set; }

        [Required(ErrorMessage = "Account No. is required.")]
        public string AccountNo { get; set; }

        [Required(ErrorMessage = "Supplier Name is required.")]
        public string SupplierName { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string SupplierCity { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        public string SupplierPhone { get; set; }

        public bool IsContactAuthorized { get; set; }
        // Contact Information
        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Mobile Phone is required.")]
        public string MobilePhone { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        public string EmailAddress { get; set; }

        public string Fax { get; set; }

        // 2-Factor Authentication
        public bool IsTwoFactorAuthEnabled { get; set; }
        public string MobileNumber { get; set; }

        // Account Statements and Invoices
        [Required(ErrorMessage = "Authorization to view invoices and accounts payable is required.")]
        public bool IsAuthorizedToViewInvoices { get; set; }

        // Account on www.canamdistributors.com
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        public string ConfirmPassword { get; set; }
        public bool IsAuthorizedForPurchases { get; set; }
        public bool AgreesToTerms { get; set; }
        public bool ReceiveEmails { get; set; }
        public bool Guarantee { get; set; }
        public bool AuthorizationForVerification { get; set; }
        public bool AccuracyConfirmation { get; set; }
        public bool TermsAcknowledgement { get; set; }
    }
}

