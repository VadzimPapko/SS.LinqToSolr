using SS.LinqToSolr.Models;
using SS.LinqToSolr.Models.SearchResponse;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Collections.Generic;

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
            query = $"{query}&wt=json";
            LastQuery = query;

            var httpMessage = TranslateQueryToPostMessage(query);
            var response = _client.PostAsync($"{Core}/{Handler}", httpMessage).Result;

            var body = response.Content.ReadAsStringAsync().Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = JsonConvert.DeserializeObject<Response<T>>(body);
                return result;
            }

            return null;
        }

        private FormUrlEncodedContent TranslateQueryToPostMessage(string query)
        {
            var paramsList = new List<KeyValuePair<string, string>>();
            var parameters = HttpUtility.ParseQueryString(query);

            foreach (string key in parameters.Keys)
            {
                var values = parameters.GetValues(key);
                if (values == null) continue;

                paramsList.AddRange(values.Select(value => new KeyValuePair<string, string>(key, value)));
            }
            return new FormUrlEncodedContent(paramsList);
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
