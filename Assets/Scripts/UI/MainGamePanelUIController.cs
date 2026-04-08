using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class MainGamePanelUIController : MonoBehaviour
    {
        /// <summary>
        /// Connect to the dedicated server on Render.com.
        /// Address is read from Resources/ServerNetworkSettings.asset.
        /// Assign this to your "PLAY" button's OnClick event.
        /// </summary>
        public void ConnectToServer()
        {
            LobbyManager.Instance.ConnectToDedicatedServer();
            MainMenuUIManager.Instance.ShowConnectingToServerPopup();
        }

        /// <summary>
        /// Legacy Relay/Lobby quick-match (P2P via Unity Services).
        /// Kept for local testing and fallback.
        /// </summary>
        public void StartQuickGame()
        {
            LobbyParameters lobbyParameters = new LobbyParameters();
            lobbyParameters.playersCount = SettingsManager.Instance.lobby.defaultPlayerCount;
            lobbyParameters.version = SettingsManager.Instance.common.projectVersion;

            LobbyManager.Instance.JoinOrCreateLobby(lobbyParameters);
            MainMenuUIManager.Instance.ShowWaitingForPublicGamePopup();
        }
    }
}
