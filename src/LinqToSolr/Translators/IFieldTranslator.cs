using System.Reflection;

namespace SS.LinqToSolr.Translators
{
    public interface IFieldTranslator
    {
        string Translate(MemberInfo member);
    }
}
