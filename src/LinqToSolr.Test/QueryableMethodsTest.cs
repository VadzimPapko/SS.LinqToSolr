using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var whereEqualsQuery = _api.GetContext<TestDocument>().Where(x => x.Title == "test").ToString();
            Assert.AreEqual(whereEqualsQuery, "q=(title_s:test)");
        }

        [TestMethod]
        public void OrderBy()
        {
            var orderByQuery = _api.GetContext<TestDocument>().OrderBy(x => x.Id).ThenBy(x => x.Title).ToString();
            Assert.AreEqual(orderByQuery, "q=*:*&sort=title_s asc,_uniqueid asc");
        }

        [TestMethod]
        public void OrderByDescending()
        {
            var orderByDescendingQuery = _api.GetContext<TestDocument>().OrderByDescending(x => x.Id).ThenByDescending(x => x.Title).ToString();
            Assert.AreEqual(orderByDescendingQuery, "q=*:*&sort=title_s desc,_uniqueid desc");
        }

        [TestMethod]
        public void Take()
        {
            var pageQuery = _api.GetContext<TestDocument>().Take(4).ToString();
            Assert.AreEqual(pageQuery, "q=*:*&rows=4");
        }

        [TestMethod]
        public void Skip()
        {
            var pageQuery = _api.GetContext<TestDocument>().Skip(4).ToString();
            Assert.AreEqual(pageQuery, "q=*:*&start=4");
        }
    }
}
