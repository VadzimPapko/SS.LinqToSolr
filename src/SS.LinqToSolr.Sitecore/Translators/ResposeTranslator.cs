using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Common;
using SS.LinqToSolr.Models.SearchResponse;
using SS.LinqToSolr.Translators;
using System;
using System.Collections.Generic;

namespace SS.LinqToSolr.Sitecore.Translators
{
    public class ResposeTranslator : IResposeTranslator
    {
        private FieldNameTranslator _fieldNameTranslator;
        public ResposeTranslator(FieldNameTranslator fieldNameTranslator)
        {
            _fieldNameTranslator = fieldNameTranslator;
        }

        public Response<T> Translate<T>(string responce)
        {
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new CustomContractResolver(typeof(T), _fieldNameTranslator);
            var result = JsonConvert.DeserializeObject<Response<T>>(responce, settings);

            return result;
        }

        private class CustomContractResolver : DefaultContractResolver
        {
            private FieldNameTranslator _fieldNameTranslator;
            private Dictionary<string, string> PropertyMappings { get; set; } = new Dictionary<string, string>();

            public CustomContractResolver(Type type, FieldNameTranslator fieldNameTranslator)
            {
                _fieldNameTranslator = fieldNameTranslator;
                var props = type.GetProperties();
                foreach (var prop in props)
                {
                    if (Attribute.IsDefined(prop, typeof(IndexFieldAttribute)))
                    {
                        var fieldName = _fieldNameTranslator.GetIndexFieldName(prop);
                        PropertyMappings.Add(prop.Name, fieldName);
                    }
                }
            }

            protected override string ResolvePropertyName(string propertyName)
            {
                var resolved = PropertyMappings.TryGetValue(propertyName, out string resolvedName);
                return resolved ? resolvedName : base.ResolvePropertyName(propertyName);
            }
        }
    }
}
