using Newtonsoft.Json;
using System.Collections.Generic;

namespace SS.LinqToSolr.Models.SearchResponse
{
    public class ResponseNode<T>
    {
        [JsonProperty(PropertyName = "numFound")]
        public int Found { get; set; }

        [JsonProperty(PropertyName = "start")]
        public int Start { get; set; }

        [JsonIgnore]
        public int PerPage { get; set; }

        [JsonProperty(PropertyName = "docs")]
        public IEnumerable<T> Documents { get; set; }
    }
}