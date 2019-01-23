using SS.LinqToSolr.Extensions;
using SS.LinqToSolr.Models.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SS.LinqToSolr.Translators;

namespace SS.LinqToSolr.ExpressionParsers
{
    public class ExpressionParser : System.Linq.Expressions.ExpressionVisitor
    {
        protected Type _itemType;
        protected CompositeQuery _compositeQuery;
        protected IFieldTranslator _fieldTranslator;

        public ExpressionParser(Type itemType, IFieldTranslator fieldTranslator)
        {
            _itemType = itemType;
            _compositeQuery = new CompositeQuery();
            _fieldTranslator = fieldTranslator;
        }

        public virtual CompositeQuery Parse(Expression expression)
        {
            Visit(expression);
            return _compositeQuery;
        }

        protected virtual Expression VisitQueryableDismaxExtensionMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "BoostQuery":
                    _compositeQuery.BoostQuery.Add(New().Parse(m.Arguments[1]));
                    Visit(m.Arguments[0]);
                    return m;
                case "DismaxQueryAlt":
                    _compositeQuery.QueryAlt = New().Parse(m.Arguments[1]);
                    Visit(m.Arguments[0]);
                    return m;
                case "EDismax":
                    _compositeQuery.QueryParser = QueryParser.EDismax;
                    return m;
                case "Dismax":
                    _compositeQuery.QueryParser = QueryParser.Dismax;
                    return m;
                default:
                    throw new NotSupportedException($"'{m.Method.Name}' is not supported");
            }
        }

        protected virtual Expression VisitQueryableExtensionMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "GetResponse":
                    _compositeQuery.ScalarMethod = m.Method;
                    Visit(m.Arguments[0]);
                    return m;
                case "Filter":
                    return VisitFacet(m);
                case "Query":
                    _compositeQuery.Query.Add(New().Parse(m.Arguments[1]));
                    Visit(m.Arguments[0]);
                    return m;
                case "Facet":
                    return VisitFacet(m);
                case "PivotFacet":
                    return VisitPivotFacet(m);
                default:
                    throw new NotSupportedException($"'{m.Method.Name}' is not supported");
            }
        }

        protected virtual Expression VisitQueryableMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Where":
                    _compositeQuery.Query.Add(New().Parse(m.Arguments[1]));
                    Visit(m.Arguments[0]);
                    return m;
                case "Count":
                    if (m.Arguments.Count == 1)
                    {
                        Visit(m.Arguments[0]);
                    }
                    else if (m.Arguments.Count == 2)
                    {
                        _compositeQuery.Query.Add(New().Parse(m.Arguments[1]));
                        Visit(m.Arguments[0]);
                    }
                    _compositeQuery.ScalarMethod = m.Method;
                    return m;
                case "First":
                case "FirstOrDefault":
                case "Last":
                case "LastOrDefault":
                case "Single":
                case "SingleOrDefault":
                    if (m.Arguments.Count == 1)
                    {
                        Visit(m.Arguments[0]);
                    }
                    else if (m.Arguments.Count == 2)
                    {
                        _compositeQuery.Query.Add(New().Parse(m.Arguments[1]));
                        Visit(m.Arguments[0]);
                    }

                    _compositeQuery.Take = 1;
                    _compositeQuery.ScalarMethod = m.Method;
                    return m;
                case "OrderBy":
                case "ThenBy":
                case "OrderByDescending":
                case "ThenByDescending":
                    var field = New().Parse(m.Arguments[1]);
                    _compositeQuery.OrderByList.Add(new OrderBy(field, m.Method.Name));
                    Visit(m.Arguments[0]);
                    return m;
                case "Take":
                    _compositeQuery.Take = (int)((ConstantExpression)m.Arguments[1]).Value;
                    Visit(m.Arguments[0]);
                    return m;
                case "Skip":
                    _compositeQuery.Skip = (int)((ConstantExpression)m.Arguments[1]).Value;
                    Visit(m.Arguments[0]);
                    return m;
                default:
                    throw new NotSupportedException($"'{m.Method.Name}' is not supported");
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable))
                return VisitQueryableMethod(m);
            if (m.Method.DeclaringType == typeof(QueryableExtensions))
                return VisitQueryableExtensionMethod(m);
            if (m.Method.DeclaringType == typeof(QueryableDismaxExtensions))
                return VisitQueryableDismaxExtensionMethod(m);

            throw new NotSupportedException($"'{m.Method.Name}' is not supported");
        }

        protected virtual string GetFieldName(MemberInfo member)
        {
            return _fieldTranslator.Translate(member);
        }

        protected virtual object GetValue(Expression exp, MemberExpression parentExp = null)
        {
            object val = null;
            if (parentExp != null && exp.NodeType == ExpressionType.MemberAccess && parentExp.NodeType == ExpressionType.MemberAccess)
            {
                val = GetValue(((MemberExpression)exp).Expression, (MemberExpression)exp);
                val = GetMemberValue(parentExp, val);
            }
            else if (exp.NodeType == ExpressionType.Call)
            {
                return GetValue(((MethodCallExpression)exp).Arguments[0]);
            }
            else if (exp.NodeType == ExpressionType.MemberAccess)
            {
                return GetValue(((MemberExpression)exp).Expression, (MemberExpression)exp);
            }
            else if (exp.NodeType == ExpressionType.Constant)
            {
                var cExp = (ConstantExpression)exp;
                val = cExp.Value;
                val = GetMemberValue(parentExp, val);
            }
            else if (exp.NodeType == ExpressionType.Parameter)
            {
                val = GetFieldName(parentExp.Member);
            }
            else if (exp.NodeType == ExpressionType.ArrayIndex)
            {
                var be = (BinaryExpression)exp;
                var left = GetValue(be.Left) as IList<object>;
                var right = (int)((ConstantExpression)be.Right).Value;
                var obj = left[right];
                val = GetMemberValue(parentExp, obj);
            }
            return val;
        }

        protected virtual object GetMemberValue(MemberExpression m, object obj)
        {
            if (m != null)
            {
                object val = null;
                if (m.Member is FieldInfo)
                {
                    val = ((FieldInfo)m.Member).GetValue(obj);
                }
                else if (m.Member is PropertyInfo)
                {
                    val = ((PropertyInfo)m.Member).GetValue(obj, null);
                }
                return val;
            }
            return obj;
        }

        protected virtual NodeExpressionParser New()
        {
            return new NodeExpressionParser(_itemType, _fieldTranslator);
        }

        protected virtual Expression VisitFilter(MethodCallExpression m)
        {
            _compositeQuery.QueryFilters.Add(New().Parse(m.Arguments[1]));
            Visit(m.Arguments[0]);
            return m;
        }
        protected virtual Expression VisitFacet(MethodCallExpression m)
        {
            var facetField = New().Parse(m.Arguments[1]);
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
            if (!string.IsNullOrWhiteSpace(facetField))
                _compositeQuery.Facets.Add(new Facet(facetField, facetValues, facetIsMultiFacet));

            Visit(m.Arguments[0]);
            return m;
        }

        protected virtual Expression VisitPivotFacet(MethodCallExpression m)
        {
            var facets = new ExpressionParser(_itemType, _fieldTranslator).Parse(m.Arguments[1]).Facets;
            facets.Reverse();
            _compositeQuery.PivotFacets.Add(new PivotFacet(facets.Select(x => x.Field).ToList()));

            Visit(m.Arguments[0]);
            return m;
        }
    }
}