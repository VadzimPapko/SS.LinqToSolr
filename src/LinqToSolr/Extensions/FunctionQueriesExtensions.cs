namespace SS.LinqToSolr.Extensions
{
    public static class FunctionQueriesExtensions
    {
        /// <summary>
        /// Returns the absolute value of the specified value or function.     
        /// </summary>
        public static T ABS<T>(this T value, float boost)
        {
            return value;
        }

        /// <summary>
        /// Returns the value of the given field for one of the matched child docs when searching by {!parent}. It can be used only in sort parameter.    
        /// </summary>
        public static T ChildField<T>(this T value, float boost)
        {
            return value;
        }

        /// <summary>
        /// def is short for default. Returns the value of field "field", or if the field does not exist, returns the default value specified. and yields the first value where exists()==true.)
        /// </summary>
        public static T Def<T>(this T value, float boost)
        {
            return value;
        }

        /// <summary>
        /// Divides one value or function by another. div(x,y) divides x by y.
        /// </summary>
        public static T Div<T>(this T value, float boost)
        {
            return value;
        }

        /// <summary>
        /// Return the distance between two vectors (points) in an n-dimensional space. 
        /// </summary>
        public static T Dist<T>(this T value, float boost)
        {
            return value;
        }

        /// <summary>
        /// Returns the number of documents that contain the term in the field. This is a constant (the same value for all documents in the index).
        /// </summary>
        public static T DocFreq<T>(this T value, float boost)
        {
            return value;
        }

        /// <summary>
        /// Returns the numeric docValues or indexed value of the field with the specified name.
        /// </summary>
        public static T Field<T>(this T value, float boost)
        {
            return value;
        }

        /// <summary>
        /// The Haversine distance calculates the distance between two points on a sphere when traveling along the sphere. 
        /// </summary>
        public static T Hsin<T>(this T value, float boost)
        {
            return value;
        }

        /// <summary>
        /// Inverse document frequency
        /// </summary>
        public static T Idf<T>(this T value, float boost)
        {
            return value;
        }
    }
}
