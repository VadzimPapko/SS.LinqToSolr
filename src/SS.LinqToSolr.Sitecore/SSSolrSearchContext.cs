using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Abstractions;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.SolrProvider;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Diagnostics;
using SS.LinqToSolr.Sitecore.Translators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SS.LinqToSolr.Sitecore
{
    public class SSSolrSearchContext : IProviderSearchContext, IDisposable
    {
        protected SearchContext _api;

        private readonly SolrSearchIndex index;
        private readonly SearchSecurityOptions securityOptions;
        private readonly IContentSearchConfigurationSettings contentSearchSettings;
        private ISettings settings;
        private bool? convertQueryDatesToUtc;

        public SSSolrSearchContext(SSSolrSearchIndex index, SearchSecurityOptions options = SearchSecurityOptions.Default)
        {
            Assert.ArgumentNotNull(index, nameof(index));
            Assert.ArgumentNotNull(options, nameof(options));
            if (options == SearchSecurityOptions.Default)
                options = index.Configuration.DefaultSearchSecurityOption;
            this.index = index;
            contentSearchSettings = index.Locator.GetInstance<IContentSearchConfigurationSettings>();
            settings = index.Locator.GetInstance<ISettings>();
            securityOptions = options;

            var solrUrl = $"{SolrContentSearchManager.SolrSettings.ServiceAddress().TrimEnd('/')}/";

            _api = new SearchContext(new System.Net.Http.HttpClient
            {
                BaseAddress = new Uri(solrUrl)
            }, index.Core, new SitecoreNodeTranslator(new FieldTranslator(index.FieldNameTranslator)), new ResposeTranslator(index.FieldNameTranslator));            
        }

        public ISearchIndex Index { get; private set; }
        public SearchSecurityOptions SecurityOptions { get; private set; }
        public bool ConvertQueryDatesToUtc
        {
            get
            {
                if (convertQueryDatesToUtc.HasValue)
                    return convertQueryDatesToUtc.Value;
                return contentSearchSettings.ConvertQueryDatesToUtc;
            }
            set
            {
                convertQueryDatesToUtc = new bool?(value);
            }
        }

        public IQueryable<T> GetQueryable<T>()
        {
            return GetQueryable<T>(new IExecutionContext[0]);
        }

        public IQueryable<T> GetQueryable<T>(IExecutionContext executionContext)
        {
            return GetQueryable<T>(new IExecutionContext[1]
            {
                executionContext
            });
        }

        public virtual IQueryable<T> GetQueryable<T>(params IExecutionContext[] executionContexts)
        {
            return _api.GetQueryable<T>();
        }

        public IEnumerable<SearchIndexTerm> GetTermsByFieldName(string fieldName, string filter)
        {
            SolrSearchFieldConfiguration fieldConfiguration = index.Configuration.FieldMap.GetFieldConfiguration(fieldName.ToLowerInvariant()) as SolrSearchFieldConfiguration;
            if (fieldConfiguration != null)
                fieldName = fieldConfiguration.FormatFieldName(fieldName, Index.Schema, null, settings.DefaultLanguage());
            IEnumerable<SearchIndexTerm> searchIndexTerms = new HashSet<SearchIndexTerm>();
            return searchIndexTerms;
        }

        #region Disposing
        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _api.Dispose();
            }

            _disposed = true;
        }
        #endregion
    }
}
