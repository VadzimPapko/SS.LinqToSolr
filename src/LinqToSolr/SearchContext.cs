using SS.LinqToSolr.Models;
using SS.LinqToSolr.Models.SearchResponse;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;

namespace SS.LinqToSolr
{
    public class SearchContext : ISolrService, IDisposable
    {
        public string LastQuery { get; private set; }

        private readonly HttpClient _client;
        private readonly string _core;
        private readonly string _handler;

        public SearchContext(string url, string core, string handler = "select")
        {
            _core = core;
            _handler = handler;

            _client = new HttpClient
            {
                BaseAddress = new Uri(url)
            };
        }

        public IQueryable<T> GetQueryable<T>() where T : Document
        {
            return new SolrQueryProvider<T>(this).GetQueryable();
        }


        public Response<T> Search<T>(string query) where T : Document
        {
            LastQuery = query;
            try
            {
                query = query + "&wt=json";
                var response = _client.GetAsync($"{_core}/{_handler}?{query}").Result;

                var body = response.Content.ReadAsStringAsync().Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<Response<T>>(body);
                    return result;
                }

                return null;
            }
            catch (Exception exp)
            {
                return null;
            }
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
