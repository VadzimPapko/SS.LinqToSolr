using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Common;
using SS.LinqToSolr.Extensions;
using SS.LinqToSolr.Models.Query;
using SS.LinqToSolr.Translators;

namespace SS.LinqToSolr.Sitecore.Translators
{
    public class SitecoreNodeTranslator : NodeTranslator
    {
        public SitecoreNodeTranslator(IFieldTranslator fieldTranslator) : base(fieldTranslator)
        {
        }

        public override string Translate(List<MethodNode> methods, out string scalarMethodName)
        {
            var inContext = methods.FirstOrDefault(x=>x.Name == "InContext");
            if(inContext != null)
            {
                var executionContext = (CultureExecutionContext)((ConstantNode)inContext.Body).Value;
                _fieldTranslator.Accept(executionContext.Culture);
            }

            return base.Translate(methods, out scalarMethodName);
        }

        public override List<Tuple<string, string>> TranslateMethod(MethodNode method, ref QueryParser parser, out string scalarMethodName)
        {
            scalarMethodName = null;
            var @params = new List<Tuple<string, string>>();

           

            switch (method.Name)
            {
                case "InContext":
                    break;
                default:
                    @params.AddRange(base.TranslateMethod(method, ref parser, out scalarMethodName));
                    break;
            }
            return @params;
        }
    }
}
