using Newtonsoft.Json;
using Occtoo.Provider.Norce.Formatter;
using Occtoo.Provider.Norce.Model;
using Occtoo.Onboarding.Sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Occtoo.Provider.Norce.Services
{
    public interface INorceService
    {
        Task<string> GetProductFileUrl(string date);
        Task<string> GetProductDeltaFileUrl(string date);
        Task<string> GetProductFile(string fileUrl);
        Task<List<DynamicEntity>> GetProductsEntities(string fileContent);
        Task<List<DynamicEntity>> GetProductsEntitiesFullSync(string fileContent);
        Task<List<DynamicEntity>> GetMediaEntities(string fileContent);
        Task<List<DynamicEntity>> GetMediaEntitiesFullSync(string fileContent);

        List<DynamicEntity> GetPriceEntities(string fileContent);
        List<DynamicEntity> GetPriceEntitiesFullSync(string fileContent);
        List<DynamicEntity> GetPriceDeltaEntities(string fileContent);
        List<DynamicEntity> GetPriceDelta(string fileContent);
        List<DynamicEntity> GetStockEntities(string fileContent);
        List<DynamicEntity> GetStockEntitiesFullSync(string fileContent);

        List<DynamicEntity> GetStockEntitiesDelta(string fileContent);
        List<DynamicEntity> GetStockDelta(string fileContent);
        List<DynamicEntity> GetProductsEntitiesDelete(string fileContent);
        Task<List<DynamicEntity>> GetFullProductsEntities(string fileContent);
    }
    public class NorceService : INorceService
    {
        private readonly HttpClient _httpClient;
        private readonly IBlobService _blobService;
        private readonly IProductFormatter _productFormatter;
        private readonly IMediaFormatter _mediaFormatter;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IStockFormatter _stockFormatter;

        public NorceService(HttpClient httpClient, IBlobService blobService,
                            IProductFormatter productFormatter,
                            IMediaFormatter mediaFormatter,
                            IPriceFormatter priceFormatter, IStockFormatter stockFormatter)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
            _httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("BaseApi"));
            _blobService = blobService;
            _productFormatter = productFormatter;
            _mediaFormatter = mediaFormatter;
            _priceFormatter = priceFormatter;
            _stockFormatter = stockFormatter;
        }
        public async Task<string> GetProductFileUrl(string date)
        {
            string dataBlobUrl = await ReadDataFromNorce(date);
            return dataBlobUrl;
        }

        public async Task<string> GetProductDeltaFileUrl(string date)
        {
            string dataBlobUrl = await ReadDeltaDataFromNorce(date);
            return dataBlobUrl;
        }

        public async Task<string> GetProductFile(string fileUrl)
        {
            string data = await _blobService.GetBlobFeedByFileName(fileUrl);
            return data;
        }

        public async Task<List<DynamicEntity>> GetProductsEntitiesFullSync(string fileContent)
        {
            NorceProductModel norceProductModel = JsonConvert.DeserializeObject<NorceProductModel>(fileContent);
            var products = await _productFormatter.FormatToProductModelFullSync(norceProductModel);
            var entities = _productFormatter.FormatToEntites(products);
            entities = RemoveDuplicateProperties(entities);
            return entities;
        }

        public async Task<List<DynamicEntity>> GetProductsEntities(string fileContent)
        {
            List<NorceProductModel> norceProductModels = JsonConvert.DeserializeObject<List<NorceProductModel>>(fileContent);
            var products = await _productFormatter.FormatToProductModel(norceProductModels);
            var entities = _productFormatter.FormatToEntites(products);
            entities = RemoveDuplicateProperties(entities);
            return entities;
        }

        public async Task<List<DynamicEntity>> GetFullProductsEntities(string fileContent)
        {
            List<NorceProductModel> norceProductModels = JsonConvert.DeserializeObject<List<NorceProductModel>>(fileContent);
            var products = await _productFormatter.FormatToProductModel(norceProductModels);
            var entities = _productFormatter.FormatToEntites(products);
            entities = RemoveDuplicateProperties(entities);
            return entities;
        }

        public List<DynamicEntity> GetProductsEntitiesDelete(string fileContent)
        {
            List<NorceProductModel> norceProductModels = JsonConvert.DeserializeObject<List<NorceProductModel>>(fileContent);
            var products = _productFormatter.FormatToProductModelDelete(norceProductModels);
            var entities = _productFormatter.FormatToEntitesDelete(products);

            return entities;
        }

        public async Task<List<DynamicEntity>> GetMediaEntities(string fileContent)
        {
            List<NorceProductModel> norceProductModels = JsonConvert.DeserializeObject<List<NorceProductModel>>(fileContent);
            MediaProductModel mediaProductModel = await _mediaFormatter.FormatToMediaModel(norceProductModels);
            var medias = mediaProductModel.Media;
            var products = mediaProductModel.Products;
            var entities = _mediaFormatter.FormatToEntites(medias);
            var productEntities = _productFormatter.FormatToEntitesWithMedia(products);

            entities.AddRange(productEntities);

            return entities;
        }

        public async Task<List<DynamicEntity>> GetMediaEntitiesFullSync(string fileContent)
        {
            NorceProductModel norceProductModel = JsonConvert.DeserializeObject<NorceProductModel>(fileContent);
            MediaProductModel mediaProductModel = await _mediaFormatter.FormatToMediaModel(norceProductModel);
            var medias = mediaProductModel.Media;
            var products = mediaProductModel.Products;
            var entities = _mediaFormatter.FormatToEntites(medias);
            var productEntities = _productFormatter.FormatToEntitesWithMedia(products);

            entities.AddRange(productEntities);

            return entities;
        }

        public List<DynamicEntity> GetPriceEntities(string fileContent)
        {
            List<NorceProductModel> norceProductModels = JsonConvert.DeserializeObject<List<NorceProductModel>>(fileContent);
            var prices = _priceFormatter.FormatToPriceModel(norceProductModels);
            var entities = _priceFormatter.FormatToEntites(prices);

            return entities;
        }

        public List<DynamicEntity> GetPriceEntitiesFullSync(string fileContent)
        {
            NorceProductModel norceProductModel = JsonConvert.DeserializeObject<NorceProductModel>(fileContent);
            var prices = _priceFormatter.FormatToPriceModel(norceProductModel);
            var entities = _priceFormatter.FormatToEntites(prices);

            return entities;
        }


        public List<DynamicEntity> GetPriceDeltaEntities(string fileContent)
        {
            List<NorcePriceStockModel> norceProductModels = JsonConvert.DeserializeObject<List<NorcePriceStockModel>>(fileContent);
            var prices = _priceFormatter.FormatToPriceModel(norceProductModels);
            var entities = _priceFormatter.FormatToEntites(prices);

            return entities;
        }
        public List<DynamicEntity> GetPriceDelta(string fileContent)
        {
            List<NorcePriceStockModel> norceProductModels = new List<NorcePriceStockModel>();
            NorcePriceStockModel norceProductModel = JsonConvert.DeserializeObject<NorcePriceStockModel>(fileContent);
            norceProductModels.Add(norceProductModel);
            var prices = _priceFormatter.FormatToPriceModel(norceProductModels);
            var entities = _priceFormatter.FormatToEntites(prices);

            return entities;
        }

        public List<DynamicEntity> GetStockEntities(string fileContent)
        {
            List<NorceProductModel> norceProductModels = JsonConvert.DeserializeObject<List<NorceProductModel>>(fileContent);
            var stocks = _stockFormatter.FormatToStockModel(norceProductModels);
            var entities = _stockFormatter.FormatToEntites(stocks);

            return entities;
        }

        public List<DynamicEntity> GetStockEntitiesFullSync(string fileContent)
        {
            NorceProductModel norceProductModels = JsonConvert.DeserializeObject<NorceProductModel>(fileContent);
            var stocks = _stockFormatter.FormatToStockModel(norceProductModels);
            var entities = _stockFormatter.FormatToEntites(stocks);

            return entities;
        }


        public List<DynamicEntity> GetStockEntitiesDelta(string fileContent)
        {
            List<NorcePriceStockModel> norceProductModels = JsonConvert.DeserializeObject<List<NorcePriceStockModel>>(fileContent);
            var stocks = _stockFormatter.FormatToStockModel(norceProductModels);
            var entities = _stockFormatter.FormatToEntites(stocks);

            return entities;
        }

        public List<DynamicEntity> GetStockDelta(string fileContent)
        {
            List<NorcePriceStockModel> norceProductModels = new List<NorcePriceStockModel>();
            NorcePriceStockModel norceProductModel = JsonConvert.DeserializeObject<NorcePriceStockModel>(fileContent);
            norceProductModels.Add(norceProductModel);
            var stocks = _stockFormatter.FormatToStockModel(norceProductModels);
            var entities = _stockFormatter.FormatToEntites(stocks);

            return entities;
        }

        private List<DynamicEntity> RemoveDuplicateProperties(List<DynamicEntity> entities)
        {
            List<DynamicEntity> filteredList = new();
            foreach (var entity in entities)
            {
                entity.Properties = entity.Properties
                    .GroupBy(prop => new { prop.Id, prop.Language })
                    .Select(group => group.First())
                    .ToList();
                filteredList.Add(entity);
            }

            return filteredList;
        }


        private async Task<string> ReadDataFromNorce(string date)
        {
            var requestBody = new
            {
                channelKey = Environment.GetEnvironmentVariable("ChannelKey"),
                deltaFromDate = date
            };

            var requestBodyJson = JsonConvert.SerializeObject(requestBody);
            var httpContent = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(Environment.GetEnvironmentVariable("ProductApi"), httpContent);
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NorceResponseModel>(responseContent);
            if (result != null)
            {
                return result.DataUrl;
            }
            else return string.Empty;
        }

        private async Task<string> ReadDeltaDataFromNorce(string date)
        {
            var requestBody = new
            {
                channelKey = Environment.GetEnvironmentVariable("ChannelKey"),
                deltaFromDate = date
            };

            var requestBodyJson = JsonConvert.SerializeObject(requestBody);
            var httpContent = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(Environment.GetEnvironmentVariable("ProductStatusApi"), httpContent);
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NorceResponseModel>(responseContent);
            if (result != null)
            {
                return result.DataUrl;
            }
            else return string.Empty;
        }
    }
}
