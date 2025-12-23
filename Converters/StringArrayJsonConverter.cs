using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dotnetfsample.Converters
{
    public class SingleArrayJsonConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // null 처리
            if (reader.TokenType == JsonToken.Null)
                return null; // 또는 Array.Empty<T>()

            // 토큰을 한 번 로드해서 타입 판별
            var token = JToken.Load(reader);

            // [ ... ] 배열이면 그대로 배열로
            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<T[]>(serializer);
            }

            // 단일 값/객체면 길이 1 배열로 감싸기
            var single = token.ToObject<T>(serializer);
            return new[] { single };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // value는 T[] 여야 함
            var arr = value as T[];

            if (arr == null)
            {
                writer.WriteNull();
                return;
            }

            // 항상 배열로 출력
            writer.WriteStartArray();
            foreach (var item in arr)
            {
                serializer.Serialize(writer, item);
            }
            writer.WriteEndArray();
        }
    }
}