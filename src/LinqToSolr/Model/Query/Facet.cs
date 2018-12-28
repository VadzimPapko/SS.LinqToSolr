using System.Collections.Generic;

namespace SS.LinqToSolr.Model.Query
{
    public class Facet
    {
        public Facet(string field, List<string> values, bool isMultiFacet)
        {
            Field = field;
            Values = values;
            IsMultiFacet = isMultiFacet;
        }

        public string Field { get; private set; }
        public List<string> Values { get; private set; }
        public bool IsMultiFacet { get; private set; }
    }
}
