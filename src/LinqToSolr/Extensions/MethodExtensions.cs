using System;

namespace SS.LinqToSolr.Extensions
{
    public static class MethodExtensions
    {
        /// <summary>
        /// Boosting allows you to control the relevance of a document by boosting its term
        /// <para>By default, the boost factor is 1. Although the boost factor must be positive, it can be less than 1 (for example, it could be 0.2)</para>
        /// </summary>
        public static T Boost<T>(this T value, float boost)
        {
            if (boost < 0)
                throw new ArgumentException("boost can not be less than 0");
            return value;
        }

        /// <summary>
        /// Constant score queries are created with ^=, which sets the entire clause to the specified score for any documents matching that clause.
        /// <para>This is desirable when you only care about matches for a particular clause and don’t want other relevancy factors such as term frequency (the number of times the term appears in the field) or inverse document frequency (a measure across the whole index for how rare a term is in a field).</para>
        /// </summary>        
        public static T ConstantScore<T>(this T value, float score)
        {
            if (score < 0)
                throw new ArgumentException("boost can not be less than 0");
            return value;
        }

        /// <summary>
        /// Solr’s standard query parser supports fuzzy searches based on the Damerau-Levenshtein Distance or Edit Distance algorithm.        
        /// <para>An optional distance parameter specifies the maximum number of edits allowed, between 0 and 2, defaulting to 2</para>  
        /// </summary>        
        public static bool Fuzzy(this string field, string word)
        {
            if (word.IsPhrase())
                throw new ArgumentException("string must be word");

            return false;
        }

        /// <summary>
        /// Solr’s standard query parser supports fuzzy searches based on the Damerau-Levenshtein Distance or Edit Distance algorithm.        
        /// <para>An optional distance parameter specifies the maximum number of edits allowed, between 0 and 2, defaulting to 2</para>  
        /// </summary>
        public static bool Fuzzy(this string field, string word, float distance = 2)
        {
            return false;
        }

        /// <summary>
        /// Solr’s standard query parser supports fuzzy searches based on the Damerau-Levenshtein Distance or Edit Distance algorithm.        
        /// <para>An optional distance parameter specifies the maximum number of edits allowed, between 0 and 2, defaulting to 2</para>  
        /// </summary>
        public static string Fuzzy(this string word, float distance = 2)
        {
            if (word.IsPhrase())
                throw new ArgumentException("string must be word");
            if (distance > 2)
                throw new ArgumentException("distance can not be more than 2");
            if (distance < 0)
                throw new ArgumentException("distance can not be less than 0");
            return $"{word}~{(distance == 2 ? "" : distance.ToString())}";
        }

        /// <summary>
        /// A proximity search looks for terms that are within a specific distance from one another.
        /// <para>For example, to search for a "apache" and "jakarta" within 10 words of each other in a document, use the search:</para>
        /// <para>"jakarta apache"~10</para>
        /// </summary>
        public static bool Proximity(this string field, string phrase, int distance)
        {
            return false;
        }

        /// <summary>
        /// A proximity search looks for terms that are within a specific distance from one another.
        /// <para>For example, to search for a "apache" and "jakarta" within 10 words of each other in a document, use the search:</para>
        /// <para>"jakarta apache"~10</para>
        /// </summary>
        public static string Proximity(this string phrase, int distance)
        {
            if (!phrase.IsPhrase())
                throw new ArgumentException("string must be phrase");
            if (distance < 0)
                throw new ArgumentException("distance can not be less than 0");
            return $"{phrase.ToSearchValue()}~{distance}";
        }

        public static bool Wildcard(this string str, float comparison)
        {
            return false;
        }

        internal static bool IsPhrase(this string str)
        {
            return str.Contains(" ");
        }

        internal static string ToSearchValue(this string str)
        {
            if (str.IsPhrase())
                return $"\"{str}\"";
            return str;
        }

        internal static string ToSearchValue(this object str)
        {
            return str.ToString().ToSearchValue();
        }

        internal static string ToSolrGroup(this string str)
        {
            return $"({str})";
        }
    }
}
