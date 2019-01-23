using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Common;
using SS.LinqToSolr.ExpressionParsers;
using SS.LinqToSolr.Sitecore.Translators;
using SS.LinqToSolr.Translators;
using System;
using System.Linq.Expressions;

namespace SS.LinqToSolr.Sitecore.ExpressionParsers
{
    /// <summary>
    /// https://doc.sitecore.com/developers/90/sitecore-experience-management/en/linq-to-sitecore.html
    /// </summary>
    //public class SitecoreExpressionParser : ExpressionParser
    //{
    //    public SitecoreExpressionParser(Type itemType, IFieldTranslator fieldTranslator) : base(itemType, fieldTranslator)
    //    {
    //    }

    //    protected override Expression VisitMethodCall(MethodCallExpression m)
    //    {
    //        if (m.Method.DeclaringType == typeof(QueryableExtensions))
    //            return VisitSitecoreQueryableExtensionMethod(m);
    //        return base.VisitMethodCall(m);
    //    }

    //    protected virtual Expression VisitSitecoreQueryableExtensionMethod(MethodCallExpression m)
    //    {
    //        switch (m.Method.Name)
    //        {
    //            case "Page":

    //                Visit(m.Arguments[0]);
    //                return m;
    //            case "Filter":
    //                return VisitFilter(m);
    //            case "FacetOn":
    //                return VisitFacet(m);
    //            case "FacetPivotOn":
    //                return VisitPivotFacet(m);
    //            case "GetFacets":
    //            case "GetResults":
    //                _compositeQuery.ScalarMethod = m.Method;
    //                Visit(m.Arguments[0]);
    //                return m;
    //            case "SelfJoin":

    //                Visit(m.Arguments[0]);
    //                return m;
    //            case "WithinRadius":

    //                Visit(m.Arguments[0]);
    //                return m;
    //            case "OrderByDistance":

    //                Visit(m.Arguments[0]);
    //                return m;
    //            case "OrderByDistanceDescending":

    //                Visit(m.Arguments[0]);
    //                return m;
    //            case "InContext":
    //                var executionContext = (IExecutionContext)((ConstantExpression)m.Arguments[1]).Value;
    //                ((FieldTranslator)_fieldTranslator).Accept(executionContext);
    //                Visit(m.Arguments[0]);
    //                return m;
    //            default:
    //                throw new NotSupportedException($"'{m.Method.Name}' is not supported");
    //        }
    //    }
    //}
}
