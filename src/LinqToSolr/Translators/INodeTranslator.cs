using SS.LinqToSolr.Models.Query;
using System.Collections.Generic;

namespace SS.LinqToSolr.Translators
{
    public interface INodeTranslator
    {
        string Translate(List<MethodNode> methods);
    }
}
