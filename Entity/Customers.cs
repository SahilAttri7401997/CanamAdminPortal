using System.ComponentModel.DataAnnotations;

namespace CanamDistributors.Entity
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string LegalName { get; set; }
        public string TradeName { get; set; }
        public string BusinessType { get; set; }
        public string PhoneNumber { get; set; }
        public string TaxID { get; set; }
        public bool IsRegisteredBusiness { get; set; }
        public string PSTNumber { get; set; }
        public string BillingAddress { get; set; }
        public string BillingAddress2 { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingCountry { get; set; }
        public string BillingZip { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingAddress2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingZipcode { get; set; }
        public string BankName { get; set; }
        public string BankContactName { get; set; }
        public string BankAddress { get; set; }
        public string BankPhoneNumber { get; set; }
        public string TransitNo { get; set; }
        public string InstNo { get; set; }
        public string AccountNo { get; set; }
        public string SupplierName { get; set; }
        public string SupplierCity { get; set; }
        public string SupplierPhone { get; set; }
        public bool IsContactAuthorized { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string MobilePhone { get; set; }
        public string EmailAddress { get; set; }
        public string Fax { get; set; }
        public bool IsTwoFactorAuthEnabled { get; set; }
        public string MobileNumber { get; set; }
        public bool IsAuthorizedToViewInvoices { get; set; }
        public string Password { get; set; }
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
