using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Occtoo.Provider.Norce.Common;
using Occtoo.Provider.Norce.Model;
using Occtoo.Provider.Norce.Services;
using Occtoo.Onboarding.Sdk;
using Occtoo.Onboarding.Sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Occtoo.Provider.Norce.Formatter
{
    public interface IProductFormatter
    {
        Task<List<ProductModel>> FormatToProductModel(List<NorceProductModel> norceProducts);
        Task<List<ProductModel>> FormatToProductModelFullSync(NorceProductModel norceProduct);
        List<DynamicEntity> FormatToEntites(List<ProductModel> products);
        List<DynamicEntity> FormatToEntitesWithMedia(List<ProductModel> products);
        List<ProductModel> FormatToProductModelDelete(List<NorceProductModel> norceProductModels);
        List<DynamicEntity> FormatToEntitesDelete(List<ProductModel> products);
    }
    public class ProductFormatter : IProductFormatter
    {
        private readonly IOcctooService _occtooService;

        public ProductFormatter(IOcctooService occtooService)
        {
            _occtooService = occtooService;
        }
        public async Task<List<ProductModel>> FormatToProductModel(List<NorceProductModel> norceProducts)
        {
            List<ProductModel> products = new List<ProductModel>();
            foreach (var norceProduct in norceProducts)
            {
                string imageExtension = ImageExtensionHelper.GetImageExtensionFromMime(norceProduct.PrimaryImage.MimeType);
                MediaFileDto productMedia = new();
                if (!string.IsNullOrEmpty(norceProduct.PrimaryImage.Url))
                {
                    productMedia = await UploadFileToOcctooMediaService(norceProduct.PrimaryImage.Url, $"{norceProduct.PrimaryImage.Key}{imageExtension}", norceProduct.PrimaryImage.Key);
                }
                foreach (var variant in norceProduct.Variants)
                {
                    MediaFileDto variantMedia = new();
                    imageExtension = ImageExtensionHelper.GetImageExtensionFromMime(variant.PrimaryImage.MimeType);

                    if (!string.IsNullOrEmpty(variant.PrimaryImage.Url))
                    {
                        variantMedia = await UploadFileToOcctooMediaService(variant.PrimaryImage.Url, $"{variant.PrimaryImage.Key}{imageExtension}", variant.PrimaryImage.Key);
                    }

                    var productModel = new ProductModel
                    {
                        ProductCode = norceProduct.Code,
                        ProductManufacturer = norceProduct.Manufacturer.Name,
                        ProductName = GetProductNames(norceProduct.Names),
                        ProductPrimaryCategory = GetPrimaryCategory(norceProduct.PrimaryCategory),
                        ProductPrimaryCategories = GetPrimaryCategories(norceProduct.PrimaryCategory),
                        ProductAdditionalCategory = GetProductAdditionalCategory(norceProduct.AdditionalCategories),
                        Families = GetFamilies(norceProduct.Families),
                        ProductPrimaryImageThumbnailUrl = !string.IsNullOrEmpty(productMedia.PublicUrl) ? $"{productMedia.PublicUrl}?impolicy=small" : string.Empty,
                        ProductDescription = GetProductDescription(norceProduct.Texts),
                        ProductSubheader = GetProductSubHeader(norceProduct.Texts),
                        ProductSubDescription = GetProductSubDescription(norceProduct.Texts),
                        ProductSynonyms = GetProductSynonyms(norceProduct.Texts),
                        ProductParametrics = GetProductParametrics(norceProduct.Parametrics),
                        PartNo = variant.PartNo,
                        ManufacturerPartNo = variant.ManufacturerPartNo,
                        VariantName = GetVariantNames(variant.Names),
                        UniqueUrlName = GetVariantUniqueUrlName(variant.Names),
                        EanCode = variant.EanCode,
                        Status = variant.Status,
                        VariantFlags = GetVariantFlags(variant.Flags),
                        PrimaryImageThumbnailUrl = !string.IsNullOrEmpty(variantMedia.PublicUrl) ? $"{variantMedia.PublicUrl}?impolicy=small" : string.Empty,
                        VariantDefiningParametrics = GetVariantDefinfingParametrics(variant.VariantDefiningParametrics),
                        VariantAdditionalParametrics = GetVariantAdditionalParametrics(variant.AdditionalParametrics),
                        VariantText = GetVariantText(variant.Texts),
                        VariantLogistics = null,
                        VariantWidht = variant.Logistics.Width,
                        VariantHeight = variant.Logistics.Height,
                        VariantDepth = variant.Logistics.Depth,
                        VariantWeight = variant.Logistics.Weight,
                        CommodityCode = variant.CommodityCode,
                        RecommendedQty = variant.RecommendedQty,
                        IsRecommendedQtyFixed = variant.IsRecommendedQtyFixed,
                        StartDate = variant.StartDate,
                        EndDate = variant.EndDate,

                    };

                    products.Add(productModel);
                }

            }

            return products;
        }

        public async Task<List<ProductModel>> FormatToProductModelFullSync(NorceProductModel norceProduct)
        {
            List<ProductModel> products = new List<ProductModel>();
            string imageExtension = ImageExtensionHelper.GetImageExtensionFromMime(norceProduct.PrimaryImage.MimeType);
            MediaFileDto productMedia = new();
            if (!string.IsNullOrEmpty(norceProduct.PrimaryImage.Url))
            {
                productMedia = await UploadFileToOcctooMediaService(norceProduct.PrimaryImage.Url, $"{norceProduct.PrimaryImage.Key}{imageExtension}", norceProduct.PrimaryImage.Key);
            }
            foreach (var variant in norceProduct.Variants)
            {
                MediaFileDto variantMedia = new();
                imageExtension = ImageExtensionHelper.GetImageExtensionFromMime(variant.PrimaryImage.MimeType);

                if (!string.IsNullOrEmpty(variant.PrimaryImage.Url))
                {
                    variantMedia = await UploadFileToOcctooMediaService(variant.PrimaryImage.Url, $"{variant.PrimaryImage.Key}{imageExtension}", variant.PrimaryImage.Key);
                }

                var productModel = new ProductModel
                {
                    ProductCode = norceProduct.Code,
                    ProductManufacturer = norceProduct.Manufacturer.Name,
                    ProductName = GetProductNames(norceProduct.Names),
                    ProductPrimaryCategory = GetPrimaryCategory(norceProduct.PrimaryCategory),
                    ProductPrimaryCategories = GetPrimaryCategories(norceProduct.PrimaryCategory),
                    ProductAdditionalCategory = GetProductAdditionalCategory(norceProduct.AdditionalCategories),
                    Families = GetFamilies(norceProduct.Families),
                    ProductPrimaryImageThumbnailUrl = !string.IsNullOrEmpty(productMedia.PublicUrl) ? $"{productMedia.PublicUrl}?impolicy=small" : string.Empty,
                    ProductDescription = GetProductDescription(norceProduct.Texts),
                    ProductSubheader = GetProductSubHeader(norceProduct.Texts),
                    ProductSubDescription = GetProductSubDescription(norceProduct.Texts),
                    ProductSynonyms = GetProductSynonyms(norceProduct.Texts),
                    ProductParametrics = GetProductParametrics(norceProduct.Parametrics),
                    PartNo = variant.PartNo,
                    ManufacturerPartNo = variant.ManufacturerPartNo,
                    VariantName = GetVariantNames(variant.Names),
                    UniqueUrlName = GetVariantUniqueUrlName(variant.Names),
                    EanCode = variant.EanCode,
                    Status = variant.Status,
                    VariantFlags = GetVariantFlags(variant.Flags),
                    PrimaryImageThumbnailUrl = !string.IsNullOrEmpty(variantMedia.PublicUrl) ? $"{variantMedia.PublicUrl}?impolicy=small" : string.Empty,
                    VariantDefiningParametrics = GetVariantDefinfingParametrics(variant.VariantDefiningParametrics),
                    VariantAdditionalParametrics = GetVariantAdditionalParametrics(variant.AdditionalParametrics),
                    VariantText = GetVariantText(variant.Texts),
                    VariantLogistics = null,
                    VariantWidht = variant.Logistics.Width,
                    VariantHeight = variant.Logistics.Height,
                    VariantDepth = variant.Logistics.Depth,
                    VariantWeight = variant.Logistics.Weight,
                    CommodityCode = variant.CommodityCode,
                    RecommendedQty = variant.RecommendedQty,
                    IsRecommendedQtyFixed = variant.IsRecommendedQtyFixed,
                    StartDate = variant.StartDate,
                    EndDate = variant.EndDate,

                };

                products.Add(productModel);

            }

            return products;
        }


        public List<ProductModel> FormatToProductModelDelete(List<NorceProductModel> norceProducts)
        {
            List<ProductModel> products = new List<ProductModel>();
            foreach (var norceProduct in norceProducts)
            {
                foreach (var variant in norceProduct.Variants)
                {
                    var productModel = new ProductModel
                    {
                        PartNo = variant.PartNo,
                    };

                    products.Add(productModel);
                }

            }

            return products;
        }

        public List<DynamicEntity> FormatToEntites(List<ProductModel> products)
        {
            var response = new List<DynamicEntity>();

            foreach (var product in products)
            {
                var key = $"{product.PartNo}";
                var entity = new DynamicEntity
                {
                    Key = key
                };

                var properties = typeof(ProductModel).GetProperties();
                foreach (var property in properties)
                {
                    switch (property.Name)
                    {
                        case "ProductName":
                        case "ProductPrimaryCategory":
                        case "ProductPrimaryCategories":
                        case "ProductAdditionalCategory":
                        case "Families":
                        case "ProductDescription":
                        case "ProductSubheader":
                        case "ProductSubDescription":
                        case "ProductSynonyms":
                        case "VariantName":
                        case "UniqueUrlName":
                        case "VariantText":
                        case "ProductFlags":
                            var values = (List<LocalizationModel>)property.GetValue(product);
                            if (values != null)
                            {
                                foreach (var val in values)
                                {
                                    entity.Properties.Add(new DynamicProperty
                                    {
                                        Id = property.Name,
                                        Value = val.Value,
                                        Language = val.Language
                                    });
                                }
                            }
                            break;
                        case "VariantDefiningParametrics":
                        case "VariantAdditionalParametrics":
                        case "ProductParametrics":
                            var parametrics = (List<VariantParametrics>)property.GetValue(product);
                            if (parametrics != null)
                            {
                                foreach (var param in parametrics)
                                {
                                    foreach (var paramVal in param.Value)
                                    {
                                        entity.Properties.Add(new DynamicProperty
                                        {
                                            Id = param.Name,
                                            Value = paramVal.Value,
                                            Language = paramVal.Language
                                        });
                                    }
                                }
                            }
                            break;
                        case "Media":
                            break;
                        default:
                            entity.Properties.Add(new DynamicProperty
                            {
                                Id = property.Name,
                                Value = property.GetValue(product)?.ToString()
                            });
                            break;
                    }
                }

                response.Add(entity);

            }

            return response;
        }

        public List<DynamicEntity> FormatToEntitesDelete(List<ProductModel> products)
        {
            var response = new List<DynamicEntity>();

            foreach (var product in products)
            {
                var key = $"{product.PartNo}";
                var entity = new DynamicEntity
                {
                    Key = key,
                    Delete = true
                };

                response.Add(entity);
            }

            return response;

        }
        public List<DynamicEntity> FormatToEntitesWithMedia(List<ProductModel> products)
        {
            var response = new List<DynamicEntity>();

            foreach (var product in products)
            {
                var key = $"{product.PartNo}";
                var entity = new DynamicEntity
                {
                    Key = key
                };
                entity.Properties.Add(new DynamicProperty
                {
                    Id = "Media",
                    Value = product.Media
                });

                response.Add(entity);
            }

            return response;
        }

        private List<LocalizationModel> GetVariantText(object[] texts)
        {
            //nothing to map here
            return null;
        }

        private List<VariantParametrics> GetProductParametrics(Parametric[] parametrics)
        {
            List<VariantParametrics> productParametrics = new List<VariantParametrics>();
            foreach (var par in parametrics)
            {
                VariantParametrics param = new VariantParametrics();
                param.Name = $"ProductParametrics{par.Code}";
                param.Value = new();

                foreach (var culture in par.Cultures)
                {
                    string value = string.Empty;

                    if (!string.IsNullOrEmpty(culture.Value))
                    {
                        value = culture.Value;
                    }
                    else if (culture.ListValue != null)
                    {
                        value = culture.ListValue.Name;
                    }
                    else if (culture.MultipleValues != null && culture.MultipleValues.ToString() != "[]")
                    {
                        value = culture.MultipleValues.ToString();
                    }
                    LocalizationModel model = new LocalizationModel
                    {
                        Language = culture.CultureCode,
                        Value = value,
                    };

                    param.Value.Add(model);
                }
                productParametrics.Add(param);

            }
            return productParametrics;

        }

        private List<VariantParametrics> GetVariantAdditionalParametrics(AdditionalParametric[] additionalParametrics)
        {
            List<VariantParametrics> parametrics = new List<VariantParametrics>();
            foreach (var par in additionalParametrics)
            {
                VariantParametrics param = new VariantParametrics();
                param.Name = $"variantAdditionalParametrics{par.Code}";
                param.Value = new();

                foreach (var culture in par.Cultures)
                {
                    string value = string.Empty;

                    if (!string.IsNullOrEmpty(culture.Value))
                    {
                        value = culture.Value;
                    }
                    else if (culture.ListValue != null)
                    {
                        value = culture.ListValue.Name;
                    }
                    else if (culture.MultipleValues != null && culture.MultipleValues.ToString() != "[]")
                    {
                        value = culture.MultipleValues.ToString();
                    }
                    LocalizationModel model = new LocalizationModel
                    {
                        Language = culture.CultureCode,
                        Value = value,
                    };

                    param.Value.Add(model);
                }
                parametrics.Add(param);

            }
            return parametrics;
        }

        private List<VariantParametrics> GetVariantDefinfingParametrics(VariantDefiningParametric[] variantDefiningParametrics)
        {
            List<VariantParametrics> parametrics = new List<VariantParametrics>();
            foreach (var par in variantDefiningParametrics)
            {
                VariantParametrics param = new VariantParametrics();
                param.Name = $"variantDefiningParametrics{par.Code}";
                param.Value = new();

                foreach (var culture in par.Cultures)
                {
                    string value = string.Empty;

                    if (!string.IsNullOrEmpty(culture.Value))
                    {
                        value = culture.Value;
                    }
                    else if (culture.ListValue != null)
                    {
                        value = culture.ListValue.Name;
                    }
                    else if (culture.MultipleValues != null && culture.MultipleValues.ToString() != "[]")
                    {
                        value = culture.MultipleValues.ToString();
                    }
                    LocalizationModel model = new LocalizationModel
                    {
                        Language = culture.CultureCode,
                        Value = value,
                    };

                    param.Value.Add(model);
                }
                parametrics.Add(param);
            }
            return parametrics;
        }

        private string GetVariantFlags(Flag[] flags)
        {
            string flag = string.Empty;

            foreach (var f in flags)
            {
                flag += $"{f.Code}|";
            }

            return flag.TrimEnd('|');
        }

        private List<LocalizationModel> GetVariantUniqueUrlName(VariantName[] names)
        {
            List<LocalizationModel> urlNamesList = new List<LocalizationModel>();
            foreach (var name in names)
            {
                LocalizationModel model = new LocalizationModel
                {
                    Language = name.CultureCode,
                    Value = name.UniqueUrlName
                };
                urlNamesList.Add(model);
            }
            return urlNamesList;
        }

        private List<LocalizationModel> GetVariantNames(VariantName[] names)
        {
            List<LocalizationModel> namesList = new List<LocalizationModel>();
            foreach (var name in names)
            {
                LocalizationModel model = new LocalizationModel
                {
                    Language = name.CultureCode,
                    Value = name.Name
                };
                namesList.Add(model);
            }
            return namesList;
        }

        private List<LocalizationModel> GetProductSynonyms(Text[] texts)
        {
            List<LocalizationModel> synonyms = new List<LocalizationModel>();
            foreach (var text in texts)
            {
                LocalizationModel model = new LocalizationModel
                {
                    Language = text.CultureCode,
                    Value = text.Synonyms
                };
                synonyms.Add(model);
            }
            return synonyms;
        }

        private List<LocalizationModel> GetProductSubDescription(Text[] texts)
        {
            List<LocalizationModel> subDescriptions = new List<LocalizationModel>();
            foreach (var text in texts)
            {
                LocalizationModel model = new LocalizationModel
                {
                    Language = text.CultureCode,
                    Value = text.SubDescription
                };
                subDescriptions.Add(model);
            }
            return subDescriptions;
        }

        private List<LocalizationModel> GetProductSubHeader(Text[] texts)
        {
            List<LocalizationModel> subHeaders = new List<LocalizationModel>();
            foreach (var text in texts)
            {
                LocalizationModel model = new LocalizationModel
                {
                    Language = text.CultureCode,
                    Value = text.SubHeader
                };
                subHeaders.Add(model);
            }
            return subHeaders;
        }

        private List<LocalizationModel> GetProductDescription(Text[] texts)
        {
            List<LocalizationModel> descritptions = new List<LocalizationModel>();
            foreach (var text in texts)
            {
                LocalizationModel model = new LocalizationModel
                {
                    Language = text.CultureCode,
                    Value = text.Description
                };
                descritptions.Add(model);
            }
            return descritptions;
        }

        private List<LocalizationModel> GetFamilies(Family[] families)
        {
            List<LocalizationModel> familiesList = new List<LocalizationModel>();
            foreach (var family in families)
            {
                foreach (var culture in family.Cultures)
                {
                    LocalizationModel model = new LocalizationModel()
                    {
                        Language = culture.CultureCode,
                        Value = culture.Name
                    };

                    familiesList.Add(model);
                }
            }
            return familiesList;
        }

        private List<LocalizationModel> GetProductAdditionalCategory(Category[] additionalCategories)
        {
            List<LocalizationModel> additionalCaterogyList = new List<LocalizationModel>();
            foreach (var category in additionalCategories)
            {
                foreach (var localization in category.Cultures)
                {
                    LocalizationModel model = new LocalizationModel
                    {
                        Language = localization.CultureCode,
                        Value = localization.Name
                    };

                    if (!additionalCaterogyList.Any(x => x.Language == localization.CultureCode))
                    {
                        additionalCaterogyList.Add(model);
                    }
                }
            }
            return additionalCaterogyList;
        }

        private List<LocalizationModel> GetPrimaryCategory(Category primaryCategory)
        {
            List<LocalizationModel> categoryModels = new List<LocalizationModel>();
            foreach (var localization in primaryCategory.Cultures)
            {
                LocalizationModel model = new LocalizationModel
                {
                    Language = localization.CultureCode,
                    Value = localization.Name
                };
                categoryModels.Add(model);
            }

            return categoryModels;
        }

        private List<LocalizationModel> GetPrimaryCategories(Category primaryCategory)
        {
            List<LocalizationModel> categoryModels = new List<LocalizationModel>();
            foreach (var localization in primaryCategory.Cultures)
            {
                var categories = localization.FullName.Replace(" - ", "/");
                LocalizationModel model = new LocalizationModel
                {
                    Language = localization.CultureCode,
                    Value = categories
                };
                categoryModels.Add(model);
            }

            return categoryModels;
        }

        private List<LocalizationModel> GetProductNames(NameClass[] names)
        {
            List<LocalizationModel> nameModels = new List<LocalizationModel>();

            foreach (var name in names)
            {
                LocalizationModel model = new LocalizationModel
                {
                    Language = name.CultureCode,
                    Value = name.Name
                };

                nameModels.Add(model);
            }

            return nameModels;
        }

        private async Task<MediaFileDto> UploadFileToOcctooMediaService(string url, string filename, string uniqueIdentifier)
        {
            var dataProviderId = Environment.GetEnvironmentVariable("MediaDataProviderId");
            var dataProviderSecret = Environment.GetEnvironmentVariable("MediaDataProviderSecret");
            try
            {

                var onboardingServliceClient = new OnboardingServiceClient(dataProviderId, dataProviderSecret);
                var fileToUpload = new FileUploadFromLink(
                            url,
                            filename,
                            uniqueIdentifier);

                var token = await GetToken(dataProviderId, dataProviderSecret);
                var cancellationToken = new CancellationTokenSource(180000).Token; // 3 mins               
                var response = await onboardingServliceClient.UploadFromLinkAsync(fileToUpload, token, cancellationToken);
                if (response.StatusCode == 200)
                {
                    MediaFileDto uploadDto = response.Result;
                    return uploadDto;
                }
                else
                {
                    throw new Exception($"There was a problem uploading media {response.StatusCode}");
                }

            }
            catch (Exception)
            {
                throw;
            }
        }


        private async Task<string> GetToken(string dataProviderId, string dataProviderSecret)
        {
            string token = string.Empty;
            var entity = await GetTableEntity("OcctooToken", "Token", "OcctooTokenMedia");
            if (entity != null && entity.Properties.ContainsKey("OcctooTokenValue"))
            {
                string inputTime = entity.Timestamp.ToString();
                DateTime parsedTime = DateTime.Parse(inputTime);

                if (IsWithinOneHour(parsedTime))
                {
                    token = entity.Properties["OcctooTokenValue"].StringValue;
                }
                else
                {
                    var tokenDocument = await _occtooService.GetTokenDocument(dataProviderId, dataProviderSecret);
                    token = tokenDocument.RootElement.GetProperty("result").GetProperty("accessToken").GetString();

                    await SaveToken(token);

                }
            }
            if (string.IsNullOrEmpty(token))
            {
                var tokenDocument = await _occtooService.GetTokenDocument(dataProviderId, dataProviderSecret);
                token = tokenDocument.RootElement.GetProperty("result").GetProperty("accessToken").GetString();

                await SaveToken(token);

            }
            return token;

        }

        private async Task SaveToken(string token)
        {
            if (token != null)
            {
                DynamicTableEntity entity = new DynamicTableEntity("Token", "OcctooTokenMedia");
                entity.Properties["OcctooTokenValue"] = new EntityProperty(token);
                await AddDynamicTableEntity("OcctooToken", entity);
            }
        }

        private async Task<DynamicTableEntity> GetTableEntity(string tableName, string partitionKey, string rowKey)
        {
            CloudTable table = await GetTableReference(tableName);

            TableOperation retrieveOperation = TableOperation.Retrieve<DynamicTableEntity>(partitionKey, rowKey);

            TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);

            return retrievedResult.Result as DynamicTableEntity;

        }

        private static async Task<CloudTable> GetTableReference(string name)
        {
            var connection = Environment.GetEnvironmentVariable("StorageConnectionString");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connection);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(name);

            await table.CreateIfNotExistsAsync();

            return table;
        }

        private static async Task AddDynamicTableEntity(string tableName, DynamicTableEntity entity)
        {
            var table = await GetTableReference(tableName);

            TableOperation addOperation = TableOperation.InsertOrReplace(entity);

            TableBatchOperation userEnvironmentBatch = new TableBatchOperation();
            userEnvironmentBatch.Add(addOperation);

            await table.ExecuteBatchAsync(userEnvironmentBatch);
        }

        private bool IsWithinOneHour(DateTime timeToCheck)
        {
            TimeSpan difference = DateTime.Now - timeToCheck;
            return difference.TotalMinutes >= 0 && difference.TotalMinutes <= 59;
        }
    }

}

