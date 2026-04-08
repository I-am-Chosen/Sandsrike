using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class PickUpItemController : NetworkBehaviour
    {
        //Scriptable object file reference 
        public CommandScriptableObject container;

        private void OnTriggerEnter(Collider other)
        {
            if (NetworkManager.Singleton.IsServer == false) return;

            if (container == null) return;

            CommandsControlSystem commandsControlSystem = other.transform.GetComponent<CommandsControlSystem>();

            if (commandsControlSystem != null)
            {
                //Pick up drop element and broadcast message. It's server side.

                string serializedCommand = JsonConvert.SerializeObject(new List<CommandScriptableObject>() { container });
                commandsControlSystem.ReceiveCommandsRpc(serializedCommand);

                NetworkObject.Despawn(true);
            }
        }
    }
}
