using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class CommandScriptableObject : ScriptableObjectJson
    {
        [JsonProperty("st")]
        public Status status { get; protected set; }

        public bool isComplete => status == Status.Complete;
        public bool isActive => status == Status.Active;

        public virtual void Activate(CommandsControlSystem commandTarget)
        {
            status = Status.Active;
        }

        public virtual void Complete()
        {
            status = Status.Complete;
        }

        public virtual void FixedUpdate()
        {
        }

        public enum Status
        {
            Inactive,
            Active,
            Complete
        }
    }
}
