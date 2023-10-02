using System;

namespace Occtoo.Provider.Norce.Model
{
    public class OcctooProductModel
    {
        public string[] productCareInstructions { get; set; }
        public string[] productMaterialSpecification { get; set; }
        public string id { get; set; }
        public DateTime endDate { get; set; }
        public DateTime startDate { get; set; }
        public bool isRecommendedQuantityFixed { get; set; }
        public object recommendedQuantity { get; set; }
        public object commodityCode { get; set; }
        public object variantWeight { get; set; }
        public object variantDepth { get; set; }
        public object variantHeight { get; set; }
        public object variantWidth { get; set; }
        public string primaryImageThumbnailUrl { get; set; }
        public object primaryImageUrl { get; set; }
        public string variantFlags { get; set; }
        public string status { get; set; }
        public string eanCode { get; set; }
        public string uniqueUrlName { get; set; }
        public string variantName { get; set; }
        public string manufacturerPartNo { get; set; }
        public string partNo { get; set; }
        public object productSynonyms { get; set; }
        public string productSubDescription { get; set; }
        public string productSubHeader { get; set; }
        public string productDescription { get; set; }
        public string productPrimaryImageThumbnailUrl { get; set; }
        public object productPrimaryImageUrl { get; set; }
        public string families { get; set; }
        public string productPrimaryCategory { get; set; }
        public string productName { get; set; }
        public string productManufacturer { get; set; }
        public string productCode { get; set; }
        public string media { get; set; }
        public string definingColor { get; set; }
        public string additionalColor { get; set; }
        public string productSizeSpecification { get; set; }
        public string productManufacturingCountry { get; set; }
        public string productEffect { get; set; }
        public string productBase { get; set; }
        public string productMaxWatt { get; set; }
        public string productOwenSafe { get; set; }
        public string productInductionCompatible { get; set; }
        public string productMicrowaveSafe { get; set; }
        public string productFreezerSafe { get; set; }
        public string productDiameter { get; set; }
        public string productHeight { get; set; }
        public string productLength { get; set; }
        public string productDesigner { get; set; }
        public string productDishwasherSafe { get; set; }
        public string productVolumeCl { get; set; }
        public string productVolumeMl { get; set; }
        public string productMaterialInformation { get; set; }
        public string productCertificateproduct { get; set; }
        public string productUsp3 { get; set; }
        public string productLicenseNo { get; set; }
        public string productClimateFootprint { get; set; }
        public string productUsp1 { get; set; }
        public string productUsp2 { get; set; }
        public object[] medias { get; set; }
        public PriceOcctoo[] prices { get; set; }
    }

    public class PriceOcctoo
    {
        public string productPartNo { get; set; }
        public string valueIncVat { get; set; }
        public bool isActive { get; set; }
        public string unitCost { get; set; }
        public string purchaseCost { get; set; }
        public string availableOnWarehouseCodeLocation { get; set; }
        public string availableOnWarehouseCode { get; set; }
        public string vatRate { get; set; }
        public string original { get; set; }
        public bool isDiscountable { get; set; }
        public string value { get; set; }
        public string currency { get; set; }
        public string priceListCode { get; set; }
        public string salesArea { get; set; }
    }

    public class Facet
    {
        public string propertyId { get; set; }
        public string header { get; set; }
        public Value[] values { get; set; }
    }

    public class Value
    {
        public int count { get; set; }
        public string key { get; set; }
    }

}
