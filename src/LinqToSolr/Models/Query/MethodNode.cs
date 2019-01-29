using System;

namespace SS.LinqToSolr.Models.Query
{
    public class MethodNode : IQueryNode
    {
        public string Name { get; private set; }
        public MethodNode(string name, Type declaringType)
        {
            Name = name;
            DeclaringType = declaringType;
        }
        public Type DeclaringType { get; private set; }
        public IQueryNode Body { get; set; }
        public IQueryNode SubBody { get; set; }
    }
}
