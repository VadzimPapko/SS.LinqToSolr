using SS.LinqToSolr.Helpers;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SS.LinqToSolr
{
    public abstract class QueryProvider : IQueryProvider
    {
        protected QueryProvider()
        {
        }

        IQueryable<T> IQueryProvider.CreateQuery<T>(Expression expression)
        {
            return new Query<T>(this, expression);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);

            try
            {
                return (IQueryable) Activator.CreateInstance(typeof(Query<>).MakeGenericType(elementType),
                    new object[] {this, expression});
            }
            catch (TargetInvocationException exp)
            {
                throw exp.InnerException;
            }
        }

        T IQueryProvider.Execute<T>(Expression expression)
        {
            return (T) Execute(expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return Execute(expression);
        }

        public abstract string GetQueryText(Expression expression);
        public abstract object Execute(Expression expression);
    }
}