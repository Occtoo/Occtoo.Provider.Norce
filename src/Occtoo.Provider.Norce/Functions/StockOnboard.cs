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
    public class StockOnboard
    {
        private readonly INorceService _norceService;
        private readonly IOcctooService _occtooService;
        private readonly IBlobService _blobService;
        private readonly ILogService _logService;
        private readonly ITableService _tableService;

        public StockOnboard(INorceService norceService,
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

        [FunctionName("StockOnboard")]
        public async Task Run([QueueTrigger("delta-stock-queue", Connection = "StorageConnectionString")] string myQueueItem, ILogger log)
        {
            try
            {
                var blobContent = await _blobService.ReadJsonBlobAsync($"{myQueueItem}.json", "stockonboard");

                await ParseAndImportFileStock(blobContent);
            }
            catch (Exception ex)
            {
                LogMessageModel errorMessage = new(ex.Message, ex.StackTrace, true);
                await _logService.LogMessage(errorMessage, "StockProcessing");
            }
        }

        private async Task ParseAndImportFileStock(string fileContent)
        {
            var dataSource = Environment.GetEnvironmentVariable("NorceStockDataSource");
            var dataProviderId = Environment.GetEnvironmentVariable("StockDataProviderId");
            var dataProviderSecret = Environment.GetEnvironmentVariable("StockDataProviderSecret");

            try
            {
                var dynamicEntities = _norceService.GetStockDelta(fileContent);
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
                await _logService.LogMessage(errorMessage, "StockProcessing");
            }

        }

        private async Task<string> GetToken(string dataProviderId, string dataProviderSecret)
        {
            string token = string.Empty;
            var entity = await _tableService.GetTableEntity("OcctooToken", "Token", "OcctooTokenStock");
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
                DynamicTableEntity entity = new DynamicTableEntity("Token", "OcctooTokenStock");
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
