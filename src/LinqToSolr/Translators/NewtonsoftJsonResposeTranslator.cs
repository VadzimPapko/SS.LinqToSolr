using Newtonsoft.Json;
using SS.LinqToSolr.Models;
using SS.LinqToSolr.Models.SearchResponse;

namespace SS.LinqToSolr.Translators
{
    public class NewtonsoftJsonResposeTranslator : IResposeTranslator
    {
        public Response<T> Translate<T>(string responce) where T : Document
        {
            var result = JsonConvert.DeserializeObject<Response<T>>(responce);
            return result;
        }
    }
}
