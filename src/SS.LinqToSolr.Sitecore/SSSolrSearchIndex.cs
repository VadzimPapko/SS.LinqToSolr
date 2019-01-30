using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.SolrProvider;

namespace SS.LinqToSolr.Sitecore
{
    public class SSSolrSearchIndex : SolrSearchIndex
    {
        public SSSolrSearchIndex(string name, string core, IIndexPropertyStore propertyStore) : base(name, core, propertyStore)
        {
        }

        public SSSolrSearchIndex(string name, string core, IIndexPropertyStore propertyStore, string group) : base(name, core, propertyStore, group)
        {
        }

        public override IProviderSearchContext CreateSearchContext(SearchSecurityOptions options = SearchSecurityOptions.Default)
        {
            return new SSSolrSearchContext(this, options);
        }
    }
}
