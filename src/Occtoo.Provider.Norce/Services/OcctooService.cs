using Occtoo.Provider.Norce.Model;
using Occtoo.Onboarding.Sdk;
using Occtoo.Onboarding.Sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Occtoo.Provider.Norce.Services
{
    public interface IOcctooService
    {
        Task ImportEntitiesAsync(IReadOnlyList<DynamicEntity> entities, string dataSource, string dataProviderId, string dataProviderSecret, string token);
        Task<string> GetToken(string dataProviderId, string dataProviderSecret);
        Task<JsonDocument> GetTokenDocument(string dataProviderId, string dataProviderSecret);
        Task<JsonDocument> GetProductDestinationToken();
        Task<List<OcctooProductModel>> GetOcctooProducts(List<string> productIds, string token);
    }

    public class OcctooService : IOcctooService
    {
        private readonly HttpClient _httpClient;

        public OcctooService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("OcctooBaseAddress"));

        }

        public async Task ImportEntitiesAsync(IReadOnlyList<DynamicEntity> entities, string dataSource, string dataProviderId, string dataProviderSecret, string token)
        {
            DynamicEntityModel model = new DynamicEntityModel
            {
                Entities = entities.ToList(),
            };
            var onboardingServliceClient = new OnboardingServiceClient(dataProviderId, dataProviderSecret);
            var response = await onboardingServliceClient.StartEntityImportAsync(dataSource, entities, token);
            if (response.StatusCode != 202)
            {
                throw new Exception("There was a problem with onboarding data to Occtoo Studio");
            }

        }

        public async Task<string> GetToken(string dataProviderId, string dataProviderSecret)
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "dataProviders/tokens");
            tokenRequest.Content = new StringContent(JsonSerializer.Serialize(new
            {
                id = dataProviderId,
                secret = dataProviderSecret
            }), Encoding.UTF8, "application/json");
            var tokenResponse = await _httpClient.SendAsync(tokenRequest);
            var tokenResponseContent = await tokenResponse.Content.ReadAsStreamAsync();
            var tokenDocument = JsonSerializer.Deserialize<JsonDocument>(tokenResponseContent);
            var token = tokenDocument.RootElement.GetProperty("result").GetProperty("accessToken").GetString();
            return token;
        }

        public async Task<JsonDocument> GetTokenDocument(string dataProviderId, string dataProviderSecret)
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "dataProviders/tokens");
            tokenRequest.Content = new StringContent(JsonSerializer.Serialize(new
            {
                id = dataProviderId,
                secret = dataProviderSecret
            }), Encoding.UTF8, "application/json");
            var tokenResponse = await _httpClient.SendAsync(tokenRequest);
            var tokenResponseContent = await tokenResponse.Content.ReadAsStreamAsync();
            var tokenDocument = JsonSerializer.Deserialize<JsonDocument>(tokenResponseContent);
            return tokenDocument;
        }

        public async Task<JsonDocument> GetProductDestinationToken()
        {
            var productTokenUrl = Environment.GetEnvironmentVariable("ProductTokenUrl");
            var productTokenSecret = Environment.GetEnvironmentVariable("ProductTokenSecret");
            var productTokenClientId = Environment.GetEnvironmentVariable("ProductTokenClientId");

            var body = "{\"clientId\": \"" + productTokenClientId + "\", \"clientSecret\": \"" + productTokenSecret + "\"}";
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(
                    productTokenUrl,
                    content);
            response.EnsureSuccessStatusCode();
            var tokenResponseContent = await response.Content.ReadAsStreamAsync();
            var tokenDocument = JsonSerializer.Deserialize<JsonDocument>(tokenResponseContent);
            return tokenDocument;

        }

        public async Task<List<OcctooProductModel>> GetOcctooProducts(List<string> productIds, string token)
        {
            var destinationEndPoint = Environment.GetEnvironmentVariable("ProductDestination");

            var body = JsonSerializer.Serialize(new
            {
                top = 10000,
                searchOn = new[] { "id" },
                search = productIds
            }, new JsonSerializerOptions { WriteIndented = true });


            var content = new StringContent(body, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync(
                    destinationEndPoint,
                    content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseModel = JsonSerializer.Deserialize<DestinationResponseModel<OcctooProductModel>>(responseContent);

            return responseModel.Results.ToList();
        }

    }
}
