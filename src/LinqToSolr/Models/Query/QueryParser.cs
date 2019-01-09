namespace SS.LinqToSolr.Models.Query
{
    public sealed class QueryParser
    {
        private readonly string name;

        public static readonly QueryParser Default = new QueryParser("lucene");
        public static readonly QueryParser Dismax = new QueryParser("dismax");
        public static readonly QueryParser EDismax = new QueryParser("edismax");

        private QueryParser(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
