using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;
using SS.LinqToSolr.Test.Models;

namespace SS.LinqToSolr.Test
{
    [TestClass]
    public class QueryCommonTest : BaseSolrTest
    {
        public QueryCommonTest() : base("sitecore_signals")
        {
        }

        [TestMethod]
        public void Empty()
        {
            var emptyQuery = _api.GetQueryable<TestDocument>().ToString();
            Assert.AreEqual(emptyQuery, "q=*:*");
        }

        [TestMethod]
        public void Format()
        {
            var test = "test";
            var formatQuery = _api.GetQueryable<TestDocument>().Where(x => x.Title == $"{test}").ToString();
            Assert.AreEqual(formatQuery, "q=(title_s:test)");
        }

        [TestMethod]
        public void Phrase()
        {
            var whereEqualsQuery = _api.GetQueryable<TestDocument>().Where(x => x.Title == "test test").ToString();
            Assert.AreEqual(whereEqualsQuery, "q=(title_s:\"test test\")");
        }

        [TestMethod]
        public void NotEqual()
        {
            var whereEqualsQuery = _api.GetQueryable<TestDocument>().Where(x => x.Title != "test").ToString();
            Assert.AreEqual(whereEqualsQuery, "q=(-title_s:test)");
        }

        [TestMethod]
        public void Contains()
        {
            var containsQuery = _api.GetQueryable<TestDocument>().Where(x => x.Title.Contains("test")).ToString();
            Assert.AreEqual(containsQuery, "q=(title_s:*test*)");
        }

        [TestMethod]
        public void StartWith()
        {
            var startWithQuery = _api.GetQueryable<TestDocument>().Where(x => x.Title.StartsWith("test")).ToString();
            Assert.AreEqual(startWithQuery, "q=(title_s:test*)");
        }

        [TestMethod]
        public void EndsWith()
        {
            var endsWithQuery = _api.GetQueryable<TestDocument>().Where(x => x.Title.EndsWith("test")).ToString();
            Assert.AreEqual(endsWithQuery, "q=(title_s:*test)");
        }
        
        [TestMethod]
        public void ArrayIndex()
        {
            var data = new[] { new TestDocument { Title = "test" } };
            var queryWithGetItem = _api.GetQueryable<TestDocument>().Where(x => x.Title == data[0].Title).ToString();
            Assert.AreEqual(queryWithGetItem, "q=(title_s:test)");
        }

        [TestMethod]
        public void GetItem()
        {
            var data = new TestDocument();
            var queryWithGetItem = _api.GetQueryable<TestDocument>().Where(x => x.Title == data["test"]).ToString();
            Assert.AreEqual(queryWithGetItem, "q=(title_s:test)");
        }

        [TestMethod]
        public void Range()
        {
            var date = new DateTime(2000, 1, 1);
            var greaterThan = _api.GetQueryable<TestDocument>().Where(x => x.Timestamp > date).ToString();
            Assert.AreEqual(greaterThan, "q=(_timestamp:{2000-01-01T12:00:00.000Z TO *])");

            var greaterThanOrEqual = _api.GetQueryable<TestDocument>().Where(x => x.Timestamp >= date).ToString();
            Assert.AreEqual(greaterThanOrEqual, "q=(_timestamp:[2000-01-01T12:00:00.000Z TO *])");

            var lessThan = _api.GetQueryable<TestDocument>().Where(x => x.Timestamp < date).ToString();
            Assert.AreEqual(lessThan, "q=(_timestamp:[* TO 2000-01-01T12:00:00.000Z})");

            var lessThanOrEqual = _api.GetQueryable<TestDocument>().Where(x => x.Timestamp <= date).ToString();
            Assert.AreEqual(lessThanOrEqual, "q=(_timestamp:[* TO 2000-01-01T12:00:00.000Z])");
        }

        [TestMethod]
        public void And()
        {
            var and = _api.GetQueryable<TestDocument>().Where(x => x.Title == "test" && x.Title == "test1").ToString();
            Assert.AreEqual(and, "q=(title_s:test AND title_s:test1)");
        }

        [TestMethod]
        public void Or()
        {
            var or = _api.GetQueryable<TestDocument>().Where(x => x.Title == "test" || x.Title == "test1").ToString();
            Assert.AreEqual(or, "q=(title_s:test OR title_s:test1)");
        }       
    }
}
