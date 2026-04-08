using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    /// <summary>
    /// Bootstraps the dedicated server on startup.
    /// Attach to the NetworkManager GameObject in the Boot scene.
    ///
    /// Activation priority:
    ///   1. UNITY_SERVER build target (headless Linux build)
    ///   2. Command-line flag:  -server 1
    ///
    /// Command-line arguments (all optional):
    ///   -port 7777          UDP/WebSocket port to listen on
    ///   -maxPlayers 10      Max concurrent players per instance
    ///   -map GameScene      Scene name to load on start
    ///
    /// Environment variables (Render.com injects these):
    ///   PORT                Overrides default port (Render sets this to 10000)
    /// </summary>
    public class DedicatedServerBootstrap : MonoBehaviour
    {
        [SerializeField] private ushort defaultPort = 7777;
        [SerializeField] private string defaultMap = "GameScene";
        [SerializeField] private int defaultMaxPlayers = 10;

        public static bool IsRunning { get; private set; }
        public static ushort ActivePort { get; private set; }
        public static int MaxPlayers { get; private set; }

        private void Awake()
        {
#if UNITY_SERVER
            Boot();
#else
            if (CommandLineHelper.TryGetArgumentValue("server", out _))
                Boot();
#endif
        }

        private void Boot()
        {
            IsRunning = true;
            ActivePort = ResolvePort();
            MaxPlayers = ResolveMaxPlayers();
            string map = ResolveMap();

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            // WebSocket transport: runs over TCP, works behind Render.com's load balancer.
            // The load balancer handles TLS; the container speaks plain WS internally.
            transport.UseWebSockets = true;
            transport.SetConnectionData("0.0.0.0", ActivePort);

            Debug.Log($"[DedicatedServer] Starting | Port: {ActivePort} | MaxPlayers: {MaxPlayers} | Map: {map}");

            NetworkManager.Singleton.StartServer();
            SceneLoadManager.Instance.LoadNetworkScene(map);
        }

        // ── Port resolution: env var → CLI arg → default ──────────────────────

        private ushort ResolvePort()
        {
            // Render.com injects PORT env variable (typically 10000)
            string envPort = System.Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(envPort) && ushort.TryParse(envPort, out ushort fromEnv))
                return fromEnv;

            if (CommandLineHelper.TryGetArgumentValue("port", out string portArg)
                && ushort.TryParse(portArg, out ushort fromArg))
                return fromArg;

            return defaultPort;
        }

        private int ResolveMaxPlayers()
        {
            if (CommandLineHelper.TryGetArgumentValue("maxPlayers", out string arg)
                && int.TryParse(arg, out int value))
                return value;
            return defaultMaxPlayers;
        }

        private string ResolveMap()
        {
            if (CommandLineHelper.TryGetArgumentValue("map", out string arg)
                && !string.IsNullOrEmpty(arg))
                return arg;
            return defaultMap;
        }
    }
}
