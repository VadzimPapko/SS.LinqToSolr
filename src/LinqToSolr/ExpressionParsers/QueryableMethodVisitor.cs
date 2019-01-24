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
            if (exp.NodeType == ExpressionType.Quote)
                VisitQuote(exp);
            if (exp.NodeType == ExpressionType.Lambda)
                VisitLambda((LambdaExpression)exp);
        }

        protected virtual MethodNode VisitMethodCall(MethodCallExpression m)
        {
            Visit(m.Arguments[0]);

            var node = new MethodNode(m.Method.Name, m.Type.DeclaringType);
            switch (m.Method.Name)
            {
                case "Facet":
                    var facetField = _bodyParser.Parse(m.Arguments[1]);
                    var facetValues = new List<string>();
                    var facetIsMultiFacet = false;

                    if (m.Arguments.Count >= 3)
                    {
                        var exp = m.Arguments[2] as ConstantExpression;
                        var values = exp?.Value as IEnumerable<string>;
                        if (values == null)
                            values = exp?.Value as string[];
                        facetValues = values?.Select(x => x.ToSearchValue()).ToList();
                    }
                    if (m.Arguments.Count >= 4)
                    {
                        facetIsMultiFacet = ((m.Arguments[3] as ConstantExpression)?.Value as bool?) ?? false;
                    }

                    node.Body = new FacetNode(facetField, facetValues, facetIsMultiFacet);
                    return node;
                case "PivotFacet":
                    var pivotFacetBody = new QueryableMethodVisitor(_bodyParser).Parse(m.Arguments[1]);
                    node.Body = new PivotFacetNode(pivotFacetBody);
                    return node;
                default:

                    if (m.Arguments.Count == 2)
                    {
                        node.Body = _bodyParser.Parse(m.Arguments[1]);
                    }
                    return node;
            }
        }

        protected void VisitQuote(Expression exp)
        {
            while (exp.NodeType == ExpressionType.Quote)
            {
                exp = ((UnaryExpression)exp).Operand;
            }
            Visit(exp);
        }

        protected void VisitLambda(LambdaExpression exp)
        {
            Visit(exp.Body);
        }
    }
}
