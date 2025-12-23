using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace dotnetfsample.Converters
{
    public class SapDatsConverter : JsonConverter
    {
        private static readonly string[] Formats =
        {
            "yyyyMMdd",
            "yyyy-MM-dd"
        };

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.String)
            {
                var str = reader.Value == null ? null : reader.Value.ToString();

                if (string.IsNullOrWhiteSpace(str))
                    return null;

                DateTime dt;
                if (DateTime.TryParseExact(
                    str,
                    Formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out dt))
                {
                    return dt;
                }
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            // 핵심: Newtonsoft 기본 DateTime 직렬화에 위임
            serializer.Serialize(writer, value);
        }
    }
}