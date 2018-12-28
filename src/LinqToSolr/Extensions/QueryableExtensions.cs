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

        ///https://lucene.apache.org/solr/guide/6_6/the-standard-query-parser.html
        ///http://www.solrtutorial.com/solr-query-syntax.html
        ///Proximity matching "foo bar"~4
        ///Range searches mod_date:[20020101 TO 20030101]
        ///Boosts (title:foo OR title:bar)^1.5 (body:foo OR body:bar)
        ///A * may be used for either or both endpoints to specify an open-ended range query.

        //        field:[* TO 100] finds all field values less than or equal to 100

        //field:[100 TO *] finds all field values greater than or equal to 100

        //field:[* TO *] matches all documents with the field

        //Pure negative queries(all clauses prohibited) are allowed.
        //-inStock:false finds all field values where inStock is not false

        //-field:[* TO *] finds all documents without a value for field

        //A hook into FunctionQuery syntax. Quotes will be necessary to encapsulate the function when it includes parentheses.


        //Example: _val_:myfield

        //Example: _val_:"recip(rord(myfield),1,2,3)"


        //Nested query support for any type of query parser (via QParserPlugin). Quotes will often be necessary to encapsulate the nested query if it contains reserved characters.
        //Example: _query_:"{!dismax qf=myfield}how now brown cow"
    }
}