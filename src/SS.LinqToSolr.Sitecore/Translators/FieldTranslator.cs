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

        public void Accept(CultureInfo culture)
        {
            _fieldNameTranslator.Accept(new CultureExecutionContext(culture));
        }

        public string Translate(MemberInfo member)
        {
            var fieldName = _fieldNameTranslator.GetIndexFieldName(member);
            return fieldName;
        }
    }
}
