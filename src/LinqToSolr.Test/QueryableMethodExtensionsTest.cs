using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using SS.LinqToSolr.Extensions;
using SS.LinqToSolr.Test.Models;

namespace SS.LinqToSolr.Test
{
    [TestClass]
    public class QueryableMethodExtensionsTest : BaseSolrTest
    {
        public QueryableMethodExtensionsTest() : base("sitecore_signals")
        {
        }

        [TestMethod]
        public void PlainQuery()
        {
            var test = "test";
            var plainQuery = _api.GetQueryable<TestDocument>().Query(test).ToString();
            Assert.AreEqual(plainQuery, "q=(test)");
        }

        [TestMethod]
        public void FilterEqualsQuery()
        {
            var filterEqualsQuery = _api.GetQueryable<TestDocument>().Filter(x => x.Title == "test").ToString();
            Assert.AreEqual(filterEqualsQuery, "q=*:*&fq=(title_s:test)");
        }

        [TestMethod]
        public void PageQuery()
        {
            var pageQuery = _api.GetQueryable<TestDocument>().Page(0, 4).ToString();
            Assert.AreEqual(pageQuery, "q=*:*&start=0&rows=4");
        }
        
        [TestMethod]
        public void FacetQuery()
        {
            var facetQuery = _api.GetQueryable<TestDocument>();
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
            var multifacetQuery = _api.GetQueryable<TestDocument>();
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
            var facetQueryBykey = _api.GetQueryable<TestDocument>().Facet(x => x[author]).ToString();
            Assert.AreEqual(facetQueryBykey, "q=*:*&facet.field=Author&facet=on");
        }

        [TestMethod]
        public void PivotQuery()
        {
            var pivotQuery = _api.GetQueryable<TestDocument>().PivotFacet(x => x.Facet(f => f.DocumentId).Facet(f => f.Type)).ToString();
            Assert.AreEqual(pivotQuery, "q=*:*&facet.pivot=_documentid,_type&facet=on");
        }        
    }
}
