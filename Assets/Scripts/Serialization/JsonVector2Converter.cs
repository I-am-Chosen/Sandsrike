using Newtonsoft.Json;
using System;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class JsonVector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Vector3 result = Vector3.zero;
            result.x = (float)reader.ReadAsDouble();
            result.y = (float)reader.ReadAsDouble();
            reader.Read();

            return result;
        }

        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(Math.Round(value.x, 3));
            writer.WriteValue(Math.Round(value.y, 3));
            writer.WriteEndArray();
        }
    }
}
