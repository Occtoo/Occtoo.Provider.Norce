using CSharpFunctionalExtensions;
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
using MediaFileDto = Occtoo.Onboarding.Sdk.Models.MediaFileDto;

namespace Occtoo.Provider.Norce.Formatter
{
    public interface IMediaFormatter
    {
        Task<MediaProductModel> FormatToMediaModel(List<NorceProductModel> norceProducts);
        Task<MediaProductModel> FormatToMediaModel(NorceProductModel norceProduct);

        List<DynamicEntity> FormatToEntites(List<MediaModel> medias);
    }
    public class MediaFormatter : IMediaFormatter
    {

        private readonly IOcctooService _occtooService;

        public MediaFormatter(IOcctooService occtooService)
        {
            _occtooService = occtooService;
        }
        public List<DynamicEntity> FormatToEntites(List<MediaModel> medias)
        {
            var response = new List<DynamicEntity>();

            foreach (var media in medias)
            {
                var guid = Guid.NewGuid();
                var entity = new DynamicEntity
                {
                    Key = media.Key,
                };

                var properties = typeof(MediaModel).GetProperties();
                foreach (var property in properties)
                {
                    entity.Properties.Add(new DynamicProperty
                    {
                        Id = property.Name,
                        Value = property.GetValue(media)?.ToString()
                    });
                }

                response.Add(entity);
            }

            return response;

        }

        public async Task<MediaProductModel> FormatToMediaModel(NorceProductModel norceProduct)
        {
            return await FormatToMediaModel(new List<NorceProductModel> { norceProduct });
        }

        public async Task<MediaProductModel> FormatToMediaModel(List<NorceProductModel> norceProducts)
        {
            MediaProductModel mediaProductModel = new MediaProductModel();
            List<ProductModel> products = new List<ProductModel>();
            List<MediaModel> medias = new List<MediaModel>();
            var productIds = GetProductIds(norceProducts);
            var token = await GetProductToken();
            var occtooProducts = await _occtooService.GetOcctooProducts(productIds, token);

            foreach (var norceProduct in norceProducts)
            {
                int sortVariantMedia = 1;
                int sortProductMedia = 20;
                List<string> productMedias = new List<string>();
                var primaryImage = norceProduct.PrimaryImage;
                var primaryMedia = await GetMediaUrl(medias, primaryImage);

                if (primaryMedia != null)
                {
                    var mediaModel = new MediaModel
                    {
                        Sort = sortProductMedia,
                        Type = primaryImage.Type,
                        Code = primaryImage.Code,
                        FileCode = primaryImage.FileCode,
                        Key = primaryMedia.Id,
                        FileType = primaryImage.MimeType,
                        Url = primaryMedia.PublicUrl,
                        Thumbnail = $"{primaryMedia.PublicUrl}?impolicy=small"
                    };
                    sortProductMedia++;
                    medias.Add(mediaModel);
                    productMedias.Add(primaryMedia.Id);
                }
                foreach (var f in norceProduct.Files)
                {
                    var media = await GetMediaUrl(medias, f);
                    if (media != null)
                    {
                        var mediaModel = new MediaModel
                        {
                            Sort = sortProductMedia,
                            Type = f.Type,
                            Code = f.Code,
                            FileCode = f.FileCode,
                            Key = media.Id,
                            FileType = f.MimeType,
                            Url = media.PublicUrl,
                            Thumbnail = $"{media.PublicUrl}?impolicy=small"
                        };
                        sortProductMedia++;
                        medias.Add(mediaModel);
                        productMedias.Add(media.Id);
                    }
                }

                foreach (var variant in norceProduct.Variants)
                {
                    ProductModel product = new ProductModel();
                    product.PartNo = variant.PartNo;
                    var occtooProductMedia = occtooProducts.Where(p => p.id == variant.PartNo).FirstOrDefault();
                    List<string> productMediaString = new List<string>();
                    if (occtooProductMedia != null && occtooProductMedia.media != null)
                    {
                        string[] occtooMedia = occtooProductMedia.media.Split('|');
                        foreach (var om in occtooMedia)
                        {
                            productMediaString.Add(om.ToString());
                        }

                    }
                    var variantImage = variant.PrimaryImage;
                    var variantMedia = await GetMediaUrl(medias, variantImage);

                    if (variantMedia != null)
                    {
                        var mediaModel = new MediaModel
                        {
                            Sort = sortVariantMedia,
                            Type = variantImage.Type,
                            Code = variantImage.Code,
                            FileCode = variantImage.FileCode,
                            Key = variantMedia.Id,
                            FileType = variantImage.MimeType,
                            Url = variantMedia.PublicUrl,
                            Thumbnail = $"{variantMedia.PublicUrl}?impolicy=small"
                        };
                        sortVariantMedia++;
                        medias.Add(mediaModel);
                        productMediaString.Add(variantMedia.Id);
                    }
                    foreach (var file in variant.Files)
                    {
                        var media = await GetMediaUrl(medias, file);
                        if (media != null)
                        {
                            var mediaModel = new MediaModel
                            {
                                Sort = sortVariantMedia,
                                Type = file.Type,
                                Code = file.Code,
                                FileCode = file.FileCode,
                                Key = media.Id,
                                FileType = file.MimeType,
                                Url = media.PublicUrl,
                                Thumbnail = $"{media.PublicUrl}?impolicy=small"
                            };
                            sortVariantMedia++;
                            medias.Add(mediaModel);
                            productMediaString.Add(media.Id);
                        }
                    }

                    productMedias.AddRange(productMediaString);
                    HashSet<string> uniqueProductMedias = new HashSet<string>(
                    productMedias.Where(media =>
                     !string.IsNullOrWhiteSpace(media) && media.Length <= 36));

                    product.Media = string.Join("|", uniqueProductMedias);
                    products.Add(product);
                }

            }

            var distinctMedias = medias
          .GroupBy(media => media.Key)
          .Select(group => group.OrderBy(media => media.Sort).First())
          .ToList();
            var sortedMedias = medias.OrderByDescending(media => media.Sort).ToList();
            mediaProductModel.Media = distinctMedias;
            mediaProductModel.Products = products;
            return mediaProductModel;
        }

