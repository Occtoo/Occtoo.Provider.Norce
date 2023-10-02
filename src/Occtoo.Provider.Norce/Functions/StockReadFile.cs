using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Occtoo.Provider.Norce.Model;
using Occtoo.Provider.Norce.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Occtoo.Provider.Norce.Functions
{
    public class StockReadFile
    {
        private readonly INorceService _norceService;
        private readonly IBlobService _blobService;
        private readonly ITableService _tableService;
        private readonly ILogService _logService;
        public StockReadFile(INorceService norceService,
                              ITableService tableService,
                              IBlobService blobService,
                              ILogService logService)
        {
            _norceService = norceService;
            _tableService = tableService;
            _blobService = blobService;
            _logService = logService;
        }

        [FunctionName("StockReadFile")]
#if DEBUG
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
#else
        public async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
#endif
        {
            try
            {
                string fileUrl = await GetFileUrl();
                if (!string.IsNullOrEmpty(fileUrl))
                {
                    string json = await DownloadFileContentProduct(fileUrl);
                    List<NorcePriceStockModel> result;
                    _ = TryParseJson(json, out result);
                    if (result != null && result.Any())
                    {
                        await SaveProductDataToBlob(json);
                        await UpdateFileUrl(fileUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessageModel errorMessage = new(ex.Message, ex.StackTrace, true);
                await _logService.LogMessage(errorMessage, "StockProcessing");
            }
        }

        private async Task UpdateFileUrl(string fileUrl)
        {
            string rowKey = "FileUrl";
            DynamicTableEntity entity = new DynamicTableEntity("StockFileUrl", rowKey);

            entity.Properties["Url"] = new EntityProperty(fileUrl);
            entity.Properties["Onboarded"] = new EntityProperty(true);

            await _tableService.AddDynamicTableEntity("FileUrls", entity);

        }

        private async Task SaveProductDataToBlob(string jsonFileContent)
        {
            List<NorcePriceStockModel> norceProductModels = JsonConvert.DeserializeObject<List<NorcePriceStockModel>>(jsonFileContent);
            List<string> productGuids = new List<string>();
            foreach (var norceProduct in norceProductModels)
            {
                string fileName = $"{norceProduct.PartNo}_{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}";

                var json = JsonConvert.SerializeObject(norceProduct);
                await _blobService.UploadJsonBlobAsync(fileName, json, "stockonboard");
                productGuids.Add(fileName);
            }
            foreach (var productGuid in productGuids)
            {
                await _blobService.SendToQueue(productGuid, "delta-stock-queue");
            }

        }

        private async Task<string> GetFileUrl()
        {
            string url = string.Empty;
            url = await _tableService.GetDynamicTableEntityFileUrl("FileUrls", "StockFileUrl", "FileUrl", "Url");
            return url;
        }

        private async Task<string> DownloadFileContentProduct(string fileUrl)
        {
            string data = string.Empty;
            try
            {
                data = await _norceService.GetProductFile(fileUrl);
            }
            catch (Exception ex)
            {
                LogMessageModel errorMessage = new(ex.Message, ex.StackTrace, true);
                await _logService.LogMessage(errorMessage, "StockProcessing");
            }
            return data;
        }

        private bool TryParseJson(string json, out List<NorcePriceStockModel> result)
        {
            try
            {
                if (json == null || string.IsNullOrEmpty(json))
                {
                    result = null;
                    return false;
                }
                else
                {
                    result = JsonConvert.DeserializeObject<List<NorcePriceStockModel>>(json);
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unexpected end when deserializing array"))
                {
                    result = null;
                    return false;
                }
                else
                {
                    LogMessageModel errorMessage = new(ex.Message, ex.StackTrace, true);
                    _ = _logService.LogMessage(errorMessage, "StockProcessing");
                    throw;
                }
            }
        }

    }
}
