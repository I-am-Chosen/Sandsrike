using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    /// <summary>
    /// ScriptableObject holding dedicated server connection parameters.
    /// Create via: Assets → Create → Game → ServerNetworkSettings
    ///
    /// Client-side values (what the game client uses to connect):
    ///   serverAddress   — Render.com service URL or raw IP
    ///   serverPort      — 443 for Render.com (TLS proxy), or raw port for direct
    ///
    /// Server-side defaults (overridden by env vars / CLI args at runtime):
    ///   defaultServerPort   — the port the Unity container listens on
    ///   maxPlayersPerServer — max clients per server instance
    ///   defaultMap          — scene to load on server start
    /// </summary>
    [CreateAssetMenu(fileName = "ServerNetworkSettings", menuName = "Game/ServerNetworkSettings")]
    public class ServerNetworkSettings : ScriptableObject
    {
        [Header("Client Connection")]
        [Tooltip("Render.com service URL, e.g. sandsrike.onrender.com")]
        public string serverAddress = "sandsrike.onrender.com";

        [Tooltip("443 for Render.com (WSS proxy). Use raw port for LAN/testing.")]
        public ushort serverPort = 443;

        [Header("Server Defaults (overridden at runtime)")]
        [Tooltip("Port the container listens on (Render sets this via PORT env var).")]
        public ushort defaultServerPort = 7777;

        public int maxPlayersPerServer = 10;
        public string defaultMap = "GameScene";
    }
}
