using SS.LinqToSolr.Extensions;
using SS.LinqToSolr.Models.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SS.LinqToSolr.ExpressionParsers
{
    public class QueryableMethodVisitor
    {
        protected ExpressionVisitor _bodyParser;

        public QueryableMethodVisitor(ExpressionVisitor bodyParser)
        {
            _bodyParser = bodyParser;
        }

        protected List<MethodNode> _root = new List<MethodNode>();

        public List<MethodNode> Parse(Expression expression)
        {
            Visit(expression);
            return _root;
        }

        protected virtual void Visit(Expression exp)
        {
            if (exp.NodeType == ExpressionType.Call)
                _root.Add(VisitMethodCall((MethodCallExpression)exp));
        }

        protected virtual MethodNode VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable))
                return VisitQueryableMethod(m);
            if (m.Method.DeclaringType == typeof(QueryableExtensions))
                return VisitQueryableExtensionMethod(m);
            if (m.Method.DeclaringType == typeof(QueryableDismaxExtensions))
                return VisitQueryableDismaxExtensionMethod(m);
            return null;
        }

        private MethodNode VisitQueryableMethod(MethodCallExpression m)
        {
            var node = new MethodNode(m.Method.Name);
            switch (m.Method.Name)
            {
                case "Where":
                case "OrderBy":
                case "ThenBy":
                case "OrderByDescending":
                case "ThenByDescending":
                case "Take":
                case "Skip":
                    Visit(m.Arguments[0]);
                    node.Body = _bodyParser.Parse(m.Arguments[1]);
                    return node;
                case "Count":
                case "First":
                case "FirstOrDefault":
                case "Last":
                case "LastOrDefault":
                case "Single":
                case "SingleOrDefault":
                    Visit(m.Arguments[0]);

                    if (m.Arguments.Count == 2)
                    {
                        node.Body = _bodyParser.Parse(m.Arguments[1]);
                    }
                    return node;
                default:
                    throw new NotSupportedException($"'{m.Method.Name}' is not supported");
            }
        }

        private MethodNode VisitQueryableDismaxExtensionMethod(MethodCallExpression m)
        {
            var node = new MethodNode(m.Method.Name);
            switch (m.Method.Name)
            {
                case "BoostQuery":
                case "DismaxQueryAlt":
                    //node.AddChild(Visit(m.Arguments[1]));
                    //_root.AddChild(Visit(m.Arguments[0]));
                    return node;
                case "EDismax":
                    Visit(m.Arguments[0]);
                    return node;
                case "Dismax":
                    Visit(m.Arguments[0]);
                    return node;
                default:
                    throw new NotSupportedException($"'{m.Method.Name}' is not supported");
            }
        }

        private MethodNode VisitQueryableExtensionMethod(MethodCallExpression m)
        {
            var node = new MethodNode(m.Method.Name);
            switch (m.Method.Name)
            {
                case "GetResponse":
                    //_root.AddChild(Visit(m.Arguments[0]));
                    return node;
                case "Filter":
                case "Query":
                    //node.AddChild(Visit(m.Arguments[1]));
                    //_root.AddChild(Visit(m.Arguments[0]));
                    return node;
                case "Facet":
                    //var facetField = Visit(m.Arguments[1]);
                    //var facetValues = new List<string>();
                    //var facetIsMultiFacet = false;

                    //if (m.Arguments.Count >= 3)
                    //{
                    //    var exp = m.Arguments[2] as ConstantExpression;
                    //    var values = exp?.Value as IEnumerable<string>;
                    //    if (values == null)
                    //        values = exp?.Value as string[];
                    //    facetValues = values?.Select(x => x.ToSearchValue()).ToList();
                    //}
                    //if (m.Arguments.Count >= 4)
                    //{
                    //    facetIsMultiFacet = ((m.Arguments[3] as ConstantExpression)?.Value as bool?) ?? false;
                    //}
                    //node.AddChild(new FacetNode((QueryNode)Visit(m.Arguments[1]), facetValues, facetIsMultiFacet));
                    //_root.AddChild(Visit(m.Arguments[0]));
                    return node;
                case "PivotFacet":
                    //var facets = Visit(m.Arguments[1]);
                    //facets.Reverse();
                    //_compositeQuery.PivotFacets.Add(new PivotFacet(facets.Select(x => x.Field).ToList()));

                    //_root.AddChild(Visit(m.Arguments[0]));
                    return node;
                default:
                    throw new NotSupportedException($"'{m.Method.Name}' is not supported");
            }
        }
    }
}
