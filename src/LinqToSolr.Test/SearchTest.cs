using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using SS.LinqToSolr.Extensions;
using System.Configuration;
using System;

namespace SS.LinqToSolr.Test
{
    [TestClass]
    public class SearchTest : IDisposable
    {
        private SolrService _api;
        public SearchTest()
        {
            _api = new SolrService(ConfigurationManager.AppSettings["Solr.Url"], "sitecore_signals");
        }

        [TestMethod]
        public void EmptyQuery()
        {
            var emptyQuery = _api.GetContext<TestDocument>().ToString();
            Assert.AreEqual(emptyQuery, "q=*:*");
        }

        [TestMethod]
        public void FormatQuery()
        {
            var test = "test";
            var formatQuery = _api.GetContext<TestDocument>().Where(x => x.Title == $"{test}").ToString();
            Assert.AreEqual(formatQuery, "q=(title_s:test)");
        }

        [TestMethod]
        public void PlainQuery()
        {
            var test = "test";
            var plainQuery = _api.GetContext<TestDocument>().Query(test).ToString();
            Assert.AreEqual(plainQuery, "q=(test)");
        }

        [TestMethod]
        public void WhereEqualsQuery()
        {
            var whereEqualsQuery = _api.GetContext<TestDocument>().Where(x => x.Title == "test").ToString();
            Assert.AreEqual(whereEqualsQuery, "q=(title_s:test)");
        }

        [TestMethod]
        public void PhraseQuery()
        {
            var whereEqualsQuery = _api.GetContext<TestDocument>().Where(x => x.Title == "test test").ToString();
            Assert.AreEqual(whereEqualsQuery, "q=(title_s:\"test test\")");
        }

        [TestMethod]
        public void WhereNotEqualsQuery()
        {
            var whereEqualsQuery = _api.GetContext<TestDocument>().Where(x => x.Title != "test").ToString();
            Assert.AreEqual(whereEqualsQuery, "q=(-title_s:test)");
        }

        [TestMethod]
        public void FilterEqualsQuery()
        {
            var filterEqualsQuery = _api.GetContext<TestDocument>().Filter(x => x.Title == "test").ToString();
            Assert.AreEqual(filterEqualsQuery, "q=*:*&fq=(title_s:test)");
        }

        [TestMethod]
        public void PageQuery()
        {
            var pageQuery = _api.GetContext<TestDocument>().Page(0, 4).ToString();
            Assert.AreEqual(pageQuery, "q=*:*&start=0&rows=4");
        }

        [TestMethod]
        public void OrderByQuery()
        {
            var orderByQuery = _api.GetContext<TestDocument>().OrderBy(x => x.Id).ThenBy(x => x.Title).ToString();
            Assert.AreEqual(orderByQuery, "q=*:*&sort=title_s asc,_uniqueid asc");
        }

        [TestMethod]
        public void OrderByDescendingQuery()
        {
            var orderByDescendingQuery = _api.GetContext<TestDocument>().OrderByDescending(x => x.Id).ThenByDescending(x => x.Title).ToString();
            Assert.AreEqual(orderByDescendingQuery, "q=*:*&sort=title_s desc,_uniqueid desc");
        }

        [TestMethod]
        public void ContainsQuery()
        {
            var containsQuery = _api.GetContext<TestDocument>().Where(x => x.Title.Contains("test")).ToString();
            Assert.AreEqual(containsQuery, "q=(title_s:*test*)");
        }

        [TestMethod]
        public void StartWithQuery()
        {
            var startWithQuery = _api.GetContext<TestDocument>().Where(x => x.Title.StartsWith("test")).ToString();
            Assert.AreEqual(startWithQuery, "q=(title_s:test*)");
        }

        [TestMethod]
        public void EndsWithQuery()
        {
            var endsWithQuery = _api.GetContext<TestDocument>().Where(x => x.Title.EndsWith("test")).ToString();
            Assert.AreEqual(endsWithQuery, "q=(title_s:*test)");
        }

        [TestMethod]
        public void FacetQuery()
        {
            var facetQuery = _api.GetContext<TestDocument>();
            var facets = new Dictionary<string, List<string>> { { "Author", new List<string> { "test", "test test" } } };
            foreach (var facet in facets)
            {
                facetQuery = facetQuery.Facet(x => x[facet.Key], facet.Value, true);
            }
            var facetQueryStr = facetQuery.ToString();
            Assert.AreEqual(facetQueryStr, "q=*:*&facet.field={!ex=Author}Author&fq={!tag=Author}Author:(test OR \"test test\")&facet=on");
        }

