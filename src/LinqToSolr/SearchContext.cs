﻿using SS.LinqToSolr.Models.SearchResponse;
using System;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Collections.Generic;
using SS.LinqToSolr.Translators;

namespace SS.LinqToSolr
{
    public class SearchContext : ISearchContext, IDisposable
    {
        public string LastQuery { get; private set; }
        public string Handler { get; set; } = "select";
        public string Core { get; set; }

        protected readonly HttpClient _client;
        protected INodeTranslator _nodeTranslator;
        protected IResposeTranslator _resposeTranslator;

        public SearchContext(HttpClient client, string core, INodeTranslator nodeTranslator = null, IResposeTranslator resposeTranslator = null)
        {
            Core = core;
            _client = client;

            if (nodeTranslator == null)
                _nodeTranslator = new NodeTranslator(new NewtonsoftJsonFieldTranslator());
            else
                _nodeTranslator = nodeTranslator;

            if (resposeTranslator == null)
                _resposeTranslator = new NewtonsoftJsonResposeTranslator();
            else
                _resposeTranslator = resposeTranslator;
        }

        public virtual IQueryable<T> GetQueryable<T>()
        {
            return new SolrQueryProvider<T>(this, _nodeTranslator).GetQueryable();
        }

        public virtual Response<T> Search<T>(string query)
        {
            query = $"{query}&fl=*,score&wt=json";
            LastQuery = query;

            var httpMessage = TranslateQueryToPostMessage(query);
            var response = _client.PostAsync($"{Core}/{Handler}", httpMessage).Result;

            var body = response.Content.ReadAsStringAsync().Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return _resposeTranslator.Translate<T>(body);
            }

            return null;
        }

        protected virtual FormUrlEncodedContent TranslateQueryToPostMessage(string query)
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
