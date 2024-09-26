namespace CanamDistributors.Entity
{
    public class Products
    {
        public string? Id { get; set; }
        public string SyncToken { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Active { get; set; }
        public string? SubItem { get; set; }
        public string? ParentRefName { get; set; }
        public string? ParentRefText { get; set; }
        public string? Level { get; set; }
        public string? FullyQualifiedName { get; set; }
        public string? Taxable { get; set; }
        public string? SalesTaxIncluded { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Type { get; set; }
        public string? IncomeAccountRefName { get; set; }
        public string? IncomeAccountRefText { get; set; }
        public string? PurchaseTaxIncluded { get; set; }
        public decimal? PurchaseCost { get; set; }
        public string? ExpenseAccountRefName { get; set; }
        public string? ExpenseAccountRefText { get; set; }
        public string? AssetAccountRefName { get; set; }
        public string? AssetAccountRefText { get; set; }
        public string? TrackQtyOnHand { get; set; }
        public decimal? QtyOnHand { get; set; }
        public decimal? ReorderPoint { get; set; }
        public string? SalesTaxCodeRefName { get; set; }
        public string? SalesTaxCodeRefText { get; set; }
        public DateTime? InvStartDate { get; set; }
        public string? _domain { get; set; }
        public string? _sparse { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public bool IsAdminAdded { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string? Images { get; set; }
    }
}
