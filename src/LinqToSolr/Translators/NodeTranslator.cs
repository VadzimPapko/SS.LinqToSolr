using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SS.LinqToSolr.Extensions;
using SS.LinqToSolr.Models.Query;

namespace SS.LinqToSolr.Translators
{
    public class NodeTranslator : INodeTranslator
    {
        protected IFieldTranslator _fieldTranslator;
        public NodeTranslator(IFieldTranslator fieldTranslator)
        {
            _fieldTranslator = fieldTranslator;
        }

        public virtual string Translate(List<MethodNode> methods, out string scalarMethodName)
        {
            scalarMethodName = null;
            QueryParser parser = QueryParser.Default;
            var @params = new List<Tuple<string, string>>();
            foreach (var method in methods)
            {
                @params.AddRange(TranslateMethod(method, ref parser, out scalarMethodName));
            }

            if (@params.Any(x => x.Item1 == "facet.field" || x.Item1 == "facet.pivot"))
                @params.Add(new Tuple<string, string>("facet", "on"));

            if (parser == QueryParser.Dismax)
            {
                if (!@params.Any(x => x.Item1 == "q") && !@params.Any(x => x.Item1 == "q.alt"))
                {
                    @params.Insert(0, new Tuple<string, string>("q.alt", "*:*"));
                }
                @params.Insert(0, new Tuple<string, string>("defType", "dismax"));
            }
            else if (parser == QueryParser.Default && !@params.Any(x => x.Item1 == "q"))
                @params.Insert(0, new Tuple<string, string>("q", "*:*"));

            var queryParams = new List<string>();
            var paramGroups = @params.GroupBy(x => x.Item1);
            foreach (var group in paramGroups)
            {
                switch (group.Key)
                {
                    case "q":
                    case "fq":
                        IEnumerable<string> result = null;
                        if (group.Count() > 1)
                            result = group.Select(x => $"({x.Item2})");
                        else
                            result = group.Select(x => x.Item2);
                        queryParams.Add($"{group.Key}={string.Join(" AND ", result)}");
                        break;
                    case "sort":
                        queryParams.Add($"{group.Key}={string.Join(",", group.Select(x => x.Item2))}");
                        break;
                    default:
                        foreach (var param in group)
                        {
                            queryParams.Add($"{group.Key}={param.Item2}");
                        }
                        break;
                }
            }

            return string.Join("&", queryParams);
        }

        public virtual List<Tuple<string, string>> TranslateMethod(MethodNode method, ref QueryParser parser, out string scalarMethodName)
        {
            scalarMethodName = null;
            var @params = new List<Tuple<string, string>>();
            switch (method.Name)
            {
                case "Query":
                case "Where":
                    @params.Add(new Tuple<string, string>("q", TranslateBody(method.Body)));
                    break;
                case "Filter":
                    @params.Add(new Tuple<string, string>("fq", TranslateBody(method.Body)));
                    break;
                case "Facet":
                    var facet = (FacetNode)method.Body;
                    var facetFieldName = TranslateBody(facet.Field);
                    if (facet.IsMultiFacet && facet.Values.Any())
                    {
                        @params.Add(new Tuple<string, string>("facet.field", $"{{!ex={facetFieldName}}}{facetFieldName}"));
                        @params.Add(new Tuple<string, string>("fq", $"{{!tag={facetFieldName}}}{facetFieldName}:{string.Join(" OR ", facet.Values).ToSolrGroup()}"));
                    }
                    else
                    {
                        @params.Add(new Tuple<string, string>("facet.field", facetFieldName));
                    }
                    break;
                case "PivotFacet":
                    var pivotFacet = (PivotFacetNode)method.Body;
                    var fields = pivotFacet.Facets.Select(x => TranslateBody(((FacetNode)x.Body).Field));
                    @params.Add(new Tuple<string, string>("facet.pivot", string.Join(",", fields)));
                    break;
                case "OrderBy":
                case "ThenBy":
                case "OrderByDescending":
                case "ThenByDescending":
                    var field = TranslateBody(method.Body);
                    var direction = method.Name.EndsWith("Descending") ? "desc" : "asc";
                    @params.Add(new Tuple<string, string>("sort", $"{field} {direction}"));
                    break;
                case "Take":
                    @params.Add(new Tuple<string, string>("rows", TranslateBody(method.Body)));
                    break;
                case "Skip":
                    @params.Add(new Tuple<string, string>("start", TranslateBody(method.Body)));
                    break;
                case "Count":
                case "First":
                case "FirstOrDefault":
                case "Last":
                case "LastOrDefault":
                case "Single":
                case "SingleOrDefault":
                    if (method.Body != null)
                    {
                        @params.Add(new Tuple<string, string>("q", TranslateBody(method.Body)));
                    }
                    scalarMethodName = method.Name;
                    break;
                case "GetResponse":
                    scalarMethodName = method.Name;
                    break;
                case "Dismax":
                    parser = QueryParser.Dismax;
                    break;
                case "BoostQuery":
                    @params.Add(new Tuple<string, string>("bq", TranslateBody(method.Body)));
                    break;
                case "DismaxQueryAlt":
                    @params.Add(new Tuple<string, string>("q.alt", TranslateBody(method.Body)));
                    break;
                default:
                    throw new NotSupportedException($"'{method.Name}' is not supported");
            }
            return @params;
        }

