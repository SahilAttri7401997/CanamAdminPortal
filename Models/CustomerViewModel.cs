using Newtonsoft.Json;

namespace CanamDistributors.Models
{
    public class CustomerViewModel
    {
        public string Id { get; set; }
        public string SyncToken { get; set; }
        public string? GivenName { get; set; }
        public string? FullyQualifiedName { get; set; }
        public string? CompanyName { get; set; }
        public string? DisplayName { get; set; }
        public string? PrintOnCheckName { get; set; }
        public bool Active { get; set; }
        public string? MobileNumber { get; set; }
        public string? PrimaryEmailAddr { get; set; }
        public bool Taxable { get; set; }
        public string? BillAddrId { get; set; }
        public string? BillAddrLine1 { get; set; }
        public string? ShipAddrId { get; set; }
        public string? ShipAddrLine1 { get; set; }
        public bool Job { get; set; }
        public bool BillWithParent { get; set; }
        public decimal Balance { get; set; }
        public decimal BalanceWithJobs { get; set; }
        public string? CurrencyRefName { get; set; }
        public string? CurrencyRefText { get; set; }
        public string? PreferredDeliveryMethod { get; set; }
        public string? _domain { get; set; }
        public bool _sparse { get; set; }
        public string? CreationDate { get; set; }
        public string? LastUpdatedDate { get; set; }
    }

    public class MobileJson
    {
        [JsonProperty("FreeFormNumber")]
        public string? FreeFormNumber { get; set; }
    }

    public class PrimaryEmailAddrJson
    {
        [JsonProperty("Address")]
        public string? Address { get; set; }
    }

    public class AddressJson
    {
        [JsonProperty("Id")]
        public string? Id { get; set; }

        [JsonProperty("Line1")]
        public string? Line1 { get; set; }
    }

    public class MetaDataJson
    {
        [JsonProperty("CreateTime")]
        public string? CreateTime { get; set; }

        [JsonProperty("LastUpdatedTime")]
        public string? LastUpdatedTime { get; set; }
    }
}
