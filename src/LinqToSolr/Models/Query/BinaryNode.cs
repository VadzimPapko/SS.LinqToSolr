using System.Linq.Expressions;

namespace SS.LinqToSolr.Models.Query
{
    public class BinaryNode : IQueryNode
    {
        public IQueryNode LeftNode { get; private set; }
        public IQueryNode RightNode { get; private set; }
        public ExpressionType Type { get; private set; }

        public BinaryNode(IQueryNode leftNode, IQueryNode rightNode, ExpressionType type)
        {
            LeftNode = leftNode;
            RightNode = rightNode;
            Type = type;
        }
    }
}
