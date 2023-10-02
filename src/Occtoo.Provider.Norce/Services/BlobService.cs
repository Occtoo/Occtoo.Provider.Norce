using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Occtoo.Provider.Norce.Services
{
    public interface IBlobService
    {
        Task<string> GetBlobFeedByFileName(string fileUrl);
        Task<string> ReadJsonBlobAsync(string blobName, string containerName);
        Task UploadJsonBlobAsync(string guid, string json, string containerName);
        Task SendToQueue(string productGuid, string queueName);
    }
    public class BlobService : IBlobService
    {
        private readonly HttpClient _httpClient;
        public BlobService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<string> GetBlobFeedByFileName(string fileUrl)
        {
            try
            {
                using (HttpResponseMessage response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    using (Stream stream = await response.Content.ReadAsStreamAsync())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        StringBuilder content = new StringBuilder();
                        char[] buffer = new char[4096];
                        int bytesRead;

                        while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            content.Append(buffer, 0, bytesRead);
                        }

                        return content.ToString();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<string> ReadJsonBlobAsync(string blobName, string containerName)
        {
            string connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"Blob '{blobName}' not found in container '{containerName}'.");
            }

            BlobDownloadInfo blobDownloadInfo = await blobClient.DownloadAsync();

            using (StreamReader reader = new StreamReader(blobDownloadInfo.Content))
            {
                string json = await reader.ReadToEndAsync();
                return json;
            }
        }

        public async Task UploadJsonBlobAsync(string guid, string json, string containerName)
        {
            string connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            string blobName = $"{guid}.json";

            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            await blobClient.UploadAsync(stream);
        }

        public async Task SendToQueue(string productGuid, string queueName)
        {
            string connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference(queueName);

            CloudQueueMessage message = new CloudQueueMessage(productGuid);
            await queue.AddMessageAsync(message);

        }


    }
}
