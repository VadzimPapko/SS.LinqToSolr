using SS.LinqToSolr.Models.Query;
using System;
using System.Linq.Expressions;

namespace SS.LinqToSolr.ExpressionParsers
{
    public abstract class ExpressionVisitor
    {
        public virtual IQueryNode Parse(Expression expression)
        {
            return Visit(expression);
        }

        protected virtual IQueryNode Visit(Expression exp)
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

        protected abstract IQueryNode VisitLambda(LambdaExpression exp);
        protected abstract IQueryNode VisitQuote(Expression exp);

        protected abstract IQueryNode VisitIndex(IndexExpression exp);

        protected abstract IQueryNode VisitNew(NewExpression exp);

        protected abstract IQueryNode VisitParameter(ParameterExpression exp);

        protected abstract IQueryNode VisitMemberAccess(MemberExpression exp);

        protected abstract IQueryNode VisitInvocation(InvocationExpression exp);

        protected abstract IQueryNode VisitUnary(UnaryExpression exp);

        protected abstract IQueryNode VisitConstant(ConstantExpression exp);

        protected abstract IQueryNode VisitMethodCall(MethodCallExpression exp);

        protected abstract IQueryNode VisitBinary(BinaryExpression exp);
    }
}