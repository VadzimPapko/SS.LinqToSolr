namespace SS.LinqToSolr.Models.Query
{
    public class OrderBy
    {
        public OrderBy(string field, string methodName)
        {
            Field = field;
            Direction = methodName.EndsWith("Descending") ? "desc" : "asc";
        }

        public string Field { get; private set; }
        public string Direction { get; private set; }

        public string Translate()
        {
            return $"{Field} {Direction}";
        }
    }
}
