using Newtonsoft.Json;
using System;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class JsonVector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Vector3 result = Vector3.zero;
            result.x = (float)reader.ReadAsDouble();
            result.y = (float)reader.ReadAsDouble();
            result.z = (float)reader.ReadAsDouble();
            reader.Read();

            return result;
        }

        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(Math.Round(value.x, 3));
            writer.WriteValue(Math.Round(value.y, 3));
            writer.WriteValue(Math.Round(value.z, 3));
            writer.WriteEndArray();
        }
    }
}
