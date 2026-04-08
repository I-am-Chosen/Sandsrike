using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class CharacterIdentityControl : NetworkBehaviour
    {
        //Since we use scene objects as players, we need some tool to recognize if its player or bot 

        public bool isPlayer { get; private set; }
        public bool isBot { get; private set; }

        new public bool IsLocalPlayer => isPlayer && IsOwner;
        new public bool IsOwner => spawnParameters.ownerID == NetworkManager.Singleton?.LocalClientId;
        new public ulong OwnerClientId => spawnParameters.ownerID;

        public CharacterSpawnParameters spawnParameters;

        private NetworkVariable<FixedString128Bytes> serializedSpawnParameters = new NetworkVariable<FixedString128Bytes>(writePerm: NetworkVariableWritePermission.Server);
        private string serverBufferedSpawnParameters;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                serializedSpawnParameters.Value = serverBufferedSpawnParameters;

            spawnParameters = JsonConvert.DeserializeObject<CharacterSpawnParameters>(serializedSpawnParameters.Value.ToString());
        }

        public void SetSpawnParameters(string serializedSpawnParameters)
        {
            //It's server side
            //Prepare local copy of spawn parameters, before network spawn
            //Server will use it to initialize spawnParameters at OnNetworkSpawn()  
            serverBufferedSpawnParameters = serializedSpawnParameters;
        }

        private void Awake()
        {
            isPlayer = GetComponent<PlayerBehaviour>() != null;
            isBot = GetComponent<AIBehaviour>() != null;
        }
    }
}
