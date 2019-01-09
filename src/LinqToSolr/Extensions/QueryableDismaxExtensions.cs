using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SS.LinqToSolr.Extensions
{
    public static class QueryableDismaxExtensions
    {
        public static IQueryable<T> Dismax<T>(this IQueryable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));           
            return source.Provider.CreateQuery<T>(Expression.Call(null,
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
                new Expression[1]
                {
                    source.Expression
                }));
        }

        public static IQueryable<T> DismaxQuery<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
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

        /// <summary>
        /// q.alt
        /// </summary>        
        public static IQueryable<T> DismaxQueryAlt<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
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


        /// <summary>
        /// Exm: qf="fieldOne^2.3 fieldTwo fieldThree^0.4"
        /// </summary>
        public static IQueryable<T> QueryFields<T>(this IQueryable<T> source, Expression<Func<IQueryable<T>, IQueryable<T>>> keySelector)
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

        public static IQueryable<T> MinMatch<T, TKey>(this IQueryable<T> source, string value)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Provider.CreateQuery<T>(Expression.Call(null,
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T), typeof(TKey)),
                new Expression[2]
                {
                    source.Expression,
                    Expression.Constant(value)
                }));
        }

        /// <summary>
        /// pf
        /// </summary>        
        public static IQueryable<T> PhraseFields<T>(this IQueryable<T> source, Expression<Func<IQueryable<T>, IQueryable<T>>> keySelector)
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

        /// <summary>
        /// ps
        /// </summary>        
        public static IQueryable<T> PhraseSlop<T>(this IQueryable<T> source, Expression<Func<IQueryable<T>, IQueryable<T>>> keySelector)
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

        /// <summary>
        /// qs
        /// </summary>        
        public static IQueryable<T> QueryPhraseSlop<T>(this IQueryable<T> source, Expression<Func<IQueryable<T>, IQueryable<T>>> keySelector)
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

        /// <summary>
        /// tie
        /// </summary>        
        public static IQueryable<T> TieBreaker<T>(this IQueryable<T> source, Expression<Func<IQueryable<T>, IQueryable<T>>> keySelector)
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

        /// <summary>
        /// bq
        /// </summary>        
        public static IQueryable<T> BoostQuery<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
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

        /// <summary>
        /// bf
        /// </summary>        
        public static IQueryable<T> BoostFunctions<T>(this IQueryable<T> source, Expression<Func<IQueryable<T>, IQueryable<T>>> keySelector)
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
    }
}