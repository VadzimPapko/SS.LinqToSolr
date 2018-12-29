using SS.LinqToSolr.Extensions;
using SS.LinqToSolr.Model.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SS.LinqToSolr
{
    public sealed class ExpressionParser : ExpressionVisitor
    {
        private Type _itemType { get; set; }
        private CompositeQuery _compositeQuery = new CompositeQuery();

        public ExpressionParser(Type itemType)
        {
            _itemType = itemType;
        }

        private string GetFieldName(MemberInfo member)
        {
            var dataMemberAttribute = member.GetCustomAttribute<JsonPropertyAttribute>();
            var fieldName = !string.IsNullOrEmpty(dataMemberAttribute?.PropertyName)
                ? dataMemberAttribute.PropertyName
                : member.Name;

            return fieldName;
        }

        public CompositeQuery Parse(Expression expression)
        {
            Visit(expression);
            return _compositeQuery;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        private object GetValue(Expression exp, MemberExpression parentExp = null)
        {
            object val = null;
            if (parentExp != null && exp.NodeType == ExpressionType.MemberAccess && parentExp.NodeType == ExpressionType.MemberAccess)
            {
                val = GetValue(((MemberExpression)exp).Expression, (MemberExpression)exp);
                if (parentExp.Member is FieldInfo)
                {
                    val = ((FieldInfo)parentExp.Member).GetValue(val);
                }
                else if (parentExp.Member is PropertyInfo)
                {
                    val = ((PropertyInfo)parentExp.Member).GetValue(val, null);
                }
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
                if (parentExp != null)
                {
                    if (parentExp.Member is PropertyInfo)
                    {
                        val = ((PropertyInfo)parentExp.Member).GetValue(val, null);
                    }
                    else if (parentExp.Member is FieldInfo)
                    {
                        val = ((FieldInfo)parentExp.Member).GetValue(val);
                    }
                }
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

                if (parentExp.Member is FieldInfo)
                {
                    val = ((FieldInfo)parentExp.Member).GetValue(obj);
                }
                else if (parentExp.Member is PropertyInfo)
                {
                    val = ((PropertyInfo)parentExp.Member).GetValue(obj, null);
                }
            }
            return val;
        }

        private Expression VisitQueryableExtensionMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Filter":
                    _compositeQuery.QueryFilters.Add(new ExpressionParser(_itemType).Parse(((LambdaExpression)StripQuotes(m.Arguments[1])).Body).Translate());
                    Visit(StripQuotes(m.Arguments[0]));
                    return m;
                case "Query":
                    _compositeQuery.Query.Add(GetValue(m.Arguments[1]).ToString());
                    Visit(StripQuotes(m.Arguments[0]));
                    return m;
                case "Facet":
                    var facetField = GetValue(((LambdaExpression)StripQuotes(m.Arguments[1])).Body).ToString();
                    var facetValues = new List<string>();
                    var facetIsMultiFacet = false;

                    if (m.Arguments.Count >= 3)
                    {
                        var exp = m.Arguments[2] as ConstantExpression;
                        var values = exp?.Value as IEnumerable<string>;
                        if (values == null)
                            values = exp?.Value as string[];
                        facetValues = values?.ToList();
                    }
                    if (m.Arguments.Count >= 4)
                    {
                        facetIsMultiFacet = ((m.Arguments[3] as ConstantExpression)?.Value as bool?) ?? false;
                    }
                    if (!string.IsNullOrWhiteSpace(facetField))
                        _compositeQuery.Facets.Add(new Facet(facetField, facetValues, facetIsMultiFacet));

                    Visit(StripQuotes(m.Arguments[0]));
                    return m;
                case "PivotFacet":
                    var facets = new ExpressionParser(_itemType).Parse(((LambdaExpression)StripQuotes(m.Arguments[1])).Body).Facets;
                    facets.Reverse();
                    _compositeQuery.PivotFacets.Add(new PivotFacet(facets.Select(x => x.Field).ToList()));

                    Visit(StripQuotes(m.Arguments[0]));
                    return m;

                default:
                    throw new NotSupportedException(
                        $"'{m.Method.Name}' is not supported");
            }
        }

        private Expression VisitStringMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Format":
                    var formater = GetValue(StripQuotes(m.Arguments[0])).ToString();
                    var objs = new object[m.Arguments.Count - 1];
                    for (var i = 1; i < m.Arguments.Count; i++)
                    {
                        objs[i - 1] = GetValue(StripQuotes(m.Arguments[i])).ToString();
                    }
                    var formated = string.Format(formater, objs);
                    _compositeQuery.Write(formated);
                    return m;
                case "Contains":
                    if (m.Method.DeclaringType == typeof(string))
                    {
                        Visit(Expression.Equal(m.Object, Expression.Constant($"*{GetValue(m)}*")));
                    }
                    else
                        throw new NotSupportedException($"{m.Method.Name} support only string type");
                    return m;
                case "StartsWith":
                    if (m.Method.DeclaringType == typeof(string))
                    {
                        Visit(Expression.Equal(m.Object, Expression.Constant($"{GetValue(m)}*")));
                    }
                    else
                        throw new NotSupportedException($"{m.Method.Name} support only string type");
                    return m;
                case "EndsWith":
                    if (m.Method.DeclaringType == typeof(string))
                    {
                        Visit(Expression.Equal(m.Object, Expression.Constant($"*{GetValue(m)}")));
                    }
                    else
                        throw new NotSupportedException($"{m.Method.Name} support only string type");
                    return m;
                default:
                    throw new NotSupportedException(
                        $"'{m.Method.Name}' is not supported");
            }
        }

        private Expression VisitQueryableMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Where":
                case "First":
                case "FirstOrDefault":
                    _compositeQuery.Query.Add(new ExpressionParser(_itemType).Parse(((LambdaExpression)StripQuotes(m.Arguments[1])).Body).Translate());
                    Visit(StripQuotes(m.Arguments[0]));
                    return m;
                case "OrderBy":
                case "ThenBy":
                case "OrderByDescending":
                case "ThenByDescending":
                    _compositeQuery.OrderByList.Add(new OrderBy(GetValue(((LambdaExpression)StripQuotes(m.Arguments[1])).Body).ToString(), m.Method.Name));
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

        private Expression VisitItemMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "get_Item":
                    _compositeQuery.Write(GetValue(StripQuotes(m.Arguments[0])).ToString());
                    return m;

                default:
                    throw new NotSupportedException($"'{m.Method.Name}' is not supported");
            }
        }

        private Expression VisitExtensionMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Boost":
                    if (m.Arguments.Count == 2)
                    {
                        var term = new ExpressionParser(_itemType).Parse(StripQuotes(m.Arguments[0])).Translate();
                        var boost = GetValue(StripQuotes(m.Arguments[1]));
                        _compositeQuery.Write($"{term.ToSolrGroup()}^{boost}");
                    }
                    return m;
                case "ConstantScore":
                    if (m.Arguments.Count == 2)
                    {
                        var term = new ExpressionParser(_itemType).Parse(StripQuotes(m.Arguments[0])).Translate();
                        var score = GetValue(StripQuotes(m.Arguments[1]));
                        _compositeQuery.Write($"{term.ToSolrGroup()}^={score}");
                    }                    
                    return m;
                case "Fuzzy":
                    if (m.Arguments.Count == 3)
                    {
                        var field = GetValue(StripQuotes(m.Arguments[0]));
                        var value = GetValue(StripQuotes(m.Arguments[1])).ToSearchValue();
                        var distance = GetValue(StripQuotes(m.Arguments[2]));
                        _compositeQuery.Write($"{field}:{value}~{distance}");
                    }
                    else if (m.Arguments.Count == 2)
                    {
                        var field = GetValue(StripQuotes(m.Arguments[0]));
                        var value = GetValue(StripQuotes(m.Arguments[1])).ToSearchValue();                        
                        _compositeQuery.Write($"{field}:{value}~");
                    }
                    return m;
                case "Proximity":
                    if (m.Arguments.Count == 3)
                    {
                        var field = GetValue(StripQuotes(m.Arguments[0]));
                        var value = GetValue(StripQuotes(m.Arguments[1])).ToSearchValue();
                        var distance = GetValue(StripQuotes(m.Arguments[2]));
                        _compositeQuery.Write($"{field}:{value}~{distance}");
                    }
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
            if (m.Method.DeclaringType == typeof(string))
                return VisitStringMethod(m);
            if (m.Method.DeclaringType == _itemType || _itemType.IsSubclassOf(m.Method.DeclaringType) || m.Method.DeclaringType.IsInterface && _itemType.GetInterfaces().Any(x => x == m.Method.DeclaringType))
                return VisitItemMethod(m);
            if (m.Method.DeclaringType == typeof(MethodExtensions))
                return VisitExtensionMethod(m);

            throw new NotSupportedException($"'{m.Method.Name}' is not supported");
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            throw new NotSupportedException($"The unary operators is not supported");
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.NotEqual)
            {
                _compositeQuery.Write("-");
            }

            Visit(b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    _compositeQuery.Write(" AND ");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    _compositeQuery.Write(" OR ");
                    break;
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    _compositeQuery.Write(":");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _compositeQuery.Write(":[");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _compositeQuery.Write(":[* TO ");
                    break;
                case ExpressionType.GreaterThan:
                    _compositeQuery.Write(":{");
                    break;
                case ExpressionType.LessThan:
                    _compositeQuery.Write(":[* TO ");
                    break;
                default:
                    throw new NotSupportedException($"'{ b.Method.Name}' is not supported");
            }

            Visit(b.Right);

            switch (b.NodeType)
            {
                case ExpressionType.GreaterThanOrEqual:
                    _compositeQuery.Write(" TO *]");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _compositeQuery.Write("]");
                    break;
                case ExpressionType.GreaterThan:
                    _compositeQuery.Write(" TO *]");
                    break;
                case ExpressionType.LessThan:
                    _compositeQuery.Write("}");
                    break;
            }

            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            var q = c.Value as IQueryable;
            if (q == null)
            {
                if (c.Type == typeof(string))
                {
                    var str = c.Value.ToString();
                    _compositeQuery.Write(str.Contains(" ") ? $"\"{str}\"" : str);
                }
                if (c.Type == typeof(DateTime))
                    _compositeQuery.Write(((DateTime)c.Value).ToString("yyyy-MM-ddThh:mm:ss.fffZ"));
            }
            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression.NodeType == ExpressionType.Parameter)
            {
                _compositeQuery.Write(GetFieldName(m.Member));
                return m;
            }
            else if (m.Expression.NodeType == ExpressionType.MemberAccess || m.Expression.NodeType == ExpressionType.Constant || m.Expression.NodeType == ExpressionType.ArrayIndex)
            {
                if (m.Type == typeof(DateTime))
                    _compositeQuery.Write(((DateTime)GetValue(m)).ToString("yyyy-MM-ddThh:mm:ss.fffZ"));
                else
                    _compositeQuery.Write(GetValue(m).ToString());
                return m;
            }

            throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");
        }
    }
}