        protected virtual string TranslateBody(IQueryNode node)
        {
            var type = node.GetType();
            if (type == typeof(BinaryNode))
            {
                return TranslateBinaryNode((BinaryNode)node);
            }
            else if (type == typeof(MemberNode))
            {
                return TranslateMemberNode((MemberNode)node);
            }
            else if (type == typeof(ConstantNode))
            {
                return TranslateConstantNode((ConstantNode)node);
            }
            else if (type == typeof(MethodNode))
            {
                return TranslateSubMethod((MethodNode)node);
            }

            throw new NotSupportedException($"'{type}' is not supported");
        }

        protected virtual string TranslateSubMethod(MethodNode node)
        {
            switch (node.Name)
            {
                case "Boost":
                case "ConstantScore":
                    return $"({TranslateBody(node.Body)}){TranslateBody(node.SubBody)}";
                case "Fuzzy":
                case "Proximity":
                    return $"{TranslateBody(node.Body)}{TranslateBody(node.SubBody)}";
                default:
                    return TranslateBody(node.Body);
            }
        }

        protected virtual string TranslateConstantNode(ConstantNode c)
        {
            if (c.Type == typeof(string))
            {
                var result = c.Value.ToString();
                //for phrases
                if (result.Contains(" "))
                {
                    return $"\"{result.Trim('"')}\"";
                }
                return result;
            }
            else if (c.Type == typeof(DateTime))
            {
                return ((DateTime)c.Value).ToString("yyyy-MM-ddThh:mm:ss.fffZ");
            }
            else if (c.Type == typeof(float))
            {
                return ((float)c.Value).ToString();
            }
            else if (c.Type == typeof(int))
            {
                return ((int)c.Value).ToString();
            }

            throw new NotSupportedException($"'{c.Type}' is not supported");
        }

        protected virtual string TranslateMemberNode(MemberNode m)
        {
            return _fieldTranslator.Translate(m.Member);
        }

        protected virtual string TranslateBinaryNode(BinaryNode b)
        {
            //for ignore PredicateBuilder first node
            if (b.LeftNode is ConstantNode && ((ConstantNode)b.LeftNode).Type == typeof(bool))
            {
                return TranslateBody(b.RightNode);
            }

            var sb = new StringBuilder();

            var left = TranslateBody(b.LeftNode);
            var right = TranslateBody(b.RightNode);

            if (b.Type == ExpressionType.NotEqual)
            {
                sb.Append("-");
            }

            sb.Append(left);

            switch (b.Type)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    sb.Append(" AND ");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    sb.Append(" OR ");
                    break;
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    sb.Append(":");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(":[");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append(":[* TO ");
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append(":{");
                    break;
                case ExpressionType.LessThan:
                    sb.Append(":[* TO ");
                    break;
                default:
                    throw new NotSupportedException($"'{ b.Type}' is not supported");
            }

            sb.Append(right);

            switch (b.Type)
            {
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" TO *]");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append("]");
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append(" TO *]");
                    break;
                case ExpressionType.LessThan:
                    sb.Append("}");
                    break;
            }

            return sb.ToString();
        }
    }
}
