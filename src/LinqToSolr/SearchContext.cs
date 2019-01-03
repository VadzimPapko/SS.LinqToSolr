using SS.LinqToSolr.Models;
using SS.LinqToSolr.Models.SearchResponse;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;

namespace SS.LinqToSolr
{
    public class SearchContext : ISearchContext, IDisposable
    {
        public string LastQuery { get; private set; }
        public string Handler { get; set; } = "select";
        public string Core { get; set; }

        private readonly HttpClient _client;

        public SearchContext(HttpClient client, string core)
        {
            Core = core;
            _client = client;
        }

        public IQueryable<T> GetQueryable<T>() where T : Document
        {
            return new SolrQueryProvider<T>(this).GetQueryable();
        }


        public Response<T> Search<T>(string query) where T : Document
        {
            LastQuery = query;

            var response = _client.GetAsync($"{Core}/{Handler}?{query}&wt=json").Result;

            var body = response.Content.ReadAsStringAsync().Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = JsonConvert.DeserializeObject<Response<T>>(body);
                return result;
            }

            return null;
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
                _client.Dispose();
            }

            _disposed = true;
        }
        #endregion
    }
}
