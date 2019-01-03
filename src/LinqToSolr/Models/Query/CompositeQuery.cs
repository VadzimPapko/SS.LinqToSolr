using SS.LinqToSolr.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SS.LinqToSolr.Models.Query
{
    public class CompositeQuery
    {
        private string _value = null;
        public List<string> Query { get; set; } = new List<string>();
        public List<string> QueryFilters { get; set; } = new List<string>();
        public List<Facet> Facets { get; set; } = new List<Facet>();
        public List<PivotFacet> PivotFacets { get; set; } = new List<PivotFacet>();
        public List<OrderBy> OrderByList { get; set; } = new List<OrderBy>();
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public MethodInfo ScalarMethod { get; set; }

        public void Write(string val)
        {
            _value += val;
        }

        public string Translate()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(_value))
                return _value;

            if (Query.Any())
            {
                var falsePredicat = "( OR ";
                var truePredicat = "( AND ";
                var q = string.Join(" AND ", Query.Select(x => x.ToSolrGroup()));
                sb.Append($"q={q.Replace(falsePredicat, "(").Replace(truePredicat, "(")}");
            }
            else
            {
                sb.Append($"q=*:*");
            }

            if (PivotFacets.Any())
            {
                PivotFacets.ForEach(x =>
                {
                    sb.Append($"&facet.pivot={string.Join(",", x.Fields)}");
                });
            }

            if (Facets.Any())
            {
                Facets.ForEach(x =>
                {
                    sb.Append($"&facet.field={(x.IsMultiFacet && x.Values != null && x.Values.Any() ? $"{{!ex={x.Field}}}" : "")}{x.Field}");
                    if (x.Values != null && x.Values.Any())
                    {
                        sb.Append($"&fq={(x.IsMultiFacet ? $"{{!tag={x.Field}}}" : "")}{x.Field}:{string.Join(" OR ", x.Values.Select(v => v.ToSearchValue())).ToSolrGroup()}");
                    }
                });
            }

            if (PivotFacets.Any() || Facets.Any())
                sb.Append($"&facet=on");

            if (QueryFilters.Any())
            {
                var fq = string.Join(" AND ", QueryFilters.Select(x => x.ToSolrGroup()));
                sb.Append($"&fq={fq}");
            }

            if (OrderByList.Any())
            {
                sb.Append($"&sort=");
                sb.Append(string.Join(",", OrderByList.Select(x => x.Translate())));
            }

            if (Skip.HasValue)
                sb.Append($"&start={Skip}");
            if (Take.HasValue)
                sb.Append($"&rows={Take}");

            return sb.ToString();
        }
    }
}