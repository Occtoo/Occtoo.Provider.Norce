using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Occtoo.Provider.Norce.Common;
using Occtoo.Provider.Norce.Model;
using Occtoo.Provider.Norce.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Occtoo.Provider.Norce.Functions
{
    public class PriceOnboard
    {
        private readonly INorceService _norceService;
        private readonly IOcctooService _occtooService;
        private readonly IBlobService _blobService;
        private readonly ILogService _logService;
        private readonly ITableService _tableService;

        public PriceOnboard(INorceService norceService,
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
        [FunctionName("PriceOnboard")]
        public async Task Run([QueueTrigger("delta-price-queue", Connection = "StorageConnectionString")] string myQueueItem, ILogger log)
        {
            try
            {
                var blobContent = await _blobService.ReadJsonBlobAsync($"{myQueueItem}.json", "priceonboard");

                await ParseAndImportFilePrice(blobContent);
            }
            catch (Exception ex)
            {
                LogMessageModel errorMessage = new(ex.Message, ex.StackTrace, true);
                await _logService.LogMessage(errorMessage, "PriceProcessing");
            }
        }

        private async Task ParseAndImportFilePrice(string fileContent)
        {
            var dataSource = Environment.GetEnvironmentVariable("NorcePriceDataSource");
            var dataProviderId = Environment.GetEnvironmentVariable("PriceDataProviderId");
            var dataProviderSecret = Environment.GetEnvironmentVariable("PriceDataProviderSecret");

            try
            {
                var dynamicEntities = _norceService.GetPriceDelta(fileContent);
                dynamicEntities = dynamicEntities.GroupBy(entity => entity.Key).Select(group => group.First()).ToList();

                var count = dynamicEntities.Count();
                foreach (var batch in dynamicEntities.Batch(50))
                {
                    var token = await GetToken(dataProviderId, dataProviderSecret);

                    await _occtooService.ImportEntitiesAsync(batch.ToList(), dataSource, dataProviderId, dataProviderSecret, token);
                }


            }
            catch (Exception ex)
            {
                LogMessageModel errorMessage = new(ex.Message, ex.StackTrace, true);
                await _logService.LogMessage(errorMessage, "PriceProcessing");
            }

        }

        private async Task<string> GetToken(string dataProviderId, string dataProviderSecret)
        {
            string token = string.Empty;
            var entity = await _tableService.GetTableEntity("OcctooToken", "Token", "OcctooTokenPrice");
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
                DynamicTableEntity entity = new DynamicTableEntity("Token", "OcctooTokenPrice");
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
