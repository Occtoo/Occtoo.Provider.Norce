using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Occtoo.Provider.Norce.Common;
using Occtoo.Provider.Norce.Model;
using Occtoo.Provider.Norce.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Occtoo.Provider.Norce.Functions
{
    public class ProductOnboard
    {
        private readonly INorceService _norceService;
        private readonly IOcctooService _occtooService;
        private readonly IBlobService _blobService;
        private readonly ILogService _logService;
        private readonly ITableService _tableService;

        public ProductOnboard(INorceService norceService,
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
        [FunctionName("ProductOnboard")]
        public async Task Run([QueueTrigger("delta-product-queue", Connection = "StorageConnectionString")] string myQueueItem, ILogger log)
        {
            try
            {
                var blobContent = await _blobService.ReadJsonBlobAsync($"{myQueueItem}.json", "productonboard");

                await ParseAndImportFileProduct(blobContent);
            }
            catch (Exception ex)
            {
                LogMessageModel errorMessage = new(ex.Message, ex.StackTrace, true);
                await _logService.LogMessage(errorMessage, "ProductProcessing");
            }
        }
        private async Task ParseAndImportFileProduct(string fileContent)
        {
            var dataSource = Environment.GetEnvironmentVariable("NorceProductDataSource");
            var dataProviderId = Environment.GetEnvironmentVariable("ProductDataProviderId");
            var dataProviderSecret = Environment.GetEnvironmentVariable("ProductDataProviderSecret");
            var productCodes = new List<string>();

            try
            {
                var dynamicEntities = await _norceService.GetProductsEntitiesFullSync(fileContent);
                var count = dynamicEntities.Count();
                var json = JsonConvert.SerializeObject(dynamicEntities);
                foreach (var batch in dynamicEntities.Batch(50))
                {
                    foreach (var entity in batch)
                    {
                        productCodes.Add(entity.Key);
                    }

                    var token = await GetToken(dataProviderId, dataProviderSecret);

                    await _occtooService.ImportEntitiesAsync(batch.ToList(), dataSource, dataProviderId, dataProviderSecret, token);
                }

            }
            catch (Exception ex)
            {
                var json = JsonConvert.SerializeObject(productCodes);
                LogMessageModel errorMessage = new($"Products {json}: {ex.Message}", ex.StackTrace, true);
                await _logService.LogMessage(errorMessage, "ProductProcessing");
            }

        }

        private async Task<string> GetToken(string dataProviderId, string dataProviderSecret)
        {
            string token = string.Empty;
            var entity = await _tableService.GetTableEntity("OcctooToken", "Token", "OcctooTokenProduct");
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
                DynamicTableEntity entity = new DynamicTableEntity("Token", "OcctooTokenProduct");
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
