using Newtonsoft.Json;
using System.Globalization;
using System.Reflection;

namespace SS.LinqToSolr.Translators
{
    public class NewtonsoftJsonFieldTranslator : IFieldTranslator
    {
        public string Translate(MemberInfo member, CultureInfo culture)
        {
            var dataMemberAttribute = member.GetCustomAttribute<JsonPropertyAttribute>();
            var fieldName = !string.IsNullOrEmpty(dataMemberAttribute?.PropertyName)
                ? dataMemberAttribute.PropertyName
                : member.Name;

            if(culture != null)
            {
                return $"{fieldName}_{culture.TwoLetterISOLanguageName}";
            }

            return fieldName;
        }
    }
}
