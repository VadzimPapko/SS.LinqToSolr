using System;

namespace SS.LinqToSolr.Models.Query
{
    public class ConstantNode : QueryNode
    {
        public Type Type { get; private set; }
        public object Value { get; private set; }
        public ConstantNode(Type type, object value)
        {
            Type = type;
            Value = value;
        }
    }
}
