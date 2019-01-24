using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS.LinqToSolr.Extensions;
using SS.LinqToSolr.Test.Models;
using System.Linq;

namespace SS.LinqToSolr.Test
{
    [TestClass]
    public class QueryableMethodsTest : BaseSolrTest
    {
        public QueryableMethodsTest() : base("sitecore_signals")
        {
        }

        [TestMethod]
        public void Where()
        {
            var whereEqualsQuery = _api.GetQueryable<TestDocument>().Where(x => x.Title == "test").ToString();
            Assert.AreEqual(whereEqualsQuery, "q=title_s:test");
        }

        [TestMethod]
        public void OrderBy()
        {
            var orderByQuery = _api.GetQueryable<TestDocument>().OrderBy(x => x.Id).ThenBy(x => x.Title).ThenBy(x => x.Timestamp).ToString();
            Assert.AreEqual(orderByQuery, "q=*:*&sort=_uniqueid asc,title_s asc,_timestamp asc");
        }

        [TestMethod]
        public void OrderByDescending()
        {
            var orderByDescendingQuery = _api.GetQueryable<TestDocument>().OrderByDescending(x => x.Id).ThenByDescending(x => x.Title).ToString();
            Assert.AreEqual(orderByDescendingQuery, "q=*:*&sort=_uniqueid desc,title_s desc");
        }        

        [TestMethod]
        public void Take()
        {
            var pageQuery = _api.GetQueryable<TestDocument>().Take(4).ToString();
            Assert.AreEqual(pageQuery, "q=*:*&rows=4");
        }

        [TestMethod]
        public void Skip()
        {
            var pageQuery = _api.GetQueryable<TestDocument>().Skip(4).ToString();
            Assert.AreEqual(pageQuery, "q=*:*&start=4");
        }
    }
}
