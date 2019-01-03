using SS.LinqToSolr.Models;
using SS.LinqToSolr.Models.SearchResponse;

namespace SS.LinqToSolr.Translators
{
    public interface IResposeTranslator
    {
        Response<T> Translate<T>(string responce) where T : Document;
    }
}
