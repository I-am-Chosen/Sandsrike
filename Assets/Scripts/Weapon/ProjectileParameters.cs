using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class ProjectileParameters
    {
        [JsonProperty("sID")]
        public ulong senderID;

        [JsonProperty("s")]
        public float speed;

        [JsonProperty("t")]
        public double startTime;

        [JsonProperty("p")]
        public Vector3 startPosition;

        [JsonProperty("d")]
        public List<Vector3> directions = new List<Vector3>();

        [JsonProperty("c")]
        public List<CommandScriptableObject> commands = new List<CommandScriptableObject>();

        public void ClearCommands()
        {
            //Destroy instantiated scriptable objects
            commands.ForEach(command => Object.Destroy(command));
            commands.Clear();
        }
    }
}
