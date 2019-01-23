using SS.LinqToSolr.Models.Query;
using System;
using System.Linq.Expressions;

namespace SS.LinqToSolr.ExpressionParsers
{
    public abstract class ExpressionVisitor
    {
        public virtual QueryNode Parse(Expression expression)
        {
            return Visit(expression);
        }

        protected virtual QueryNode Visit(Expression exp)
        {
            switch (exp.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AndAlso:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Multiply:
                case ExpressionType.NotEqual:
                case ExpressionType.OrElse:
                case ExpressionType.Subtract:
                    return VisitBinary((BinaryExpression)exp);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)exp);
                case ExpressionType.Convert:
                case ExpressionType.Negate:
                case ExpressionType.Not:
                    return VisitUnary((UnaryExpression)exp);
                case ExpressionType.Invoke:
                    return VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)exp);
                case ExpressionType.New:
                    return VisitNew((NewExpression)exp);
                case ExpressionType.Index:
                    return VisitIndex((IndexExpression)exp);
                case ExpressionType.Quote:
                    return VisitQuote(exp);
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)exp);
                default:
                    throw new NotSupportedException($"'{exp.NodeType}' is not supported");
            }
        }

        protected abstract QueryNode VisitLambda(LambdaExpression exp);
        protected abstract QueryNode VisitQuote(Expression exp);

        protected abstract QueryNode VisitIndex(IndexExpression exp);

        protected abstract QueryNode VisitNew(NewExpression exp);

        protected abstract QueryNode VisitParameter(ParameterExpression exp);

        protected abstract QueryNode VisitMemberAccess(MemberExpression exp);

        protected abstract QueryNode VisitInvocation(InvocationExpression exp);

        protected abstract QueryNode VisitUnary(UnaryExpression exp);

        protected abstract QueryNode VisitConstant(ConstantExpression exp);

        protected abstract QueryNode VisitMethodCall(MethodCallExpression exp);

        protected abstract QueryNode VisitBinary(BinaryExpression exp);
    }
}