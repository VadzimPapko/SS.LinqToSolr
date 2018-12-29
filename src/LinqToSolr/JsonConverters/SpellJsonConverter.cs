using SS.LinqToSolr.Models.SearchResponse;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SS.LinqToSolr.JsonConverters
{
    public class SpellJsonConverter : JsonConverter
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

            var spellcheck = new List<Spellcheck>();
            foreach (var pr in jsonObject.Properties())
            {
                if (pr.Name == "suggestions")
                {
                    var wordSuggestions = pr.Children().ToList();
                    foreach (var wordSuggestion in wordSuggestions)
                    {
                        var words = wordSuggestion.Last?.Last?.Last?.Children().ToList();
                        if (words != null)
                            foreach (var word in words)
                            {
                                spellcheck.Add(new Spellcheck
                                {
                                    Word = word.First.Last.Value<string>(),
                                    Frequency = word.Last.Last.Value<int>()
                                });
                            }
                    }
                }
            }

            return spellcheck.OrderByDescending(x => x.Frequency);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            new NotImplementedException();
        }
    }
}