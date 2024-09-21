using Newtonsoft.Json;
using System.Xml.Serialization;

public class QuickBookResponseModel
{
    #region TokenResponse

    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("x_refresh_token_expires_in")]
        public int XRefreshTokenExpiresIn { get; set; }
    }

    #endregion

    [XmlRoot("IntuitResponse", Namespace = "http://schema.intuit.com/finance/v3")]
    public class IntuitResponse
    {
        [XmlElement("QueryResponse")]
        public QueryResponse QueryResponse { get; set; }

        [XmlAttribute("xmlns")]
        public string XmlNamespace { get; set; }

        [XmlAttribute("time")]
        public string Time { get; set; }
    }

    public class QueryResponse
    {
        [XmlElement("Item")]
        public List<InventoryItem> Items { get; set; }

        [XmlAttribute("startPosition")]
        public string StartPosition { get; set; }

        [XmlAttribute("maxResults")]
        public string MaxResults { get; set; }
    }

    public class InventoryItem
    {
        [XmlElement("Id")]
        public string Id { get; set; }

        [XmlElement("SyncToken")]
        public string SyncToken { get; set; }

        [XmlElement("MetaData")]
        public MetaData MetaData { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("Active")]
        public string Active { get; set; }

        [XmlElement("SubItem")]
        public string SubItem { get; set; }

        [XmlElement("ParentRef")]
        public ParentRef ParentRef { get; set; }

        [XmlElement("Level")]
        public string Level { get; set; }

        [XmlElement("FullyQualifiedName")]
        public string FullyQualifiedName { get; set; }

        [XmlElement("Taxable")]
        public string Taxable { get; set; }

        [XmlElement("SalesTaxIncluded")]
        public string SalesTaxIncluded { get; set; }

        [XmlElement("UnitPrice")]
        public string UnitPrice { get; set; }

        [XmlElement("Type")]
        public string Type { get; set; }

        [XmlElement("IncomeAccountRef")]
        public AccountRef IncomeAccountRef { get; set; }

        [XmlElement("PurchaseTaxIncluded")]
        public string PurchaseTaxIncluded { get; set; }

        [XmlElement("PurchaseCost")]
        public string PurchaseCost { get; set; }

        [XmlElement("ExpenseAccountRef")]
        public AccountRef ExpenseAccountRef { get; set; }

        [XmlElement("AssetAccountRef")]
        public AccountRef AssetAccountRef { get; set; }

        [XmlElement("TrackQtyOnHand")]
        public string TrackQtyOnHand { get; set; }

        [XmlElement("QtyOnHand")]
        public string QtyOnHand { get; set; }

        [XmlElement("ReorderPoint")]
        public string ReorderPoint { get; set; }

        [XmlElement("SalesTaxCodeRef")]
        public AccountRef SalesTaxCodeRef { get; set; }

        [XmlElement("InvStartDate")]
        public string InvStartDate { get; set; }

        [XmlElement("domain")]
        public string Domain { get; set; }

        [XmlElement("sparse")]
        public string Sparse { get; set; }
    }

    public class MetaData
    {
        [XmlElement("CreateTime")]
        public string CreateTime { get; set; }

        [XmlElement("LastUpdatedTime")]
        public string LastUpdatedTime { get; set; }
    }

    public class ParentRef
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    public class AccountRef
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
