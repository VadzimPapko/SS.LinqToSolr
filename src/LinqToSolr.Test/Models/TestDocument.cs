using SS.LinqToSolr.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.LinqToSolr.Test.Models
{
    class TestDocument: Document
    {
        [JsonProperty("_uniqueid")]
        public string Id { get; set; }

        [JsonProperty("title_s")]
        public string Title { get; set; }

        [JsonProperty("_documentid")]
        public string DocumentId { get; set; }

        [JsonProperty("_type")]
        public string Type { get; set; }

        [JsonProperty("_timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
