using System;
using System.Configuration;

namespace SS.LinqToSolr.Test
{
    public class BaseSolrTest : IDisposable
    {
        protected SearchContext _api;
        public BaseSolrTest(string core)
        {
            _api = new SearchContext(ConfigurationManager.AppSettings["Solr.Url"], core);
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
