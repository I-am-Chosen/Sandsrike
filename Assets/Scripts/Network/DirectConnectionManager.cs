using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    /// <summary>
    /// Connects the local client directly to a dedicated server,
    /// bypassing Unity Relay entirely.
    ///
    /// Usage from LobbyManager or UI:
    ///   DirectConnectionManager.Instance.Connect(settings.serverAddress, settings.serverPort);
    ///
    /// For Render.com deployments, serverAddress is the service URL
    /// (e.g. "sandsrike.onrender.com") and serverPort is 443.
    /// Render.com terminates TLS and proxies the WebSocket connection
    /// to the Unity server container on its internal PORT.
    /// </summary>
    public class DirectConnectionManager : Singleton<DirectConnectionManager>
    {
        [SerializeField] private ServerNetworkSettings settings;

        /// <summary>Connect using the ScriptableObject config (default).</summary>
        public void Connect() => Connect(settings.serverAddress, settings.serverPort);

        /// <summary>Connect to an explicit address/port.</summary>
        public void Connect(string serverAddress, ushort serverPort)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.UseWebSockets = true;
            transport.SetConnectionData(serverAddress, serverPort);

            Debug.Log($"[DirectConnection] Connecting to {serverAddress}:{serverPort}");

            // Let SceneLoadManager sync scenes from the server
            SceneLoadManager.Instance.SubscribeOnNetworkEvents();
            NetworkManager.Singleton.StartClient();
        }
    }
}
