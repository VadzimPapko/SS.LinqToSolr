using System.Collections.Generic;

namespace SS.LinqToSolr.Models.Query
{
    public class FacetNode : QueryNode
    {
        public FacetNode(QueryNode field, List<string> values, bool isMultiFacet)
        {
            Field = field;
            Values = values;
            IsMultiFacet = isMultiFacet;
        }

        public QueryNode Field { get; private set; }
        public List<string> Values { get; private set; }
        public bool IsMultiFacet { get; private set; }
    }
}
