using System.Text.Json.Serialization;

namespace Occtoo.Provider.Norce.Model
{
    public class NorceProductModel
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("manufacturer")]
        public Manufacturer Manufacturer { get; set; }
        [JsonPropertyName("names")]
        public NameClass[] Names { get; set; }
        [JsonPropertyName("primaryCategory")]
        public Category PrimaryCategory { get; set; }
        [JsonPropertyName("additionalCategories")]
        public Category[] AdditionalCategories { get; set; }
        [JsonPropertyName("variants")]
        public Variant[] Variants { get; set; }
        [JsonPropertyName("families")]
        public Family[] Families { get; set; }
        [JsonPropertyName("flags")]
        public Flag[] Flags { get; set; }
        [JsonPropertyName("parametrics")]
        public Parametric[] Parametrics { get; set; }
        [JsonPropertyName("primaryImage")]
        public File PrimaryImage { get; set; }
        [JsonPropertyName("files")]
        public File[] Files { get; set; }
        [JsonPropertyName("relations")]
        public object[] Relations { get; set; }
        [JsonPropertyName("texts")]
        public Text[] Texts { get; set; }
        [JsonPropertyName("popularities")]
        public object[] Popularities { get; set; }
    }

    public class Manufacturer
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class Category
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("cultures")]
        public Culture[] Cultures { get; set; }
    }

    public class Culture
    {
        [JsonPropertyName("cultureCode")]
        public string CultureCode { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("fullName")]
        public string FullName { get; set; }
        [JsonPropertyName("synonyms")]
        public string Synonyms { get; set; }
    }

    public class NameClass
    {
        [JsonPropertyName("cultureCode")]
        public string CultureCode { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("uniqueUrlName")]
        public string UniqueUrlName { get; set; }
    }

    public class Variant
    {
        [JsonPropertyName("partNo")]
        public string PartNo { get; set; }
        [JsonPropertyName("manufacturerPartNo")]
        public string ManufacturerPartNo { get; set; }
        [JsonPropertyName("names")]
        public VariantName[] Names { get; set; }
        [JsonPropertyName("eanCode")]
        public string EanCode { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("prices")]
        public Price[] Prices { get; set; }
        [JsonPropertyName("onHands")]
        public Onhand[] OnHands { get; set; }
        [JsonPropertyName("suppliers")]
        public object[] Suppliers { get; set; }
        [JsonPropertyName("flags")]
        public Flag[] Flags { get; set; }
        [JsonPropertyName("primaryImage")]
        public File PrimaryImage { get; set; }
        [JsonPropertyName("files")]
        public File[] Files { get; set; }
        [JsonPropertyName("variantDefiningParametrics")]
        public VariantDefiningParametric[] VariantDefiningParametrics { get; set; }
        [JsonPropertyName("additionalParametrics")]
        public AdditionalParametric[] AdditionalParametrics { get; set; }
        [JsonPropertyName("relations")]
        public object[] Relations { get; set; }
        [JsonPropertyName("texts")]
        public object[] Texts { get; set; }
        [JsonPropertyName("logistics")]
        public Logistics Logistics { get; set; }
        [JsonPropertyName("commodityCode")]
        public string CommodityCode { get; set; }
        [JsonPropertyName("recommendedQty")]
        public string RecommendedQty { get; set; }
        [JsonPropertyName("isRecommendedQtyFixed")]
        public bool IsRecommendedQtyFixed { get; set; }
        [JsonPropertyName("startDate")]
        public string StartDate { get; set; }
        [JsonPropertyName("endDate")]
        public string EndDate { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("popularities")]
        public object[] Popularities { get; set; }
    }

    public class Logistics
    {
        [JsonPropertyName("width")]
        public string Width { get; set; }
        [JsonPropertyName("height")]
        public string Height { get; set; }
        [JsonPropertyName("depth")]
        public string Depth { get; set; }
        [JsonPropertyName("weight")]
        public string Weight { get; set; }
    }

    public class VariantName
    {
        [JsonPropertyName("cultureCode")]
        public string CultureCode { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("uniqueUrlName")]
        public string UniqueUrlName { get; set; }
    }

    public class Price
    {
        [JsonPropertyName("salesArea")]
        public string SalesArea { get; set; }
        [JsonPropertyName("priceListCode")]
        public string PriceListCode { get; set; }
        [JsonPropertyName("currency")]
        public string Currency { get; set; }
        [JsonPropertyName("value")]
        public float Value { get; set; }
        [JsonPropertyName("isDiscountable")]
        public bool IsDiscountable { get; set; }
        [JsonPropertyName("original")]
        public float Original { get; set; }
        [JsonPropertyName("vatRate")]
        public float VatRate { get; set; }
        [JsonPropertyName("availableOnWarehouses")]
        public Availableonwarehous[] AvailableOnWarehouses { get; set; }
        [JsonPropertyName("purchaseCost")]
        public float PurchaseCost { get; set; }
        [JsonPropertyName("unitCost")]
        public float UnitCost { get; set; }
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
        [JsonPropertyName("valueIncVat")]
        public float ValueIncVat { get; set; }
    }

    public class Availableonwarehous
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("locationCode")]
        public string LocationCode { get; set; }
    }

    public interface IOnHand
    {
        Warehouse Warehouse { get; set; }
        string WarehouseType { get; set; }
        float Value { get; set; }
        int? LeadTimeDayCount { get; set; }
        string[] AvailableOnStores { get; set; }
        string[] AvailableOnPriceLists { get; set; }
        object NextDelivery { get; set; }
    }

    public class Onhand : IOnHand
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
        [JsonPropertyName("availableOnPriceLists")]
        public string[] AvailableOnPriceLists { get; set; }
        [JsonPropertyName("nextDelivery")]
        public object NextDelivery { get; set; }
    }

    public class Warehouse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("locationCode")]
        public string LocationCode { get; set; }
    }

    public class Flag
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("cultures")]
        public FlagCulture[] Cultures { get; set; }
    }

    public class FlagCulture
    {
        [JsonPropertyName("cultureCode")]
        public string CultureCode { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("groupName")]
        public string GroupName { get; set; }
    }

    public class File
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("fileCode")]
        public string FileCode { get; set; }
        [JsonPropertyName("key")]
        public string Key { get; set; }
        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public class VariantDefiningParametric
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("sortOrder")]
        public int? SortOrder { get; set; }
        [JsonPropertyName("cultures")]
        public VariantCulture[] Cultures { get; set; }
    }

    public class VariantCulture
    {
        [JsonPropertyName("cultureCode")]
        public string CultureCode { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("groupName")]
        public string GroupName { get; set; }
        [JsonPropertyName("unitOfMeasurement")]
        public string UnitOfMeasurement { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
        [JsonPropertyName("listValue")]
        public ListValue ListValue { get; set; }
        [JsonPropertyName("multipleValues")]
        public object MultipleValues { get; set; }
    }

    public class AdditionalParametric
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("sortOrder")]
        public int SortOrder { get; set; }
        [JsonPropertyName("cultures")]
        public VariantCulture[] Cultures { get; set; }
    }

    public class ListValue
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class Family
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("cultures")]
        public FamilyCulture[] Cultures { get; set; }
    }

    public class FamilyCulture
    {
        [JsonPropertyName("cultureCode")]
        public string CultureCode { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class Parametric
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("sortOrder")]
        public int SortOrder { get; set; }
        [JsonPropertyName("cultures")]
        public VariantCulture[] Cultures { get; set; }
    }

    public class Text
    {
        [JsonPropertyName("cultureCode")]
        public string CultureCode { get; set; }
        [JsonPropertyName("descriptionHeader")]
        public string DescriptionHeader { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("subHeader")]
        public string SubHeader { get; set; }
        [JsonPropertyName("subDescription")]
        public string SubDescription { get; set; }
        [JsonPropertyName("synonyms")]
        public string Synonyms { get; set; }
    }

}
