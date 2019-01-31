using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Abstractions;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.SolrProvider;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SS.LinqToSolr.Sitecore.Translators
{
    public class SolrFieldNameTranslator : AbstractFieldNameTranslator
    {
        private readonly ConcurrentDictionary<string, IEnumerable<string>> typeFieldMap = new ConcurrentDictionary<string, IEnumerable<string>>();
        private readonly SolrFieldMap fieldMap;
        private readonly SolrIndexSchema schema;
        private readonly SolrSearchIndex index;
        private string currentCultureCode;
        private ISettings settings;

        SolrIndexSchema Schema
        {
            get
            {
                return this.schema;
            }
        }

        SolrFieldMap FieldMap
        {
            get
            {
                return this.fieldMap;
            }
        }

        public SolrFieldNameTranslator(SolrSearchIndex solrSearchIndex)
        {
            this.index = solrSearchIndex;
            this.fieldMap = solrSearchIndex.Configuration.FieldMap as SolrFieldMap;
            this.schema = solrSearchIndex.Schema as SolrIndexSchema;
            this.settings = solrSearchIndex.Locator.GetInstance<ISettings>();
        }

        public override void Accept(IExecutionContext executionContext)
        {
            base.Accept(executionContext);
            CultureExecutionContext executionContext1 = executionContext as CultureExecutionContext;
            if (executionContext1 == null)
                return;
            if (executionContext1.PredicateType == CulturePredicateType.Not)
                this.ResetCultureContext();
            else
                this.AddCultureContext(executionContext1.Culture);
        }

        public void AddCultureContext(CultureInfo culture)
        {
            this.currentCultureCode = culture?.TwoLetterISOLanguageName;
        }

        public void ResetCultureContext()
        {
            this.currentCultureCode = (string)null;
        }

        public override string GetIndexFieldName(MemberInfo member)
        {
            return this.GetIndexFieldName(member, (CultureInfo)null);
        }

        public string GetIndexFieldName(MemberInfo member, CultureInfo culture)
        {
            string fieldName = member.Name;
            IIndexFieldNameFormatterAttribute formatterAttribute = this.GetIndexFieldNameFormatterAttribute(member);
            if (formatterAttribute != null)
                fieldName = formatterAttribute.GetIndexFieldName(member.Name);
            Type returnType;
            if ((object)(member as PropertyInfo) != null)
            {
                returnType = ((PropertyInfo)member).PropertyType;
            }
            else
            {
                if ((object)(member as FieldInfo) == null)
                    throw new NotSupportedException("Unexpected member type: " + member.GetType().FullName);
                returnType = ((FieldInfo)member).FieldType;
            }
            return this.ProcessFieldName(fieldName, returnType, culture, "", false);
        }

        public override string GetIndexFieldName(string fieldName, Type returnType)
        {
            return this.GetIndexFieldName(fieldName, returnType, (CultureInfo)null);
        }

        public string GetIndexFieldName(string fieldName, Type returnType, CultureInfo culture)
        {
            return this.ProcessFieldName(fieldName, returnType, culture, "", false);
        }

        public string GetIndexFieldName(string fieldName, string fieldTypeKey, Type returnType, CultureInfo culture)
        {
            return this.ProcessFieldName(fieldName, fieldTypeKey, returnType, culture, "", false);
        }

        public string GetIndexFieldName(string fieldName, string returnType)
        {
            return this.GetIndexFieldName(fieldName, returnType, (CultureInfo)null);
        }

        public string GetIndexFieldName(string fieldName, string returnType, CultureInfo culture)
        {
            return this.ProcessFieldName(fieldName, (Type)null, culture, returnType, false);
        }

        public override string GetIndexFieldName(string fieldName)
        {
            return this.GetIndexFieldName(fieldName, (CultureInfo)null);
        }

        public string GetIndexFieldName(string fieldName, CultureInfo culture)
        {
            return this.ProcessFieldName(fieldName, (Type)null, culture, (string)null, true);
        }

        public string GetIndexFieldName(string fieldName, bool aggressiveResolver)
        {
            return this.GetIndexFieldName(fieldName, aggressiveResolver, (CultureInfo)null);
        }

        public string GetIndexFieldName(string fieldName, bool aggressiveResolver, CultureInfo culture)
        {
            return this.ProcessFieldName(fieldName, (Type)null, culture, (string)null, aggressiveResolver);
        }

        public override Dictionary<string, List<string>> MapDocumentFieldsToType(Type type, MappingTargetType target, IEnumerable<string> documentFieldNames)
        {
            switch (target)
            {
                case MappingTargetType.Anything:
                    Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
                    Action<Dictionary<string, List<string>>, Dictionary<string, List<string>>> action = (Action<Dictionary<string, List<string>>, Dictionary<string, List<string>>>)((result, map) =>
                    {
                        foreach (KeyValuePair<string, List<string>> keyValuePair in map)
                        {
                            if (keyValuePair.Value != null && keyValuePair.Value.Count != 0)
                            {
                                List<string> list;
                                if (!result.TryGetValue(keyValuePair.Key, out list))
                                    result[keyValuePair.Key] = list = new List<string>();
                                list.AddRange(keyValuePair.Value.Where<string>((Func<string, bool>)(l => !list.Contains(l))));
                            }
                        }
                    });
                    action(dictionary, this.MapDocumentFieldsToTypeIndexer(type, documentFieldNames));
                    action(dictionary, this.MapDocumentFieldsToTypeProperties(type, documentFieldNames));
                    return dictionary;
                case MappingTargetType.Indexer:
                    return this.MapDocumentFieldsToTypeIndexer(type, documentFieldNames);
                case MappingTargetType.Properties:
                    return this.MapDocumentFieldsToTypeProperties(type, documentFieldNames);
                default:
                    throw new ArgumentException("Invalid mapping target type: " + (object)target, nameof(target));
            }
        }

        private Dictionary<string, List<string>> MapDocumentFieldsToTypeIndexer(Type type, IEnumerable<string> documentFieldNames)
        {
            Dictionary<string, List<string>> dictionary = documentFieldNames.ToDictionary<string, string, List<string>>((Func<string, string>)(f => f.ToLowerInvariant()), (Func<string, List<string>>)(f => this.GetTypeFieldNames(f).ToList<string>()));
            foreach (PropertyInfo property in this.GetProperties(type))
            {
                string index = property.Name;
                Type propertyType = property.PropertyType;
                IIndexFieldNameFormatterAttribute formatterAttribute = this.GetIndexFieldNameFormatterAttribute((MemberInfo)property);
                string fieldName = property.Name;
                if (formatterAttribute != null)
                {
                    index = formatterAttribute.GetIndexFieldName(index);
                    fieldName = formatterAttribute.GetTypeFieldName(fieldName);
                }
                if (!this.schema.AllFieldNames.Contains(index))
                {
                    SolrSearchFieldConfiguration fieldConfiguration = this.fieldMap.GetFieldConfiguration(propertyType) as SolrSearchFieldConfiguration;
                    if (fieldConfiguration != null)
                        index = fieldConfiguration.FormatFieldName(index, (ISearchIndexSchema)this.schema, this.currentCultureCode, (string)null);
                }
                if (dictionary.ContainsKey(index))
                    dictionary[index].Add(fieldName);
            }
            return dictionary;
        }

        private Dictionary<string, List<string>> MapDocumentFieldsToTypeProperties(Type type, IEnumerable<string> documentFieldNames)
        {
            Dictionary<string, List<string>> dictionary = documentFieldNames.ToDictionary<string, string, List<string>>((Func<string, string>)(name => name.ToLowerInvariant()), (Func<string, List<string>>)(y => this.GetTypeFieldNames(y).ToList<string>()));
            Dictionary<string, List<string>> mappedProperties = new Dictionary<string, List<string>>();
            this.ProcessProperties(type, (IDictionary<string, List<string>>)dictionary, ref mappedProperties, "", "");
            return mappedProperties;
        }

        public override IEnumerable<string> GetTypeFieldNames(string fieldName)
        {
            Func<string, IEnumerable<string>> valueFactory = (Func<string, IEnumerable<string>>)(key =>
            {
                List<string> stringList = new List<string>();
                string str = this.StripKnownExtensions(fieldName);
                if (str != fieldName)
                {
                    stringList.Add(str);
                }
                else
                {
                    fieldName = str;
                    if (!fieldName.StartsWith("_", StringComparison.Ordinal))
                        stringList.Add(Regex.Replace(fieldName, "(?<!\\.)_", " ").Trim());
                    else
                        stringList.Add(fieldName);
                }
                return (IEnumerable<string>)stringList;
            });
            return this.typeFieldMap.GetOrAdd(fieldName, valueFactory);
        }

        public string StripKnownCultures(string fieldName)
        {
            foreach (string allCulture in (IEnumerable<string>)this.schema.AllCultures)
            {
                if (fieldName.Length > allCulture.Length && fieldName.EndsWith(allCulture, StringComparison.Ordinal))
                    fieldName = fieldName.Substring(0, fieldName.Length - allCulture.Length);
            }
            return fieldName;
        }

        public bool HasCulture(string fieldName)
        {
            return this.schema.AllCultures.Where<string>((Func<string, bool>)(culture => fieldName.Length > culture.Length)).Any<string>((Func<string, bool>)(i => fieldName.EndsWith(i, StringComparison.Ordinal)));
        }

        public string StripKnownExtensions(string fieldName)
        {
            fieldName = this.StripKnownCultures(fieldName);
            foreach (SolrSearchFieldConfiguration availableType in (IEnumerable<SolrSearchFieldConfiguration>)this.fieldMap.GetAvailableTypes())
            {
                if (fieldName.StartsWith("_", StringComparison.Ordinal))
                {
                    if (!fieldName.StartsWith("__", StringComparison.Ordinal))
                        break;
                }
                string str = availableType.FieldNameFormat.Replace("{0}", string.Empty);
                if (fieldName.EndsWith(str, StringComparison.Ordinal))
                    fieldName = fieldName.Substring(0, fieldName.Length - str.Length);
                if (fieldName.StartsWith(str, StringComparison.Ordinal))
                    fieldName = fieldName.Substring(str.Length, fieldName.Length);
            }
            return fieldName;
        }

        public string StripKnownExtensions(IEnumerable<string> fields)
        {
            List<string> stringList = new List<string>();
            foreach (string field in fields)
                stringList.Add(this.StripKnownExtensions(field));
            return string.Join(",", (IEnumerable<string>)stringList);
        }

        private string ProcessFieldName(string fieldName, Type returnType, CultureInfo culture, string returnTypeString = "", bool aggressiveResolver = false)
        {
            return this.ProcessFieldName(fieldName, (string)null, returnType, culture, returnTypeString, aggressiveResolver);
        }

        private string ProcessFieldName(string fieldName, string fieldTypeKey, Type returnType, CultureInfo culture, string returnTypeString = "", bool aggressiveResolver = false)
        {
            string strippedFieldName = this.StripKnownExtensions(fieldName);
            strippedFieldName = strippedFieldName.Replace(" ", "_").ToLowerInvariant();
            string cultureCode = string.Empty;
            cultureCode = culture?.TwoLetterISOLanguageName ?? this.currentCultureCode;
            this.Debug(string.Format("Processing Field Name : {0} with parameters: \n\tStripped Field Name: {1} \n\tResolved culture code : {2}", (object)fieldName, (object)strippedFieldName, (object)cultureCode));
            SolrSearchFieldConfiguration fieldConfiguration1 = this.fieldMap.GetFieldConfiguration(strippedFieldName) as SolrSearchFieldConfiguration;
            if (fieldConfiguration1 != null)
            {
                string resolvedFieldName = fieldConfiguration1.FormatFieldName(strippedFieldName, (ISearchIndexSchema)this.schema, cultureCode, (string)null);
                this.LogResolutionResult(fieldName, resolvedFieldName, "Field Name Map");
                return resolvedFieldName;
            }
            if (this.schema.AllFieldNames.Contains(strippedFieldName))
            {
                this.LogResolutionResult(fieldName, strippedFieldName, "solr schema Fields");
                return strippedFieldName;
            }
            if (!string.IsNullOrEmpty(returnTypeString))
            {
                SolrSearchFieldConfiguration configurationByReturnType = this.fieldMap.GetFieldConfigurationByReturnType(returnTypeString) as SolrSearchFieldConfiguration;
                string reason = string.Format("return Type String: {0}", (object)returnTypeString);
                if (configurationByReturnType != null)
                {
                    string resolvedFieldName = configurationByReturnType.FormatFieldName(strippedFieldName, (ISearchIndexSchema)this.schema, cultureCode, (string)null);
                    this.LogResolutionResult(fieldName, resolvedFieldName, reason);
                    return resolvedFieldName;
                }
            }
            if (returnType != (Type)null)
            {
                SolrSearchFieldConfiguration fieldConfiguration2 = (SolrSearchFieldConfiguration)this.fieldMap.GetFieldConfiguration(returnType, (Func<List<SolrSearchFieldConfiguration>, AbstractSearchFieldConfiguration>)(list => this.ResolveMultipleFieldConfiguration(list, fieldName, strippedFieldName, returnType, cultureCode, returnTypeString, fieldTypeKey)));
                string reason = fieldConfiguration2 == null ? string.Empty : string.Format("return Type : {0}", (object)fieldConfiguration2.SystemType.FullName);
                if (fieldConfiguration2 != null)
                {
                    string resolvedFieldName = fieldConfiguration2.FormatFieldName(strippedFieldName, (ISearchIndexSchema)this.schema, cultureCode, (string)null);
                    this.LogResolutionResult(fieldName, resolvedFieldName, reason);
                    return resolvedFieldName;
                }
            }
            if (aggressiveResolver)
            {
                string resolvedFieldName = this.AggressiveProcessFieldName(fieldName.ToLower(), returnType, cultureCode, returnTypeString, fieldTypeKey, fieldName.ToLowerInvariant());
                this.LogResolutionResult(fieldName, resolvedFieldName, "Template Field");
                return resolvedFieldName;
            }
            this.Debug(string.Format("Processing Field Name : {0}. No match found", (object)fieldName.ToLowerInvariant()));
            return fieldName.ToLowerInvariant();
        }

        private void LogResolutionResult(string fieldName, string resolvedFieldName, string reason)
        {
            this.Debug(string.Format("Processing Field Name : {0}. Matched field name by {1}. Resolved Field Name is {2}", (object)fieldName, (object)reason, (object)resolvedFieldName));
        }

        protected virtual AbstractSearchFieldConfiguration ResolveMultipleFieldConfiguration(List<SolrSearchFieldConfiguration> list, string fieldName, string strippedFieldName, Type returnType, string cultureCode, string returnTypeString, string fieldTypeKey)
        {
            string str = list.Where<SolrSearchFieldConfiguration>((Func<SolrSearchFieldConfiguration, bool>)(l => l != null)).Select<SolrSearchFieldConfiguration, string>((Func<SolrSearchFieldConfiguration, string>)(l => l.FieldNameFormat)).Aggregate<string>((Func<string, string, string>)((current, next) => string.Format("\t\t{0},\n\t\t{1}", (object)current, (object)next)));
            this.Debug(string.Format("Processing Field Name : {0}. Resolving Multiple Field found on Solr Field Map. \n\treturn type '{1}'. \n\tFieldNameFormat: \n{2}", (object)fieldName, (object)returnType.FullName, (object)str));
            List<SolrSearchFieldConfiguration> source = this.fieldMap.GetFieldConfigurations(strippedFieldName, returnType);
            if (!source.Any<SolrSearchFieldConfiguration>() && source.GetType() != typeof(SolrFieldMap.NullReturnType))
                source = this.AggressiveResolveFieldConfigurations(strippedFieldName, returnType, cultureCode, returnTypeString, fieldTypeKey);
            else
                this.Debug(string.Format("Processing Field Name : {0}. Resolving Multiple Field found on Solr Field Map. Cache Found. Get template resolver result from cache.", (object)fieldName));
            List<SolrSearchFieldConfiguration> list1 = source.Where<SolrSearchFieldConfiguration>((Func<SolrSearchFieldConfiguration, bool>)(c =>
            {
                if (string.IsNullOrEmpty(fieldTypeKey))
                    return true;
                if (c.FieldTypeName != null)
                    return fieldTypeKey.ToLower() == c.FieldTypeName.ToLower();
                return false;
            })).ToList<SolrSearchFieldConfiguration>();
            if (!list1.Any<SolrSearchFieldConfiguration>())
            {
                this.Debug(string.Format("Processing Field Name : {0}. Resolving Multiple Field found on Solr Field Map. No matching template field on index field name '{1}', return type '{2}' and field type '{3}'", new object[4]
                {
          (object) fieldName,
          (object) strippedFieldName,
          (object) returnType.Name,
          (object) fieldTypeKey
                }));
                return (AbstractSearchFieldConfiguration)list.First<SolrSearchFieldConfiguration>();
            }
            foreach (SolrSearchFieldConfiguration fieldConfiguration in list)
            {
                SolrSearchFieldConfiguration item = fieldConfiguration;
                if (list1.Any<SolrSearchFieldConfiguration>((Func<SolrSearchFieldConfiguration, bool>)(c =>
                {
                    if (c != null)
                        return c.FieldNameFormat == item.FieldNameFormat;
                    return false;
                })))
                    return (AbstractSearchFieldConfiguration)item;
            }
            this.Debug(string.Format("Processing Field Name : {0}. Resolving Multiple Field found on Solr Field Map. No matching solr search field configuration on index field name '{1}', return type '{2}' and field type '{3}'. ", new object[4]
            {
        (object) fieldName,
        (object) strippedFieldName,
        (object) returnType.Name,
        (object) fieldTypeKey
            }) + string.Format("Returning the first configuration with Field Name format: {0}", (object)list.First<SolrSearchFieldConfiguration>().FieldNameFormat));
            return (AbstractSearchFieldConfiguration)list.First<SolrSearchFieldConfiguration>();
        }

        private string AggressiveProcessFieldName(string fieldName, Type returnType, string cultureCode, string returnTypeString = "", string fieldTypeKey = "", string defaultFieldName = "")
        {
            List<SolrSearchFieldConfiguration> source = this.AggressiveResolveFieldConfigurations(fieldName, returnType, cultureCode, returnTypeString, fieldTypeKey);
            if (source.Any<SolrSearchFieldConfiguration>())
                return source.First<SolrSearchFieldConfiguration>().FormatFieldName(fieldName, (ISearchIndexSchema)this.schema, cultureCode, this.settings.DefaultLanguage());
            return defaultFieldName;
        }

        private List<SolrSearchFieldConfiguration> AggressiveResolveFieldConfigurations(string fieldName, Type returnType, string cultureCode, string returnTypeString = "", string fieldTypeKey = "")
        {
            List<TemplateResolver> list = this.FindTemplateField(fieldName).ToList<TemplateResolver>();
            Action<List<SolrSearchFieldConfiguration>> action = (Action<List<SolrSearchFieldConfiguration>>)(configurations =>
            {
                if (!(returnType != (Type)null))
                    return;
                this.fieldMap.AddFieldByFieldName(fieldName, returnType, configurations);
            });
            if (list.Any<TemplateResolver>())
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (TemplateResolver templateResolver in list)
                    stringBuilder.AppendFormat("\t type: {0}\n", (object)templateResolver.Type);
                if (list.Count > 1)
                    this.Debug(string.Format("Processing Field Name : {0} Search field name in Solr with Template Resolver is returning multiple entry: \n{1}'", (object)fieldName, (object)stringBuilder));
                else
                    this.Debug(string.Format("Processing Field Name : {0} Search field name in Solr with Template Resolver is returning entry: \n{1}'", (object)fieldName, (object)stringBuilder));
                List<SolrSearchFieldConfiguration> fieldConfigurationList = new List<SolrSearchFieldConfiguration>();
                foreach (TemplateResolver templateResolver in list)
                {
                    if (templateResolver.Type != null)
                    {
                        SolrSearchFieldConfiguration configurationByFieldTypeName = (SolrSearchFieldConfiguration)this.fieldMap.GetFieldConfigurationByFieldTypeName(templateResolver.Type);
                        if (configurationByFieldTypeName == null)
                        {
                            this.Debug(string.Format("Processing Field Name : {0} Search field name in Solr with Template Resolver. Entry {1} is being skipped. Reason: No Field Type Name '{2}' found in field types section of solr configuration.", (object)fieldName, (object)templateResolver.Type, (object)templateResolver.Type));
                        }
                        else
                        {
                            SolrSearchFieldConfiguration fieldConfiguration = (SolrSearchFieldConfiguration)configurationByFieldTypeName.Clone();
                            fieldConfiguration.FieldTypeName = fieldConfiguration.FieldTypeName ?? templateResolver.Type;
                            fieldConfigurationList.Add(fieldConfiguration);
                        }
                    }
                }
                action(fieldConfigurationList);
                return fieldConfigurationList;
            }
            this.Debug(string.Format("Processing Field Name : {0} Search field name in Solr with Template Resolver is returning no entry.", (object)fieldName));
            action((List<SolrSearchFieldConfiguration>)new SolrFieldMap.NullReturnType());
            return new List<SolrSearchFieldConfiguration>();
        }

        [Obsolete]
        protected virtual string GetSearchFieldConfiguration(string fieldName, string cultureCode, SolrSearchFieldConfiguration configurationByType)
        {
            return configurationByType.FormatFieldName(fieldName, (ISearchIndexSchema)this.schema, cultureCode, this.settings.DefaultLanguage());
        }

        internal virtual IQueryable<TemplateResolver> FindTemplateField(string fieldName)
        {
            if (!this.index.Schema.AllFieldNames.Contains("_name") || !this.index.Schema.AllFieldNames.Contains("_templatename"))
            {
                CrawlingLog.Log.Error("Find Template Field failed. Solr schema fields has not been populated. Please populate solr schema under Populate Solr Managed Schema menu in control panel before perform indexing or searching.", (Exception)null);
                return new List<TemplateResolver>().AsQueryable<TemplateResolver>();
            }
            IQueryable<TemplateResolver> source = index.CreateSearchContext(SearchSecurityOptions.DisableSecurityCheck).GetQueryable<TemplateResolver>().Where(x => x.Name == fieldName).Filter(x => x.TemplateName == "Template field");
            if (source.Count() <= 1)
                return source;
            CrawlingLog.Log.Warn(string.Format("More than one template field matches. Index Name : {0} Field Name: {1}", (object)this.index.Name, (object)fieldName), (Exception)null);
            return source;
        }

        internal virtual void Debug(string message)
        {
            SearchLog.Log.Debug(message, (Exception)null);
        }

        internal virtual void Warn(string message)
        {
            SearchLog.Log.Warn(message, (Exception)null);
        }

        internal class TemplateResolver
        {
            [IndexField("_name")]
            public string Name { get; set; }

            [IndexField("_templatename")]
            public string TemplateName { get; set; }

            public string Type { get; set; }
        }
    }
}
