namespace Occtoo.Provider.Norce.Model
{
    public class StockModel
    {
        public string ProductPartNo { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseLocationCode { get; set; }
        public string WarehouseType { get; set; }
        public string Value { get; set; }
        public string LeadTimeDayCount { get; set; }
        public string AvailableOnStores { get; set; }
        public string AvailableOnPriceList { get; set; }
        public string NextDelivery { get; set; }
    }
}
