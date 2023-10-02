using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Occtoo.Provider.Norce.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Occtoo.Provider.Norce.Services
{
    public interface ITableService
    {
        Task<DynamicTableEntity> GetTableEntity(string tableName, string partitionKey, string rowKey);
        Task<string> GetDynamicTableEntity(string tableName, string partitionName, string rowKey, string value);
        Task AddDynamicTableEntity(string tableName, DynamicTableEntity entity);
        Task<string> GetDynamicTableEntityFileUrl(string tableName, string partitionName, string rowKey, string value);
        Task PersistValues(string tableName, List<DynamicTableEntity> entities);
    }
    public class TableService : ITableService
    {
        public async Task<DynamicTableEntity> GetTableEntity(string tableName, string partitionKey, string rowKey)
        {
            CloudTable table = await GetTableReference(tableName);

            TableOperation retrieveOperation = TableOperation.Retrieve<DynamicTableEntity>(partitionKey, rowKey);

            TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);

            return retrievedResult.Result as DynamicTableEntity;

        }
        public async Task<string> GetDynamicTableEntity(string tableName, string partitionName, string rowKey, string value)
        {
            string date = string.Empty;
            var table = await GetTableReference(tableName);
            TableOperation getOperation = TableOperation.Retrieve(partitionName, rowKey);
            TableBatchOperation userEnvironmentBatch = new TableBatchOperation();
            userEnvironmentBatch.Add(getOperation);
            var res = await table.ExecuteBatchAsync(userEnvironmentBatch);
            TableResult tableResult = res.FirstOrDefault();

            if (tableResult != null && tableResult.Result is DynamicTableEntity dynamicEntity)
            {
                if (dynamicEntity.Properties.TryGetValue(value, out EntityProperty property))
                {
                    if (property.PropertyType == EdmType.String)
                    {
                        date = property.StringValue;
                    }
                }
            }

            return date;
        }

        public async Task<string> GetDynamicTableEntityFileUrl(string tableName, string partitionName, string rowKey, string value)
        {
            string date = string.Empty;
            var table = await GetTableReference(tableName);

            string combinedFilter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionName),
                TableOperators.And,
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForBool("Onboarded", QueryComparisons.Equal, false)
                )
            );

            var query = new TableQuery<DynamicTableEntity>().Where(combinedFilter);

            TableQuerySegment<DynamicTableEntity> resultSegment = await table.ExecuteQuerySegmentedAsync(query, null);

            if (resultSegment.Results.Count > 0)
            {
                var dynamicEntity = resultSegment.Results.FirstOrDefault();

                if (dynamicEntity.Properties.TryGetValue(value, out EntityProperty dateProperty))
                {
                    if (dateProperty.PropertyType == EdmType.String)
                    {
                        date = dateProperty.StringValue;
                    }
                }
            }

            return date;
        }

        public async Task AddDynamicTableEntity(string tableName, DynamicTableEntity entity)
        {
            var table = await GetTableReference(tableName);

            TableOperation addOperation = TableOperation.InsertOrReplace(entity);

            TableBatchOperation userEnvironmentBatch = new TableBatchOperation();
            userEnvironmentBatch.Add(addOperation);

            await table.ExecuteBatchAsync(userEnvironmentBatch);
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

        public async Task PersistValues(string tableName, List<DynamicTableEntity> entities)
        {
            var table = await GetTableReference(tableName);

            foreach (var batch in entities.Batch(50))
            {
                TableBatchOperation batchOperation = new TableBatchOperation();

                foreach (var entity in batch)
                {
                    batchOperation.Add(TableOperation.InsertOrReplace(entity));
                }

                var result = table.ExecuteBatchAsync(batchOperation).Result;

            }
        }

    }
}
