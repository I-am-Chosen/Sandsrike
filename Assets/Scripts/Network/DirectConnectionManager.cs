using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HEAVYART.TopDownShooter.Netcode
{
    /// <summary>
    /// Connects the local client directly to a dedicated server, bypassing Unity Relay.
    ///
    /// Auto-created at runtime — no manual scene setup required.
    ///
    /// Settings priority for server address:
    ///   1. ServerNetworkSettings asset in Resources/ServerNetworkSettings
    ///   2. Hard-coded defaults (localhost:7777) for local testing
    ///
    /// Create the asset: Assets → Create → Game → ServerNetworkSettings
    /// Then move it to:  Assets/Resources/ServerNetworkSettings.asset
    /// </summary>
    public class DirectConnectionManager : MonoBehaviour
    {
        private static DirectConnectionManager _instance;
        public static DirectConnectionManager Instance => _instance;

        // Loaded from Resources/ServerNetworkSettings.asset (optional)
        private ServerNetworkSettings _settings;

        // ── Auto-create at runtime ─────────────────────────────────────────────

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (DedicatedServerBootstrap.IsServerBuild) return;

            if (FindFirstObjectByType<DirectConnectionManager>() != null)
                return;

            var go = new GameObject("[DirectConnectionManager]");
            DontDestroyOnLoad(go);
            go.AddComponent<DirectConnectionManager>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;

            // Try to load settings from Resources — won't throw if file doesn't exist
            _settings = Resources.Load<ServerNetworkSettings>("ServerNetworkSettings");

            if (_settings != null)
                Debug.Log($"[DirectConnection] Settings loaded: {_settings.serverAddress}:{_settings.serverPort}");
            else
                Debug.Log("[DirectConnection] No ServerNetworkSettings in Resources — using defaults (localhost:7777)");
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Connect using ServerNetworkSettings (or defaults if not found).</summary>
        public void Connect()
        {
            string address = _settings != null ? _settings.serverAddress : "localhost";
            ushort port    = _settings != null ? _settings.serverPort    : (ushort)7777;
            Connect(address, port);
        }

        /// <summary>Connect to an explicit address and port.</summary>
        public void Connect(string serverAddress, ushort serverPort)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.UseWebSockets = true;

            // Port 443 means the client connects via WSS (WebSocket Secure).
            // Render.com terminates TLS at the proxy and forwards plain WS to the container.
            if (serverPort == 443)
            {
                transport.UseEncryption = true;
                transport.SetClientSecrets(serverAddress); // SNI + system root CA verification
            }

            transport.SetConnectionData(serverAddress, serverPort);

            Debug.Log($"[DirectConnection] Connecting to {serverAddress}:{serverPort}");

            // Show loading screen immediately so MainMenu is gone before the network scene loads.
            // Must happen BEFORE StartClient so the old scene is already unloaded when OnLoad fires.
            SceneManager.LoadScene("LoadingScene");

            // StartClient initializes NetworkManager.SceneManager synchronously.
            NetworkManager.Singleton.StartClient();

            // Subscribe immediately after StartClient — SceneManager is now initialized,
            // and OnSynchronize hasn't fired yet (it fires in a later network tick).
            SceneLoadManager.Instance.SubscribeOnNetworkEvents();
        }
    }
}
