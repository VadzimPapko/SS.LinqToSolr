using System.Collections.Generic;

namespace SS.LinqToSolr.Models.Query
{
    public class PivotFacet
    {
        public PivotFacet(List<string> fields)
        {
            Fields = fields;
        }

        public List<string> Fields { get; private set; }
    }
}
