using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.WindowsAzure.Storage.Table;
using Occtoo.Provider.Norce.Model;
using Occtoo.Provider.Norce.Services;
using System;
using System.Threading.Tasks;

namespace Occtoo.Provider.Norce.Functions
{
    public class PriceApiCall
    {
        private readonly INorceService _norceService;
        private readonly ITableService _tableService;
        private readonly ILogService _logService;
        public PriceApiCall(ITableService tableService,
                              INorceService norceService,
                              ILogService logService)
        {
            _tableService = tableService;
            _norceService = norceService;
            _logService = logService;
        }


        [FunctionName("PriceApiCall")]
#if DEBUG
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
#else

        public async Task Run([TimerTrigger("0 8/15 * * * *")]TimerInfo myTimer, ILogger log)
#endif
        {
            try
            {
                string dateLastRun = await GetDateLastRun();

                string fileUrl = await GetDeltaFileUrlFromApiProduct(dateLastRun);

                if (!string.IsNullOrEmpty(fileUrl))
                {
                    await SaveFileUrl(fileUrl);

                    await SaveDate();

                }

            }
            catch (Exception ex)
            {
                LogMessageModel errorMessage = new(ex.Message, ex.StackTrace, true);
                await _logService.LogMessage(errorMessage, "PriceProcessing");
            }

        }

        private async Task SaveDate()
        {
            DateTime date = DateTime.Now;
            DateTime dateToSave = date.AddMinutes(-2);
            string dateString = dateToSave.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            if (!string.IsNullOrEmpty(dateString))
            {
                DynamicTableEntity entity = new DynamicTableEntity("DateLastRun", "PriceProcess");

                entity.Properties["Date"] = new EntityProperty(dateString);

                await _tableService.AddDynamicTableEntity("DateLastRun", entity);
            }
        }

        private async Task SaveFileUrl(string fileUrl)
        {
            string rowKey = "FileUrl";
            DynamicTableEntity entity = new DynamicTableEntity("PriceFileUrl", rowKey);

            entity.Properties["Url"] = new EntityProperty(fileUrl);
            entity.Properties["Onboarded"] = new EntityProperty(false);

            await _tableService.AddDynamicTableEntity("FileUrls", entity);

        }

        private async Task<string> GetDateLastRun()
        {
            DateTime date = DateTime.Now;
            string dateString = date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            string dateLastRun = await _tableService.GetDynamicTableEntity("DateLastRun", "DateLastRun", "PriceProcess", "Date");
            dateLastRun = string.IsNullOrEmpty(dateLastRun) ? dateString : dateLastRun;
            return dateLastRun;
        }

        private async Task<string> GetDeltaFileUrlFromApiProduct(string date)
        {
            string fileUrl = string.Empty;
            try
            {
                fileUrl = await _norceService.GetProductDeltaFileUrl(date);
            }
            catch (Exception ex)
            {
                LogMessageModel errorMessage = new(ex.Message, ex.StackTrace, true);
                await _logService.LogMessage(errorMessage, "PriceProcessing");
            }
            return fileUrl;
        }

    }
}
