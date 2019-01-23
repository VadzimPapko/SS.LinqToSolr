namespace SS.LinqToSolr.Models.Query
{
    public class MethodNode : QueryNode
    {
        public string Name { get; private set; }
        public MethodNode(string name)
        {
            Name = name;
        }
        public QueryNode Body { get; set; }
    }
}
