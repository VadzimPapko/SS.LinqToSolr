using SS.LinqToSolr.Common;
using SS.LinqToSolr.Models;
using SS.LinqToSolr.ExpressionParsers;
using System.Linq.Expressions;
using SS.LinqToSolr.Helpers;
using SS.LinqToSolr.Models.Query;
using System.Linq;
using System.Collections.Generic;
using System;
using SS.LinqToSolr.Models.SearchResponse;

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
            var elementType = TypeSystem.GetElementType(expression.Type);
            var compositeQuery = Parse(expression);
            var query = compositeQuery.Translate();
            var response = _solrService.Search<T>(query);
            return ApplyScalarMethod(compositeQuery, response);
        }

        public override string GetQueryText(Expression expression)
        {
            return Parse(expression).Translate();
        }

        public Query<T> GetQueryable()
        {
            return new Query<T>(this);
        }
        private CompositeQuery Parse(Expression expression)
        {
            return new ExpressionParser(typeof(T)).Parse(expression);
        }

        private object ApplyScalarMethod(CompositeQuery compositeQuery, Response<T> response)
        {
            var documents = response.ResponseNode.Documents;
            if (!compositeQuery.ScalarMethods.Any())
                return documents;
            var method = compositeQuery.ScalarMethods.First().Name;
            switch (method)
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
                    throw new NotSupportedException($"'{method}' is not supported");
            }
        }
    }
}
