using SS.LinqToSolr.Models;
using SS.LinqToSolr.Models.SearchResponse;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace SS.LinqToSolr
{
    public interface ISolrService
    {
        Response<T> Search<T>(string query) where T : Document;
        IEnumerable<T> SearchForIQueryable<T>(string query) where T : Document;
        IQueryable<T> GetContext<T>() where T : Document;
    }

    public class SolrService : ISolrService, IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _core;
        private readonly string _handler;

        public SolrService(string url, string core, string handler = "select")
        {
            _core = core;
            _handler = handler;

            _client = new HttpClient
            {
                BaseAddress = new Uri(url)
            };
            //_client.DefaultRequestHeaders.Add(_Context.SSOHeader, _Context.User);
        }

        public IQueryable<T> GetContext<T>() where T : Document
        {
            return new SolrQueryProvider<T>(this).CreateContext();
        }

        public IEnumerable<T> SearchForIQueryable<T>(string query) where T : Document
        {
            return Search<T>(query)?.ResponseNode.Documents;
        }

        public Response<T> Search<T>(string query) where T : Document
        {
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
