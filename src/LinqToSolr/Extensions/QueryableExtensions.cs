using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SS.LinqToSolr.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, int page, int pageSize)
        {
            return Queryable.Take(Queryable.Skip(source, page * pageSize), pageSize);
        }

        public static IQueryable<TSource> Facet<TSource, TKey>(this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            return source.Provider.CreateQuery<TSource>(Expression.Call(null,
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)),
                new Expression[2]
                {
                    source.Expression,
                    Expression.Quote(keySelector)
                }));
        }

        public static IQueryable<TSource> Facet<TSource, TKey>(this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector, IEnumerable<object> selectedValues, bool isMultiFacet = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            return source.Provider.CreateQuery<TSource>(Expression.Call(null,
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)),
                new Expression[4]
                {
                    source.Expression,
                    Expression.Quote(keySelector),
                    Expression.Constant(selectedValues),
                    Expression.Constant(isMultiFacet)
                }));
        }

        public static IQueryable<TSource> PivotFacet<TSource>(this IQueryable<TSource> source, Expression<Func<IQueryable<TSource>, IQueryable<TSource>>> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            return source.Provider.CreateQuery<TSource>(Expression.Call(null,
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)),
                new Expression[2]
                {
                    source.Expression,
                    Expression.Quote(keySelector)
                }));
        }

        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), new Expression[2]
            {
                source.Expression,
                Expression.Quote( predicate)
            }));
        }

        public static IQueryable<TSource> Query<TSource>(this IQueryable<TSource> source, string term)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
     
            return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), new Expression[2]
            {
                source.Expression,
                Expression.Constant(term)
            }));
        }        
    }
}