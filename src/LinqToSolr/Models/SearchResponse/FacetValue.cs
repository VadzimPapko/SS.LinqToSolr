namespace SS.LinqToSolr.Models.SearchResponse
{
    public class FacetValue: IFacet
    {
        public string Value { get; set; }
        public int Count { get; set; }
    }
}