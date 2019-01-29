using System;

namespace SS.LinqToSolr.Models.Query
{
    public class MethodNode : QueryNode
    {
        public string Name { get; private set; }
        public MethodNode(string name, Type declaringType)
        {
            Name = name;
            DeclaringType = declaringType;
        }
        public Type DeclaringType { get; private set; }
        public QueryNode Body { get; set; }
        public QueryNode SubBody { get; set; }
    }
}
