using System.Reflection;

namespace SS.LinqToSolr.Models.Query
{
    public class MemberNode : QueryNode
    {
        public MemberInfo Member { get; private set; }

        public MemberNode(MemberInfo member)
        {
            Member = member;
        }
    }
}
