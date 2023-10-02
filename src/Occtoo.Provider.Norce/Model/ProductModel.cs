using System.Collections.Generic;

namespace Occtoo.Provider.Norce.Model
{
    public class ProductModel
    {
        public string ProductCode { get; set; }
        public string ProductManufacturer { get; set; }
        public List<LocalizationModel> ProductName { get; set; }
        public List<LocalizationModel> ProductPrimaryCategory { get; set; }
        public List<LocalizationModel> ProductPrimaryCategories { get; set; }
        public List<LocalizationModel> ProductAdditionalCategory { get; set; }
        public List<LocalizationModel> Families { get; set; }
        public string ProductPrimaryImageThumbnailUrl { get; set; }
        public List<LocalizationModel> ProductDescription { get; set; }
        public List<LocalizationModel> ProductSubheader { get; set; }
        public List<LocalizationModel> ProductSubDescription { get; set; }
        public List<LocalizationModel> ProductSynonyms { get; set; }
        public List<VariantParametrics> ProductParametrics { get; set; }
        public string PartNo { get; set; }
        public string ManufacturerPartNo { get; set; }
        public List<LocalizationModel> VariantName { get; set; }
        public List<LocalizationModel> UniqueUrlName { get; set; }
        public string EanCode { get; set; }
        public string Status { get; set; }
        public string VariantFlags { get; set; }
        public string PrimaryImageThumbnailUrl { get; set; }
        public List<VariantParametrics> VariantDefiningParametrics { get; set; }
        public List<VariantParametrics> VariantAdditionalParametrics { get; set; }
        public List<LocalizationModel> VariantText { get; set; }
        public string VariantLogistics { get; set; }
        public string VariantWidht { get; set; }
        public string VariantHeight { get; set; }
        public string VariantDepth { get; set; }
        public string VariantWeight { get; set; }
        public string CommodityCode { get; set; }
        public string RecommendedQty { get; set; }
        public bool IsRecommendedQtyFixed { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public List<LocalizationModel> ProductFlags { get; set; }
        public string Media { get; set; }
    }

}
