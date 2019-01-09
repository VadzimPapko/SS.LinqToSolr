using SS.LinqToSolr.Extensions;
using SS.LinqToSolr.Models.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using SS.LinqToSolr.Translators;

namespace SS.LinqToSolr.ExpressionParsers
{
    public class NodeExpressionParser : ExpressionVisitor
    {
        protected Type _itemType;
        protected string _value;
        protected IFieldTranslator _fieldTranslator;

        public NodeExpressionParser(Type itemType, IFieldTranslator fieldTranslator)
        {
            _itemType = itemType;
            _fieldTranslator = fieldTranslator;
        }

        public virtual string Parse(Expression expression)
        {
            Visit(expression);
            return _value;
        }        

        protected virtual Expression VisitStringMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Format":
                    var formater = New().Parse(m.Arguments[0]);
                    var objs = new object[m.Arguments.Count - 1];
                    for (var i = 1; i < m.Arguments.Count; i++)
                    {
                        objs[i - 1] = New().Parse(m.Arguments[1]);
                    }
                    var formated = string.Format(formater, objs);
                    Write(formated);
                    return m;
                case "Contains":
                    Visit(Expression.Equal(m.Object, Expression.Constant($"*{New().Parse(m.Arguments[0])}*")));
                    return m;
                case "StartsWith":
                    Visit(Expression.Equal(m.Object, Expression.Constant($"{New().Parse(m.Arguments[0])}*")));
                    return m;
                case "EndsWith":
                    Visit(Expression.Equal(m.Object, Expression.Constant($"*{New().Parse(m.Arguments[0])}")));
                    return m;
                default:
                    throw new NotSupportedException($"'{m.Method.Name}' is not supported");
            }
        }

        protected virtual Expression VisitItemMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "get_Item":
                    Write(New().Parse(m.Arguments[0]));
                    return m;

                default:
                    throw new NotSupportedException($"'{m.Method.Name}' is not supported");
            }
        }

        protected virtual Expression VisitExtensionMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Boost":
                    if (m.Arguments.Count == 2)
                    {
                        var term = New().Parse(m.Arguments[0]);
                        var boost = New().Parse(m.Arguments[1]);
                        Write($"{term.ToSolrGroup()}^{boost}");
                    }
                    return m;
                case "ConstantScore":
                    if (m.Arguments.Count == 2)
                    {
                        var term = New().Parse(m.Arguments[0]);
                        var score = New().Parse(m.Arguments[1]);
                        Write($"{term.ToSolrGroup()}^={score}");
                    }
                    return m;
                case "Fuzzy":
                    if (m.Arguments.Count == 3)
                    {
                        var field = New().Parse(m.Arguments[0]);
                        var value = New().Parse(m.Arguments[1]).ToSearchValue();
                        var distance = New().Parse(m.Arguments[2]);
                        Write($"{field}:{value}~{distance}");
                    }
                    else if (m.Arguments.Count == 2)
                    {
                        var field = New().Parse(m.Arguments[0]);
                        var value = New().Parse(m.Arguments[1]).ToSearchValue();
                        Write($"{field}:{value}~");
                    }
                    return m;
                case "Proximity":
                    if (m.Arguments.Count == 3)
                    {
                        var field = New().Parse(m.Arguments[0]);
                        var value = New().Parse(m.Arguments[1]).ToSearchValue();
                        var distance = New().Parse(m.Arguments[2]);
                        Write($"{field}:{value}~{distance}");
                    }
                    return m;
                default:
                    throw new NotSupportedException($"'{m.Method.Name}' is not supported");
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(string))
                return VisitStringMethod(m);
            if (m.Method.DeclaringType == _itemType || _itemType.IsSubclassOf(m.Method.DeclaringType) || m.Method.DeclaringType.IsInterface && _itemType.GetInterfaces().Any(x => x == m.Method.DeclaringType))
                return VisitItemMethod(m);
            if (m.Method.DeclaringType == typeof(MethodExtensions))
                return VisitExtensionMethod(m);

            throw new NotSupportedException($"'{m.Method.Name}' is not supported");
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.ArrayIndex)
            {
                var left = GetValue(b.Left) as IList<object>;
                var right = (int)((ConstantExpression)b.Right).Value;
                var obj = left[right];
                Visit(Expression.Constant(obj));
                return b;
            }

            if (b.NodeType == ExpressionType.NotEqual)
            {
                Write("-");
            }

            Visit(b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    Write(" AND ");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    Write(" OR ");
                    break;
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    Write(":");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    Write(":[");
                    break;
                case ExpressionType.LessThanOrEqual:
                    Write(":[* TO ");
                    break;
                case ExpressionType.GreaterThan:
                    Write(":{");
                    break;
                case ExpressionType.LessThan:
                    Write(":[* TO ");
                    break;
                default:
                    throw new NotSupportedException($"'{ b.Method.Name}' is not supported");
            }

            Visit(b.Right);

            switch (b.NodeType)
            {
                case ExpressionType.GreaterThanOrEqual:
                    Write(" TO *]");
                    break;
                case ExpressionType.LessThanOrEqual:
                    Write("]");
                    break;
                case ExpressionType.GreaterThan:
                    Write(" TO *]");
                    break;
                case ExpressionType.LessThan:
                    Write("}");
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
                    Write(c.Value.ToString());
                }
                else if (c.Type == typeof(DateTime))
                {
                    Write(((DateTime)c.Value).ToString("yyyy-MM-ddThh:mm:ss.fffZ"));
                }
                else if (c.Type == typeof(float))
                {
                    Write(((float)c.Value).ToString());
                }
                else if (c.Type == typeof(int))
                {
                    Write(((int)c.Value).ToString());
                }
            }
            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression.NodeType == ExpressionType.Parameter)
            {
                Write(GetFieldName(m.Member));
                return m;
            }
            else if (m.Expression.NodeType == ExpressionType.MemberAccess || m.Expression.NodeType == ExpressionType.Constant || m.Expression.NodeType == ExpressionType.ArrayIndex)
            {
                if (m.Type == typeof(DateTime))
                    Write(((DateTime)GetValue(m)).ToString("yyyy-MM-ddThh:mm:ss.fffZ"));
                else
                    Write(GetValue(m).ToString());
                return m;
            }

            throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");
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

        protected virtual void Write(string val)
        {
            _value += val;
        }
    }
}