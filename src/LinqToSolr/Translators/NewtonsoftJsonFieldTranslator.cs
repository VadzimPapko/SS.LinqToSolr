using Newtonsoft.Json;
using System.Globalization;
using System.Reflection;

namespace SS.LinqToSolr.Translators
{
    public class NewtonsoftJsonFieldTranslator : IFieldTranslator
    {
        protected CultureInfo _culture;
        public void Accept(CultureInfo culture)
        {
            _culture = culture;
        }

        public string Translate(MemberInfo member)
        {
            var dataMemberAttribute = member.GetCustomAttribute<JsonPropertyAttribute>();
            var fieldName = !string.IsNullOrEmpty(dataMemberAttribute?.PropertyName)
                ? dataMemberAttribute.PropertyName
                : member.Name;

            if (_culture != null)
            {
                return $"{fieldName}_{_culture.TwoLetterISOLanguageName}";
            }

            return fieldName;
        }
    }
}
