using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class CharacterSpawnParameters
    {
        [JsonProperty("oID")]
        public ulong ownerID;

        [JsonProperty("n")]
        public string name = string.Empty;

        [JsonProperty("c")]
        public Color color;

        [JsonProperty("i")]
        public int modelIndex;

        //Add custom parameters here
    }
}
