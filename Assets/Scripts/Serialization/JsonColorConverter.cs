using Newtonsoft.Json;
using System;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class JsonColorConverter : JsonConverter<Color>
    {
        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Color result = Color.white;
            result.r = (float)reader.ReadAsDouble();
            result.g = (float)reader.ReadAsDouble();
            result.b = (float)reader.ReadAsDouble();
            result.a = (float)reader.ReadAsDouble();
            reader.Read();

            return result;
        }

        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(Math.Round(value.r, 3));
            writer.WriteValue(Math.Round(value.g, 3));
            writer.WriteValue(Math.Round(value.b, 3));
            writer.WriteValue(Math.Round(value.a, 3));
            writer.WriteEndArray();
        }
    }
}
