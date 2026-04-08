using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ScriptableObjectJson : ScriptableObject
    {
        [JsonProperty("type")]
        public string type { get; protected set; }

        public static Dictionary<string, Type> registeredTypes = new Dictionary<string, Type>();

        private static Dictionary<Type, string> registeredAliases = new Dictionary<Type, string>();

        public static void RegisterTypeAlias(string alias, Type type)
        {
            if (registeredTypes.TryAdd(alias, type) == false)
            {
                Debug.LogException(new Exception($"Alias <b>{alias}</b> for type <b>{type.Name}</b> already registered."));
                return;
            }

            registeredAliases.TryAdd(type, alias);
        }

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context)
        {
            if (registeredAliases.TryGetValue(GetType(), out string alias))
                type = alias;
            else
                type = GetType().Name;
        }

        public static void ClearRegister()
        {
            registeredTypes = new Dictionary<string, Type>();
            registeredAliases = new Dictionary<Type, string>();
        }
    }
}
