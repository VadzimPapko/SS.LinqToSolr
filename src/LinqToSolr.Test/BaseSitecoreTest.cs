using SS.LinqToSolr.Sitecore.Translators;
using SS.LinqToSolr.Translators;
using System;
using System.Configuration;

namespace SS.LinqToSolr.Test
{
    public class BaseSitecoreTest : IDisposable
    {
        protected SearchContext _api;
        public BaseSitecoreTest(string core)
        {
            _api = new SearchContext(new System.Net.Http.HttpClient
            {
                BaseAddress = new Uri(ConfigurationManager.AppSettings["Solr.Url"])
            }, core, new SitecoreNodeTranslator(new NewtonsoftJsonFieldTranslator()));
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
