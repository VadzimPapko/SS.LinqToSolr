using SS.LinqToSolr.Models.SearchResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SS.LinqToSolr.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Page<T>(this IQueryable<T> source, int page, int pageSize)
        {
            return Queryable.Take(Queryable.Skip(source, page * pageSize), pageSize);
        }

        public static IQueryable<T> Facet<T, TKey>(this IQueryable<T> source,
            Expression<Func<T, TKey>> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            return source.Provider.CreateQuery<T>(Expression.Call(null,
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T), typeof(TKey)),
                new Expression[2]
                {
                    source.Expression,
                    Expression.Quote(keySelector)
                }));
        }

        public static IQueryable<T> Facet<T, TKey>(this IQueryable<T> source,
            Expression<Func<T, TKey>> keySelector, IEnumerable<object> selectedValues, bool isMultiFacet = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            return source.Provider.CreateQuery<T>(Expression.Call(null,
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T), typeof(TKey)),
                new Expression[4]
                {
                    source.Expression,
                    Expression.Quote(keySelector),
                    Expression.Constant(selectedValues),
                    Expression.Constant(isMultiFacet)
                }));
        }

        public static IQueryable<T> PivotFacet<T>(this IQueryable<T> source, Expression<Func<IQueryable<T>, IQueryable<T>>> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            return source.Provider.CreateQuery<T>(Expression.Call(null,
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
                new Expression[2]
                {
                    source.Expression,
                    Expression.Quote(keySelector)
                }));
        }

        public static IQueryable<T> Filter<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            return source.Provider.CreateQuery<T>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T)), new Expression[2]
            {
                source.Expression,
                Expression.Quote( predicate)
            }));
        }

        public static IQueryable<T> Query<T>(this IQueryable<T> source, string term)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.CreateQuery<T>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T)), new Expression[2]
            {
                source.Expression,
                Expression.Constant(term)
            }));
        }

        public static Response<T> GetResponse<T>(this IQueryable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.Execute<Response<T>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T)), new Expression[1]
            {
                source.Expression
            }));
        }
    }
}