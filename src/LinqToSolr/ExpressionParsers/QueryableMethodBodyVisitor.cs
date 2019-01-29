using SS.LinqToSolr.Extensions;
using SS.LinqToSolr.Models.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SS.LinqToSolr.ExpressionParsers
{
    public class QueryableMethodBodyVisitor : ExpressionVisitor
    {
        protected Type _itemType;

        public QueryableMethodBodyVisitor(Type itemType)
        {
            _itemType = itemType;
        }

        protected override QueryNode VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(string))
                return VisitStringMethod(m);
            if (m.Method.DeclaringType == _itemType || _itemType.IsSubclassOf(m.Method.DeclaringType) || m.Method.DeclaringType.IsInterface && _itemType.GetInterfaces().Any(x => x == m.Method.DeclaringType))
                return VisitItemMethod(m);
            if (m.Method.DeclaringType == typeof(MethodExtensions))
                return VisitExtensionMethod(m);

            throw new NotSupportedException($"{m.Method.DeclaringType} '{m.Method.Name}' is not supported");
        }
        protected virtual QueryNode VisitStringMethod(MethodCallExpression m)
        {
            var node = new MethodNode(m.Method.Name, m.Type.DeclaringType);
            switch (m.Method.Name)
            {
                case "Format":
                    var formater = Visit(m.Arguments[0]);
                    var objs = new object[m.Arguments.Count - 1];
                    for (var i = 1; i < m.Arguments.Count; i++)
                    {
                        objs[i - 1] = ((ConstantNode)Visit(m.Arguments[i])).Value;
                    }
                    var formated = string.Format(((ConstantNode)formater).Value.ToString(), objs);
                    node.Body = new ConstantNode(typeof(string), formated);
                    return node;
                case "Contains":
                    node.Body = Visit(Expression.Equal(m.Object, Expression.Constant($"*{((ConstantNode)Visit(m.Arguments[0])).Value}*")));
                    return node;
                case "StartsWith":
                    node.Body = Visit(Expression.Equal(m.Object, Expression.Constant($"{((ConstantNode)Visit(m.Arguments[0])).Value}*")));
                    return node;
                case "EndsWith":
                    node.Body = Visit(Expression.Equal(m.Object, Expression.Constant($"*{((ConstantNode)Visit(m.Arguments[0])).Value}")));
                    return node;
                default:
                    throw new NotSupportedException($"'{m.Method.Name}' is not supported");
            }
        }

        protected virtual QueryNode VisitExtensionMethod(MethodCallExpression m)
        {
            var node = new MethodNode(m.Method.Name, m.Type.DeclaringType);
            switch (m.Method.Name)
            {
                case "Boost":
                    if (m.Arguments.Count == 2)
                    {                       
                        node.Body = Visit(m.Arguments[0]);
                        node.SubBody = new ConstantNode(typeof(string), $"^{((ConstantNode)Visit(m.Arguments[1])).Value}");
                    }
                    return node;
                case "ConstantScore":
                    if (m.Arguments.Count == 2)
                    {
                        node.Body = Visit(m.Arguments[0]);
                        node.SubBody = new ConstantNode(typeof(string), $"^={((ConstantNode)Visit(m.Arguments[1])).Value}");                        
                    }
                    return node;
                case "Fuzzy":
                    if (m.Arguments.Count == 3)
                    {                        
                        node.Body = Visit(Expression.Equal(m.Arguments[0], m.Arguments[1]));
                        node.SubBody = new ConstantNode(typeof(string), $"~{((ConstantNode)Visit(m.Arguments[2])).Value}");
                    }
                    else if (m.Arguments.Count == 2)
                    {
                        node.Body = Visit(Expression.Equal(m.Arguments[0], m.Arguments[1]));
                        node.SubBody = new ConstantNode(typeof(string), $"~");
                    }
                    return node;
                case "Proximity":
                    if (m.Arguments.Count == 3)
                    {
                        node.Body = Visit(Expression.Equal(m.Arguments[0], m.Arguments[1]));
                        node.SubBody = new ConstantNode(typeof(string), $"~{((ConstantNode)Visit(m.Arguments[2])).Value}");
                    }
                    return node;
                default:
                    throw new NotSupportedException($"'{m.Method.Name}' is not supported");
            }
        }

        protected virtual QueryNode VisitItemMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "get_Item":
                    return Visit(m.Arguments[0]);
                default:
                    throw new NotSupportedException($"'{m.Method.Name}' is not supported");
            }
        }
        protected override QueryNode VisitBinary(BinaryExpression exp)
        {
            return new BinaryNode(Visit(exp.Left), Visit(exp.Right), exp.NodeType);
        }

        protected override QueryNode VisitConstant(ConstantExpression c)
        {
            var q = c.Value as IQueryable;
            if (q == null)
            {
                return new ConstantNode(c.Type, c.Value);
            }
            return null;
        }

        protected override QueryNode VisitIndex(IndexExpression exp)
        {
            throw new NotImplementedException();
        }

        protected override QueryNode VisitInvocation(InvocationExpression node)
        {
            return Visit(node.Expression);
        }

        protected override QueryNode VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression.NodeType == ExpressionType.Parameter)
            {
                return new MemberNode(m.Member);
            }
            else if (m.Expression.NodeType == ExpressionType.Constant)
            {
                return new ConstantNode(m.Type, GetMemberValue(m, ((ConstantExpression)m.Expression).Value));
            }
            else if (m.Expression.NodeType == ExpressionType.MemberAccess)
            {
                return VisitMemberAccess((MemberExpression)m.Expression, m);
            }
            else if (m.Expression.NodeType == ExpressionType.ArrayIndex)
            {
                var be = (BinaryExpression)m.Expression;
                var left = (ConstantNode)Visit(be.Left);
                var right = (ConstantNode)Visit(be.Right);
                var obj = ((IList<object>)left.Value)[(int)right.Value];
                return new ConstantNode(m.Type, GetMemberValue(m, obj));
            }
            throw new NotSupportedException($"The member '{m.Expression.NodeType}' is not supported");
        }

        protected virtual QueryNode VisitMemberAccess(MemberExpression m, MemberExpression parent)
        {
            if (m.Expression.NodeType == ExpressionType.MemberAccess)
            {
                var val = (ConstantNode)VisitMemberAccess((MemberExpression)m.Expression, m);
                return new ConstantNode(parent.Type, GetMemberValue(m, val.Value));
            }
            else if (m.Expression.NodeType == ExpressionType.Constant)
            {
                var val = GetMemberValue(m, ((ConstantExpression)m.Expression).Value);
                return new ConstantNode(parent.Type, GetMemberValue(parent, val));
            }
            throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");
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

        protected override QueryNode VisitNew(NewExpression exp)
        {
            throw new NotImplementedException();
        }

        protected override QueryNode VisitParameter(ParameterExpression p)
        {
            throw new NotSupportedException($"The '{p.NodeType}' is not supported");
        }

        protected override QueryNode VisitUnary(UnaryExpression exp)
        {
            throw new NotImplementedException();
        }

        protected override QueryNode VisitQuote(Expression exp)
        {
            while (exp.NodeType == ExpressionType.Quote)
            {
                exp = ((UnaryExpression)exp).Operand;
            }
            return Visit(exp);
        }

        protected override QueryNode VisitLambda(LambdaExpression exp)
        {
            return Visit(exp.Body);
        }
    }
}