using SS.LinqToSolr.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SS.LinqToSolr.Models.Query
{
    public class CompositeQuery
    {
        protected string _value = null;
        public List<string> Query { get; set; } = new List<string>();
        public List<string> QueryFilters { get; set; } = new List<string>();
        public List<Facet> Facets { get; set; } = new List<Facet>();
        public List<PivotFacet> PivotFacets { get; set; } = new List<PivotFacet>();
        public List<OrderBy> OrderByList { get; set; } = new List<OrderBy>();
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public MethodInfo ScalarMethod { get; set; }

        public QueryParser QueryParser { get; set; } = QueryParser.Default;
        public string DismaxQuery { get; set; }

        public void Write(string val)
        {
            _value += val;
        }

        public virtual string GetQueryValue()
        {
            var sb = new StringBuilder();
            if (Query.Any())
            {
                var falsePredicat = "( OR ";
                var truePredicat = "( AND ";
                var q = string.Join(" AND ", Query.Select(x => x.ToSolrGroup()));
                sb.Append(q.Replace(falsePredicat, "(").Replace(truePredicat, "("));
            }
            else if (QueryParser == QueryParser.Default)
            {
                sb.Append("*:*");
            }
            return sb.ToString();
        }

        public virtual string TranslateQuery()
        {
            return $"q={GetQueryValue()}";
        }

        public virtual string TranslateFacets()
        {
            var sb = new StringBuilder();
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
                        sb.Append($"&fq={(x.IsMultiFacet ? $"{{!tag={x.Field}}}" : "")}{x.Field}:{string.Join(" OR ", x.Values).ToSolrGroup()}");
                    }
                });
            }

            if (PivotFacets.Any() || Facets.Any())
                sb.Append($"&facet=on");
            return sb.ToString();
        }

        public virtual string TranslatePager()
        {
            var sb = new StringBuilder();
            if (Skip.HasValue)
                sb.Append($"&start={Skip}");
            if (Take.HasValue)
                sb.Append($"&rows={Take}");
            return sb.ToString();
        }

        public virtual string TranslateOrder()
        {
            var sb = new StringBuilder();
            if (OrderByList.Any())
            {
                sb.Append($"&sort=");
                sb.Append(string.Join(",", OrderByList.Select(x => x.Translate())));
            }
            return sb.ToString();
        }

        public virtual string TranslateQueryFilters()
        {
            var sb = new StringBuilder();
            if (QueryFilters.Any())
            {
                var fq = string.Join(" AND ", QueryFilters.Select(x => x.ToSolrGroup()));
                sb.Append($"&fq={fq}");
            }
            return sb.ToString();
        }

        public virtual string Translate()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(_value) && QueryParser == QueryParser.Default)
                return _value;

            sb.Append(TranslateQuery());
            sb.Append(TranslateQueryFilters());
            sb.Append(TranslateFacets());
            sb.Append(TranslateOrder());
            sb.Append(TranslatePager());

            if (QueryParser == QueryParser.Dismax)
            {
                sb.Append($"{DismaxQuery}&defType=dismax");
            }

            return sb.ToString();
        }
    }
}