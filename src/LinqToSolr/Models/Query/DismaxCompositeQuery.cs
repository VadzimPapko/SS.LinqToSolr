using SS.LinqToSolr.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SS.LinqToSolr.Models.Query
{
    public class DismaxCompositeQuery
    {
        public string Query { get; set; }
        public string QueryAlt { get; set; }
        public List<string> BoostQuery { get; set; } = new List<string>();

        public virtual string Translate()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(QueryAlt))
                sb.Append($"&q.alt={QueryAlt}");

            if (BoostQuery.Any())
                sb.Append($"&bq={string.Join($" ", BoostQuery)}");

            return sb.ToString();
        }
    }
}