using System.Linq.Expressions;

namespace SS.LinqToSolr.Models.Query
{
    public class BinaryNode : QueryNode
    {
        public QueryNode LeftNode { get; private set; }
        public QueryNode RightNode { get; private set; }
        public ExpressionType Type { get; private set; }

        public BinaryNode(QueryNode leftNode, QueryNode rightNode, ExpressionType type)
        {
            LeftNode = leftNode;
            RightNode = rightNode;
            Type = type;
        }
    }
}
