using System.Collections.Generic;

namespace SS.LinqToSolr.Model.SearchResponse
{
    public class PivotFacetValue : IFacet
    {
        public PivotFacetValue()
        {
            Pivot = new List<PivotFacetValue>();
        }

        public string Field { get; set; }
        public string Value { get; set; }
        public int Count { get; set; }

        public List<PivotFacetValue> Pivot { get; set; }
    }
}