using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Occtoo.Provider.Norce.Common;
using Occtoo.Provider.Norce.Model;
using Occtoo.Provider.Norce.Services;
using Occtoo.Onboarding.Sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Occtoo.Provider.Norce.Functions
{
    public class MediaOnboard
    {
        private readonly INorceService _norceService;
        private readonly IOcctooService _occtooService;
        private readonly IBlobService _blobService;
        private readonly ILogService _logService;
        private readonly ITableService _tableService;

        public MediaOnboard(INorceService norceService,
                              IOcctooService occtooService,
                              IBlobService blobService,
                              ILogService logService,
                              ITableService tableService)
        {
            _norceService = norceService;
            _occtooService = occtooService;
            _blobService = blobService;
            _logService = logService;
            _tableService = tableService;
        }
        [FunctionName("MediaOnboard")]
        public async Task Run([QueueTrigger("delta-media-queue", Connection = "StorageConnectionString")] string myQueueItem, ILogger log)
        {
            try
            {
                var blobContent = await _blobService.ReadJsonBlobAsync($"{myQueueItem}.json", "mediaonboard");

                await ParseAndImportFileMedia(blobContent);
            }
            catch (Exception ex)
            {
                LogMessageModel errorMessage = new(ex.Message, ex.StackTrace, true);
                await _logService.LogMessage(errorMessage, "MediaProcessing");
            }

        }

        private async Task ParseAndImportFileMedia(string fileContent)
        {
            var dataSource = Environment.GetEnvironmentVariable("NorceMediaDataSource");
            var dataProviderId = Environment.GetEnvironmentVariable("MediaDataProviderId");
            var dataProviderSecret = Environment.GetEnvironmentVariable("MediaDataProviderSecret");

            var productDataSource = Environment.GetEnvironmentVariable("NorceProductDataSource");
            var productDataProviderId = Environment.GetEnvironmentVariable("ProductDataProviderId");
            var productDataProviderSecret = Environment.GetEnvironmentVariable("ProductDataProviderSecret");


            try
            {
                var dynamicEntities = await _norceService.GetMediaEntitiesFullSync(fileContent);
                //separate product from media entities
                var mediaEntities = dynamicEntities.Where(x =>
                                        x.Properties.Count(prop => prop.Value != null && !string.IsNullOrEmpty(prop.Value.ToString())) > 2
                                    );

                mediaEntities = mediaEntities.GroupBy(entity => entity.Key).Select(group => group.First()).ToList();
                var countMedia = mediaEntities.Count();
                var productEntities = dynamicEntities.Where(x =>
                                    x.Properties.Count(prop => prop.Value != null && !string.IsNullOrEmpty(prop.Value.ToString())) == 1
                                );

                productEntities = productEntities.GroupBy(entity => entity.Key).Select(group => group.First()).ToList();

                var countProduct = productEntities.Count();
                foreach (var batch in mediaEntities.Batch(50))
                {
                    var token = await GetToken(dataProviderId, dataProviderSecret, "OcctooTokenMedia");

                    await _occtooService.ImportEntitiesAsync(batch.ToList(), dataSource, dataProviderId, dataProviderSecret, token);
                }

                foreach (var batch in productEntities.Batch(50))
                {
                    await SaveProductsToAzure(batch);
                    var token = await GetToken(productDataProviderId, productDataProviderSecret, "OcctooTokenProduct");
                    await _occtooService.ImportEntitiesAsync(batch.ToList(), productDataSource, productDataProviderId, productDataProviderSecret, token);

                }


            }
            catch (Exception ex)
            {
                LogMessageModel errorMessage = new(ex.Message, ex.StackTrace, true);
                await _logService.LogMessage(errorMessage, "MediaProcessing");
            }
        }

        private async Task SaveProductsToAzure(IEnumerable<DynamicEntity> batch)
        {
            List<DynamicTableEntity> entities = new List<DynamicTableEntity>();

            foreach (var product in batch)
            {
                var entity = new DynamicTableEntity();
                entity.PartitionKey = "Product";
                entity.RowKey = product.Key;
                entity.Properties["Media"] = new EntityProperty(product.Properties[0].Value);

                entities.Add(entity);
            }

            await _tableService.PersistValues("Products", entities);
        }


        private async Task<string> GetToken(string dataProviderId, string dataProviderSecret, string tokenName)
        {
            string token = string.Empty;
            var entity = await _tableService.GetTableEntity("OcctooToken", "Token", tokenName);
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

                    await SaveToken(token, tokenName);

                }
            }
            if (string.IsNullOrEmpty(token))
            {
                var tokenDocument = await _occtooService.GetTokenDocument(dataProviderId, dataProviderSecret);
                token = tokenDocument.RootElement.GetProperty("result").GetProperty("accessToken").GetString();

                await SaveToken(token, tokenName);

            }
            return token;

        }

        private async Task SaveToken(string token, string tokenName)
        {
            if (token != null)
            {
                DynamicTableEntity entity = new DynamicTableEntity("Token", tokenName);
                entity.Properties["OcctooTokenValue"] = new EntityProperty(token);
                await _tableService.AddDynamicTableEntity("OcctooToken", entity);
            }
        }

        private bool IsWithinOneHour(DateTime timeToCheck)
        {
            TimeSpan difference = DateTime.Now - timeToCheck;
            return difference.TotalMinutes >= 0 && difference.TotalMinutes <= 59;
        }

    }
}
