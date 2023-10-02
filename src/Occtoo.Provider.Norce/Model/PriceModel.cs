namespace Occtoo.Provider.Norce.Model
{
    public class PriceModel
    {
        public string ProductPartNo { get; set; }
        public string SalesArea { get; set; }
        public string PriceListCode { get; set; }
        public string Currency { get; set; }
        public string Value { get; set; }
        public string IsDiscountable { get; set; }
        public string Original { get; set; }
        public string VatRate { get; set; }
        public string AvailableOnWarehouseCode { get; set; }
        public string AvailableOnWarehouseCodeLocation { get; set; }
        public string PurchaseCost { get; set; }
        public string UnitCost { get; set; }
        public string IsActive { get; set; }
        public string ValueIncVat { get; set; }
    }
}
