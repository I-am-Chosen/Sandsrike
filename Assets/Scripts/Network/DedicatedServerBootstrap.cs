using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    /// <summary>
    /// Bootstraps the dedicated server on startup.
    ///
    /// Auto-created at runtime — no manual scene setup required.
    /// Activation: UNITY_SERVER build target OR command-line flag: -server 1
    ///
    /// Command-line arguments (all optional):
    ///   -port 7777          WebSocket port to listen on
    ///   -maxPlayers 10      Max concurrent players per instance
    ///   -map GameScene      Scene name to load on start
    ///
    /// Environment variables (Render.com injects these):
    ///   PORT                Overrides default port (Render sets this to 10000)
    /// </summary>
    public class DedicatedServerBootstrap : MonoBehaviour
    {
        private const ushort DefaultPort = 7777;
        private const string DefaultMap = "GameScene";
        private const int DefaultMaxPlayers = 10;

        public static bool IsRunning { get; private set; }
        public static ushort ActivePort { get; private set; }
        public static int MaxPlayers { get; private set; }

        // ── Auto-create at runtime (no scene setup needed) ────────────────────

        // BeforeSceneLoad: set IsRunning early so other components' Start() can read it.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SetServerFlag()
        {
#if UNITY_SERVER
            IsRunning = true;
#else
            if (ShouldRunAsServer()) IsRunning = true;
#endif
        }

        // AfterSceneLoad: NetworkManager is now in the scene, safe to boot.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (!IsRunning) return;
            CreateBootstrap();
        }

        private static bool ShouldRunAsServer()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
                if (args[i] == "-server" && i + 1 < args.Length && args[i + 1] == "1")
                    return true;
            return false;
        }

        private static void CreateBootstrap()
        {
            if (FindFirstObjectByType<DedicatedServerBootstrap>() != null)
                return; // Already exists in scene

            var go = new GameObject("[DedicatedServerBootstrap]");
            DontDestroyOnLoad(go);
            go.AddComponent<DedicatedServerBootstrap>();
        }

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void Awake()
        {
            if (IsRunning) { Destroy(gameObject); return; } // Guard against duplicates
            Boot();
        }

        private void Boot()
        {
            IsRunning = true;
            ActivePort = ResolvePort();
            MaxPlayers = ResolveMaxPlayers();
            string map = ResolveMap();

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.UseWebSockets = true;
            transport.SetConnectionData("0.0.0.0", ActivePort);

            Debug.Log($"[DedicatedServer] Starting | Port: {ActivePort} | MaxPlayers: {MaxPlayers} | Map: {map}");

            NetworkManager.Singleton.StartServer();
            SceneLoadManager.Instance.LoadNetworkScene(map);
        }

        // ── Config resolution: env var → CLI arg → default ──────────────��─────

        private static ushort ResolvePort()
        {
            string envPort = System.Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(envPort) && ushort.TryParse(envPort, out ushort fromEnv))
                return fromEnv;
            if (CommandLineHelper.TryGetArgumentValue("port", out string portArg)
                && ushort.TryParse(portArg, out ushort fromArg))
                return fromArg;
            return DefaultPort;
        }

        private static int ResolveMaxPlayers()
        {
            if (CommandLineHelper.TryGetArgumentValue("maxPlayers", out string arg)
                && int.TryParse(arg, out int value))
                return value;
            return DefaultMaxPlayers;
        }

        private static string ResolveMap()
        {
            if (CommandLineHelper.TryGetArgumentValue("map", out string arg)
                && !string.IsNullOrEmpty(arg))
                return arg;
            return DefaultMap;
        }
    }
}
