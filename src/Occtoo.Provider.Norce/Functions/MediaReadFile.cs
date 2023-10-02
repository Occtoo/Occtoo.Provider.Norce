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
    public class MediaReadFile
    {
        private readonly INorceService _norceService;
        private readonly IBlobService _blobService;
        private readonly ITableService _tableService;
        private readonly ILogService _logService;
        public MediaReadFile(INorceService norceService,
                              ITableService tableService,
                              IBlobService blobService,
                              ILogService logService)
        {
            _norceService = norceService;
            _tableService = tableService;
            _blobService = blobService;
            _logService = logService;
        }
        [FunctionName("MediaReadFile")]
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
                    List<NorceProductModel> result;
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
                await _logService.LogMessage(errorMessage, "MediaProcessing");
            }
        }

        private async Task UpdateFileUrl(string fileUrl)
        {
            string rowKey = "FileUrl";
            DynamicTableEntity entity = new DynamicTableEntity("MediaFileUrl", rowKey);

            entity.Properties["Url"] = new EntityProperty(fileUrl);
            entity.Properties["Onboarded"] = new EntityProperty(true);

            await _tableService.AddDynamicTableEntity("FileUrls", entity);

        }

        private async Task SaveProductDataToBlob(string jsonFileContent)
        {
            List<NorceProductModel> norceProductModels = JsonConvert.DeserializeObject<List<NorceProductModel>>(jsonFileContent);
            List<string> productGuids = new List<string>();
            foreach (var norceProduct in norceProductModels)
            {
                string fileName = $"{norceProduct.Code}_{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}";

                var json = JsonConvert.SerializeObject(norceProduct);
                await _blobService.UploadJsonBlobAsync(fileName, json, "mediaonboard");
                productGuids.Add(fileName);
            }
            foreach (var productGuid in productGuids)
            {
                await _blobService.SendToQueue(productGuid, "delta-media-queue");
            }

        }

        private async Task<string> GetFileUrl()
        {
            string url = string.Empty;
            url = await _tableService.GetDynamicTableEntityFileUrl("FileUrls", "MediaFileUrl", "FileUrl", "Url");
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
                await _logService.LogMessage(errorMessage, "MediaProcessing");
            }
            return data;
        }

        private bool TryParseJson(string json, out List<NorceProductModel> result)
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
                    result = JsonConvert.DeserializeObject<List<NorceProductModel>>(json);
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
                    _ = _logService.LogMessage(errorMessage, "MediaProcessing");
                    throw;
                }
            }
        }


    }
}
