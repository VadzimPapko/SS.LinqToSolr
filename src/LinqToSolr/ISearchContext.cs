using SS.LinqToSolr.Models;
using SS.LinqToSolr.Models.SearchResponse;
using System.Linq;

namespace SS.LinqToSolr
{
    public interface ISearchContext
    {
        string LastQuery { get; }
        Response<T> Search<T>(string query) where T : Document;
        IQueryable<T> GetQueryable<T>() where T : Document;
    }
}
