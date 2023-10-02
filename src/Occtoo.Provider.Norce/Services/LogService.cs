using Microsoft.WindowsAzure.Storage.Table;
using Occtoo.Provider.Norce.Model;
using System;
using System.Threading.Tasks;

namespace Occtoo.Provider.Norce.Services
{
    public interface ILogService
    {
        Task LogMessage(LogMessageModel logMessage, string partitionKey);
    }
    public class LogService : ILogService
    {
        private readonly ITableService _tableService;
        public LogService(ITableService tableService)
        {
            _tableService = tableService;
        }
        public async Task LogMessage(LogMessageModel logMessage, string partitionKey)
        {
            if (logMessage != null)
            {
                var rowKey = Guid.NewGuid().ToString();
                DynamicTableEntity entity = new DynamicTableEntity(partitionKey, rowKey);

                entity.Properties["Message"] = new EntityProperty(logMessage.Message);
                entity.Properties["StackTrace"] = new EntityProperty(logMessage.StackTrace);
                entity.Properties["IsError"] = new EntityProperty(logMessage.IsError);
                entity.Properties["DateTime"] = new EntityProperty(logMessage.DateTimeString);

                await _tableService.AddDynamicTableEntity("Log", entity);
            }
        }
    }
}
