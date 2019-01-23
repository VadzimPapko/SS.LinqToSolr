using SS.LinqToSolr.Common;
using SS.LinqToSolr.Models;
using SS.LinqToSolr.ExpressionParsers;
using System.Linq.Expressions;
using SS.LinqToSolr.Models.Query;
using System.Linq;
using System;
using SS.LinqToSolr.Models.SearchResponse;
using SS.LinqToSolr.Translators;
using System.Collections.Generic;

namespace SS.LinqToSolr
{
    public class SolrQueryProvider<T> : QueryProvider
    {
        private QueryableMethodVisitor _visitor { get; set; }

        protected ISearchContext _solrService;
        protected IFieldTranslator _fieldTranslator;
        protected INodeTranslator _nodeTranslator;

        public SolrQueryProvider(ISearchContext solrService, IFieldTranslator fieldTranslator)
        {
            _solrService = solrService;
            _fieldTranslator = fieldTranslator;

            var bodyVisitor = new QueryableMethodBodyVisitor(typeof(T));
            _visitor = new QueryableMethodVisitor(bodyVisitor);

            _nodeTranslator = new NodeTranslator(_fieldTranslator);
        }

        public override object Execute(Expression expression)
        {
            var nodes = Parse(expression);
            var query = _nodeTranslator.Translate(nodes);
            //var query = compositeQuery.Translate();
            var response = _solrService.Search<T>(query);
            //return ApplyScalarMethod(compositeQuery, response);
            return null;
        }

        public override string GetQueryText(Expression expression)
        {
            //return Parse(expression).Translate();
            var nodes = Parse(expression);
            return _nodeTranslator.Translate(nodes);
        }

        public virtual Query<T> GetQueryable()
        {
            return new Query<T>(this);
        }
        protected virtual List<MethodNode> Parse(Expression expression)
        {
            return _visitor.Parse(expression);
        }

        //protected virtual object ApplyScalarMethod(CompositeQuery query, Response<T> response)
        //{
        //    var documents = response.ResponseNode.Documents;
        //    if (query.ScalarMethod == null)
        //        return documents;
        //    switch (query.ScalarMethod.Name)
        //    {
        //        case "GetResponse":
        //            return response;
        //        case "Count":
        //            return response.ResponseNode.Found;
        //        case "First":
        //            return documents.First();
        //        case "FirstOrDefault":
        //            return documents.FirstOrDefault();
        //        case "Last":
        //            return documents.Last();
        //        case "LastOrDefault":
        //            return documents.LastOrDefault();
        //        case "Single":
        //            return documents.Single();
        //        case "SingleOrDefault":
        //            return documents.SingleOrDefault();
        //        default:
        //            throw new NotSupportedException($"'{query.ScalarMethod.Name}' is not supported");
        //    }
        //}
    }
}
