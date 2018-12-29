using SS.LinqToSolr.Models;
using System.Linq.Expressions;

namespace SS.LinqToSolr
{
    public class SolrQueryProvider<T> : QueryProvider where T : Document
    {
        private readonly ISolrService _solrService;

        public SolrQueryProvider(ISolrService solrService)
        {
            _solrService = solrService;
        }

        public override object Execute(Expression expression)
        {
            var query = Translate(expression);
            return _solrService.SearchForIQueryable<T>(query);
        }

        private string Translate(Expression expression)
        {
            return new ExpressionParser(typeof(T)).Parse(expression).Translate();
        }

        public override string GetQueryText(Expression expression)
        {
            return Translate(expression);
        }

        public Query<T> CreateContext()
        {
            return new Query<T>(this);
        }
    }
}
