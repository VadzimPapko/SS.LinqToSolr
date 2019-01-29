using System.Collections.Generic;

namespace SS.LinqToSolr.Models.Query
{
    public class FacetNode : IQueryNode
    {
        public FacetNode(IQueryNode field, List<string> values, bool isMultiFacet)
        {
            Field = field;
            Values = values;
            IsMultiFacet = isMultiFacet;
        }

        public IQueryNode Field { get; private set; }
        public List<string> Values { get; private set; }
        public bool IsMultiFacet { get; private set; }
    }
}
