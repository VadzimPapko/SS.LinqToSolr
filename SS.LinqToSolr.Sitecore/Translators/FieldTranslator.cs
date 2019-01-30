using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Common;
using SS.LinqToSolr.Translators;
using System.Globalization;
using System.Reflection;

namespace SS.LinqToSolr.Sitecore.Translators
{
    public class FieldTranslator : IFieldTranslator
    {
        private FieldNameTranslator _fieldNameTranslator;
        public FieldTranslator(FieldNameTranslator fieldNameTranslator)
        {
            _fieldNameTranslator = fieldNameTranslator;
        }

        public void Accept(IExecutionContext executionContext)
        {
            _fieldNameTranslator.Accept(executionContext);
        }

        public string Translate(MemberInfo member, CultureInfo culture)
        {
            var fieldName = _fieldNameTranslator.GetIndexFieldName(member);
            //var dataMemberAttribute = member.GetCustomAttribute<IndexFieldAttribute>();
            //var fieldName = !string.IsNullOrEmpty(dataMemberAttribute?.IndexFieldName)
            //    ? dataMemberAttribute.IndexFieldName
            //    : member.Name;

            return fieldName;
        }
    }
}
