using SS.LinqToSolr.JsonConverters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SS.LinqToSolr.Model.SearchResponse
{
    public class Response<T>
    {
        [JsonProperty(PropertyName = "response")]
        public ResponseNode<T> ResponseNode { get; set; }

        [JsonProperty(PropertyName = "facet_counts")]
        [JsonConverter(typeof(FacetJsonConverter))]
        public Dictionary<string, IEnumerable<IFacet>> Facets { get; set; }

        [JsonProperty(PropertyName = "spellcheck")]
        [JsonConverter(typeof(SpellJsonConverter))]
        public IEnumerable<Spellcheck> SpellCheck { get; set; }

        [JsonProperty(PropertyName = "error")]
        public Error Error { get; set; }
    }
}