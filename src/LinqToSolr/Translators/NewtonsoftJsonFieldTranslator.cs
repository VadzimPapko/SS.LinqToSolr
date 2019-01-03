using Newtonsoft.Json;
using System.Reflection;

namespace SS.LinqToSolr.Translators
{
    public class NewtonsoftJsonFieldTranslator : IFieldTranslator
    {
        public string GetFieldName(MemberInfo member)
        {
            var dataMemberAttribute = member.GetCustomAttribute<JsonPropertyAttribute>();
            var fieldName = !string.IsNullOrEmpty(dataMemberAttribute?.PropertyName)
                ? dataMemberAttribute.PropertyName
                : member.Name;

            return fieldName;
        }
    }
}
