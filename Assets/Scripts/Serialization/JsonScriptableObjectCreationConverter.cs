using System;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class JsonScriptableObjectCreator : CustomCreationConverter<ScriptableObjectJson>
    {
        public override ScriptableObjectJson Create(Type type)
        {
            return ScriptableObject.CreateInstance(type) as ScriptableObjectJson;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var jsonType = jObject["type"]?.ToString();

            Type scriptableObjectType;

            if (ScriptableObjectJson.registeredTypes.TryGetValue(jsonType, out Type registeredType))
                scriptableObjectType = registeredType;
            else
            {
                string typeName = $"{typeof(ScriptableObjectJson).Namespace}.{jsonType}";
                scriptableObjectType = Type.GetType(typeName);
            }

            ScriptableObjectJson scriptableObjectJSON = Create(scriptableObjectType);

            reader = jObject.CreateReader();
            serializer.Populate(reader, scriptableObjectJSON);

            return scriptableObjectJSON;
        }
    }
}