        [TestMethod]
        public void MultifacetQuery()
        {
            var multifacetQuery = _api.GetContext<TestDocument>();
            var multifacets = new Dictionary<string, List<string>> { { "Author", new List<string> { "test" } }, { "Approver", new List<string> { "LastName" } } };
            foreach (var facet in multifacets)
            {
                multifacetQuery = multifacetQuery.Facet(x => x[facet.Key], facet.Value, true);
            }
            var multifacetQueryStr = multifacetQuery.ToString();
            Assert.AreEqual(multifacetQueryStr, "q=*:*&facet.field={!ex=Approver}Approver&fq={!tag=Approver}Approver:(LastName)&facet.field={!ex=Author}Author&fq={!tag=Author}Author:(test)&facet=on");
        }

        [TestMethod]
        public void FacetQueryBykey()
        {
            var author = "Author";
            var facetQueryBykey = _api.GetContext<TestDocument>().Facet(x => x[author]).ToString();
            Assert.AreEqual(facetQueryBykey, "q=*:*&facet.field=Author&facet=on");
        }

        [TestMethod]
        public void PivotQuery()
        {
            var pivotQuery = _api.GetContext<TestDocument>().PivotFacet(x => x.Facet(f => f.DocumentId).Facet(f => f.Type)).ToString();
            Assert.AreEqual(pivotQuery, "q=*:*&facet.pivot=_documentid,_type&facet=on");
        }

        [TestMethod]
        public void QueryWithPredicateBuilderOr()
        {
            var predicate = PredicateBuilder.False<TestDocument>();
            var list = new[] { "test1", "test2" };
            foreach (var i in list)
                predicate = predicate.Or(p => p.Title == i);
            var query = _api.GetContext<TestDocument>().Where(predicate).ToString();
            Assert.AreEqual(query, "q=(title_s:test1 OR title_s:test2)");
        }

        [TestMethod]
        public void QueryWithPredicateBuilderAnd()
        {
            var predicate = PredicateBuilder.True<TestDocument>();
            var list = new[] { "test1", "test2" };
            foreach (var i in list)
                predicate = predicate.And(p => p.Title == i);
            var query = _api.GetContext<TestDocument>().Where(predicate).ToString();
            Assert.AreEqual(query, "q=(title_s:test1 AND title_s:test2)");
        }

        [TestMethod]
        public void QueryWithArrayIndex()
        {
            var data = new[] { new TestDocument { Title = "test" } };
            var queryWithGetItem = _api.GetContext<TestDocument>().Where(x => x.Title == data[0].Title).ToString();
            Assert.AreEqual(queryWithGetItem, "q=(title_s:test)");
        }

        [TestMethod]
        public void QueryWithGetItem()
        {
            var data = new TestDocument();
            var queryWithGetItem = _api.GetContext<TestDocument>().Where(x => x.Title == data["test"]).ToString();
            Assert.AreEqual(queryWithGetItem, "q=(title_s:test)");
        }

        [TestMethod]
        public void RangeQueries()
        {
            var date = new DateTime(2000, 1, 1);
            var greaterThan = _api.GetContext<TestDocument>().Where(x => x.Timestamp > date).ToString();
            Assert.AreEqual(greaterThan, "q=(_timestamp:{2000-01-01T12:00:00.000Z TO *])");

            var greaterThanOrEqual = _api.GetContext<TestDocument>().Where(x => x.Timestamp >= date).ToString();
            Assert.AreEqual(greaterThanOrEqual, "q=(_timestamp:[2000-01-01T12:00:00.000Z TO *])");

            var lessThan = _api.GetContext<TestDocument>().Where(x => x.Timestamp < date).ToString();
            Assert.AreEqual(lessThan, "q=(_timestamp:[* TO 2000-01-01T12:00:00.000Z})");

            var lessThanOrEqual = _api.GetContext<TestDocument>().Where(x => x.Timestamp <= date).ToString();
            Assert.AreEqual(lessThanOrEqual, "q=(_timestamp:[* TO 2000-01-01T12:00:00.000Z])");
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
