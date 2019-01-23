using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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

        public string Translate(List<MethodNode> methods)
        {
            var @params = new List<Tuple<string, string>>();
            foreach (var method in methods)
            {
                switch (method.Name)
                {
                    case "Where":
                        @params.Add(new Tuple<string, string>("q", TranslateBody(method.Body)));
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
                        if (method.Body != null)
                        {
                            @params.Add(new Tuple<string, string>("q", TranslateBody(method.Body)));
                        }
                        
                        //_compositeQuery.ScalarMethod = m.Method;
                        break;
                    case "Dismax":
                        @params.Add(new Tuple<string, string>("defType", "dismax"));
                        break;
                    default:
                        throw new NotSupportedException($"'{method.Name}' is not supported");
                }
            }

            if (!@params.Any(x => x.Item1 == "q"))
                @params.Insert(0, new Tuple<string, string>("q", "*:*"));

            var queryParams = new List<string>();
            var paramGroups = @params.GroupBy(x => x.Item1);
            foreach (var group in paramGroups)
            {
                switch (group.Key)
                {
                    case "q":
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

        protected virtual string TranslateBody(QueryNode node)
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

            throw new NotSupportedException($"'{type}' is not supported");
        }

        private string TranslateConstantNode(ConstantNode c)
        {
            if (c.Type == typeof(string))
            {
                return c.Value.ToString();
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
            var sb = new StringBuilder();

            var left = TranslateBody(b.LeftNode);
            var right = TranslateBody(b.RightNode);

            if (b.Type == ExpressionType.ArrayIndex)
            {
                //var left = GetValue(b.Left) as IList<object>;
                //var right = (int)((ConstantExpression)b.Right).Value;
                //var obj = left[right];
                //Visit(Expression.Constant(obj));
                //return b;
            }

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
