using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Occtoo.Provider.Norce.Model
{
    public class DestinationResponseModel<T>
    {
        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("results")]
        public IEnumerable<T> Results { get; set; }

        [JsonPropertyName("facets")]
        public Facet[] Facets { get; set; }
    }
}
