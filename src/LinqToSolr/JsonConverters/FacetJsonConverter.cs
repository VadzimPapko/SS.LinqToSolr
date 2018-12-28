using SS.LinqToSolr.Model;
using SS.LinqToSolr.Model.SearchResponse;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SS.LinqToSolr.JsonConverters
{
    public class FacetJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var jsonObject = JObject.Load(reader);

            var facets = new Dictionary<string, IEnumerable<IFacet>>();
            foreach (var pr in jsonObject.Properties())
            {
                if (pr.Name == "facet_fields")
                {
                    var fs = pr.Children().SelectMany(x => x.Children()).ToList();
                    foreach (JProperty f in fs)
                    {
                        var result = new List<FacetValue>();
                        var values = f.Values().ToList();
                        for (var i = 0; i < values.Count; i += 2)
                        {
                            result.Add(new FacetValue
                            {
                                Value = values[i].Value<string>(),
                                Count = values[i + 1].Value<int>()
                            });
                        }

                        facets.Add(f.Name, result);
                    }
                    continue;
                }
                if (pr.Name == "facet_pivot")
                {
                    var fs = pr.Children().SelectMany(x => x.Children()).ToList();
                    foreach (JProperty f in fs)
                    {
                        var result = new List<PivotFacetValue>();
                        var values = f.Values().ToList();
                        foreach (var val in values)
                        {
                            var facet = new PivotFacetValue();
                            ParsePivot(facet, val);
                            result.Add(facet);
                        }
                        facets.Add(f.Name, result);
                    }
                    continue;
                }
            }

            return facets;
        }

        private void ParsePivot(PivotFacetValue facet, JToken json)
        {            
            var props = json.Children().ToList();
            facet.Field = props[0].Values().First().Value<string>();
            facet.Value = props[1].Values().First().Value<string>();
            facet.Count = props[2].Values().First().Value<int>();
            if (props.Count == 4)
            {
                var pivots = props[3].Children().SelectMany(x => x.Children());
                foreach (var pivot in pivots)
                {                   
                    var cfacet = new PivotFacetValue();
                    ParsePivot(cfacet, pivot);
                    facet.Pivot.Add(cfacet);
                }                
            }            
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}