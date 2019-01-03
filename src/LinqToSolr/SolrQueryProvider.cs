using SS.LinqToSolr.Common;
using SS.LinqToSolr.Models;
using SS.LinqToSolr.ExpressionParsers;
using System.Linq.Expressions;
using SS.LinqToSolr.Models.Query;
using System.Linq;
using System;
using SS.LinqToSolr.Models.SearchResponse;

namespace SS.LinqToSolr
{
    public class SolrQueryProvider<T> : QueryProvider where T : Document
    {
        private readonly ISearchContext _solrService;

        public SolrQueryProvider(ISearchContext solrService)
        {
            _solrService = solrService;
        }

        public override object Execute(Expression expression)
        {
            var compositeQuery = Parse(expression);
            var query = compositeQuery.Translate();
            var response = _solrService.Search<T>(query);
            return ApplyScalarMethod(compositeQuery, response);
        }

        public override string GetQueryText(Expression expression)
        {
            return Parse(expression).Translate();
        }

        public virtual Query<T> GetQueryable()
        {
            return new Query<T>(this);
        }
        protected virtual CompositeQuery Parse(Expression expression)
        {
            return new ExpressionParser(typeof(T)).Parse(expression);
        }

        protected virtual object ApplyScalarMethod(CompositeQuery query, Response<T> response)
        {
            var documents = response.ResponseNode.Documents;
            if (query.ScalarMethod == null)
                return documents;
            switch (query.ScalarMethod.Name)
            {
                case "Count":
                    return response.ResponseNode.Found;
                case "First":
                    return documents.First();
                case "FirstOrDefault":
                    return documents.FirstOrDefault();
                case "Last":
                    return documents.Last();
                case "LastOrDefault":
                    return documents.LastOrDefault();
                case "Single":
                    return documents.Single();
                case "SingleOrDefault":
                    return documents.SingleOrDefault();
                default:
                    throw new NotSupportedException($"'{query.ScalarMethod.Name}' is not supported");
            }
        }
    }
}