        private List<string> GetProductIds(List<NorceProductModel> norceProducts)
        {
            List<string> productIds = new List<string>();
            foreach (var norceProduct in norceProducts)
            {
                foreach (var variant in norceProduct.Variants)
                {
                    var productId = variant.PartNo;
                    productIds.Add(productId);
                }
            }
            return productIds;
        }


        private async Task<MediaFileDto> GetMediaUrl(List<MediaModel> medias, File file)
        {
            try
            {
                var url = string.Empty;
                medias = medias.Where(m => !string.IsNullOrEmpty(m.Key)).ToList();
                var fileExists = medias != null && medias.Count() > 0 ? medias.Any(m => m.Key.Equals(file.Key)) : false;
                if (!fileExists && !string.IsNullOrEmpty(file.Url) && file.FileCode != "Video")
                {
                    MediaFileDto media = new();
                    string imageExtension = ImageExtensionHelper.GetImageExtensionFromMime(file.MimeType);
                    try
                    {
                        media = await UploadFileToOcctooMediaService(file.Url, $"{file.Key}{imageExtension}", file.Key);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    return media;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;

        }

        private async Task<MediaFileDto> UploadFileToOcctooMediaService(string url, string filename, string uniqueIdentifier)
        {
            var dataProviderId = Environment.GetEnvironmentVariable("MediaDataProviderId");
            var dataProviderSecret = Environment.GetEnvironmentVariable("MediaDataProviderSecret");
            try
            {
                var token = await GetToken(dataProviderId, dataProviderSecret);
                var onboardingServliceClient = new OnboardingServiceClient(dataProviderId, dataProviderSecret);
                var fileToUpload = new FileUploadFromLink(
                            url,
                            filename,
                uniqueIdentifier);

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

        private async Task<string> GetProductToken()
        {
            string token = string.Empty;
            var entity = await GetTableEntity("OcctooToken", "Token", "OcctooTokenProductDestination");
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
                    var tokenDocument = await _occtooService.GetProductDestinationToken();
                    token = tokenDocument.RootElement.GetProperty("accessToken").GetString();

                    await SaveTokenProduct(token);

                }
            }
            if (string.IsNullOrEmpty(token))
            {
                var tokenDocument = await _occtooService.GetProductDestinationToken();
                token = tokenDocument.RootElement.GetProperty("accessToken").GetString();

                await SaveTokenProduct(token);

            }
            return token;

        }

        private async Task SaveTokenProduct(string token)
        {
            if (token != null)
            {
                DynamicTableEntity entity = new DynamicTableEntity("Token", "OcctooTokenProductDestination");
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
