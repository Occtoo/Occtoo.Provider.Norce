using System.Text.Json.Serialization;

namespace Occtoo.Provider.Norce.Model
{
    public class NorceResponseModel
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("jobKey")]
        public string JobKey { get; set; }
        [JsonPropertyName("dataUrl")]
        public string DataUrl { get; set; }
        [JsonPropertyName("itemsTotal")]
        public int ItemsTotal { get; set; }
    }

}
