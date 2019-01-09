using SS.LinqToSolr.Extensions;
using SS.LinqToSolr.Models.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using SS.LinqToSolr.Translators;

namespace SS.LinqToSolr.ExpressionParsers
{
    public class DismaxExpressionParser : ExpressionVisitor
    {
        protected Type _itemType;
        protected DismaxCompositeQuery _compositeQuery;
        protected IFieldTranslator _fieldTranslator;

        public DismaxExpressionParser(Type itemType, IFieldTranslator fieldTranslator)
        {
            _itemType = itemType;
            _compositeQuery = new DismaxCompositeQuery();
            _fieldTranslator = fieldTranslator;
        }

        public virtual DismaxCompositeQuery Parse(Expression expression)
        {
            Visit(expression);
            return _compositeQuery;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "BoostQuery":
                    var boostQuery = New().Parse(m.Arguments[1]).GetQueryValue();
                    _compositeQuery.BoostQuery.Add(boostQuery);
                    Visit(m.Arguments[0]);
                    return m;

                case "DismaxQueryAlt":
                    var altQuery = New().Parse(m.Arguments[1]).GetQueryValue();
                    _compositeQuery.QueryAlt = altQuery;
                    Visit(m.Arguments[0]);
                    return m;
                default:
                    throw new NotSupportedException($"'{m.Method.Name}' is not supported");
            }
        }

        protected virtual ExpressionParser New()
        {
            return new ExpressionParser(_itemType, _fieldTranslator);
        }
    }
}