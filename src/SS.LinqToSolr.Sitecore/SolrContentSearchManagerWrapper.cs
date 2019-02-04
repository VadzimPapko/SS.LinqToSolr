using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SolrProvider;
using System.Collections.Concurrent;

namespace SS.LinqToSolr.Sitecore
{
    public static class SolrContentSearchManagerWrapper
    {
        private static ConcurrentDictionary<string, ISearchIndex> _indexes = new ConcurrentDictionary<string, ISearchIndex>();

        public static ISearchIndex GetIndex(string name)
        {
            return CustomSolrIndex(name);
        }

        public static ISearchIndex GetIndex(IIndexable indexable)
        {
            return CustomSolrIndex(ContentSearchManager.GetContextIndexName(indexable));
        }

        private static ISearchIndex CustomSolrIndex(string name)
        {
            return _indexes.GetOrAdd(name, (x) =>
            {
                var searchIndex = ContentSearchManager.GetIndex(name) as SolrSearchIndex;

                var customSolrIndex = new SSSolrSearchIndex(searchIndex.Name, searchIndex.Core, searchIndex.PropertyStore)
                {
                    Configuration = searchIndex.Configuration
                };
                customSolrIndex.Initialize();

                return customSolrIndex;
            });
        }
    }
}
