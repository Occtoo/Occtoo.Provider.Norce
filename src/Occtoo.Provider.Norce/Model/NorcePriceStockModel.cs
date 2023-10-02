using System.Text.Json.Serialization;

namespace Occtoo.Provider.Norce.Model
{
    public class NorcePriceStockModel
    {
        [JsonPropertyName("partNo")]
        public string PartNo { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("prices")]
        public Price[] Prices { get; set; }
        [JsonPropertyName("onHands")]
        public OnhandDelta[] OnHands { get; set; }
        [JsonPropertyName("suppliers")]
        public object[] Suppliers { get; set; }
        [JsonPropertyName("primaryCategory")]
        public object PrimaryCategory { get; set; }
        [JsonPropertyName("viewCount")]
        public int ViewCount { get; set; }
    }

    public class OnhandDelta : IOnHand
    {
        [JsonPropertyName("warehouse")]
        public Warehouse Warehouse { get; set; }
        [JsonPropertyName("warehouseType")]
        public string WarehouseType { get; set; }
        [JsonPropertyName("value")]
        public float Value { get; set; }
        [JsonPropertyName("leadTimeDayCount")]
        public int? LeadTimeDayCount { get; set; }
        [JsonPropertyName("availableOnStores")]
        public string[] AvailableOnStores { get; set; }
        [JsonPropertyName("code")]
        public string[] AvailableOnPriceLists { get; set; }
        [JsonPropertyName("code")]
        public object NextDelivery { get; set; }
    }
}
