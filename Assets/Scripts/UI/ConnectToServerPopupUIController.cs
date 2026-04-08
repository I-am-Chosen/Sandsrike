using UnityEngine;
using UnityEngine.UI;

namespace HEAVYART.TopDownShooter.Netcode
{
    /// <summary>
    /// Optional popup that lets the player enter a custom server address.
    /// If you want a simpler flow (fixed server address from SO),
    /// just call MainGamePanelUIController.ConnectToServer() directly from a button.
    /// </summary>
    public class ConnectToServerPopupUIController : MonoBehaviour
    {
        [Header("UI References")]
        public InputField addressInputField;
        public InputField portInputField;
        public Button connectButton;
        public Text statusText;

        private void Start()
        {
            // Pre-fill with values from ServerNetworkSettings if available
            var settings = Resources.Load<ServerNetworkSettings>("ServerNetworkSettings");
            if (settings != null)
            {
                if (addressInputField != null) addressInputField.text = settings.serverAddress;
                if (portInputField != null)    portInputField.text    = settings.serverPort.ToString();
            }
            else
            {
                if (addressInputField != null) addressInputField.text = "sandsrike-server.onrender.com";
                if (portInputField != null)    portInputField.text    = "443";
            }
        }

        public void OnConnectButtonClicked()
        {
            string address = addressInputField != null ? addressInputField.text.Trim() : "localhost";
            string portStr = portInputField    != null ? portInputField.text.Trim()    : "443";

            if (string.IsNullOrEmpty(address))
            {
                SetStatus("Enter a server address.");
                return;
            }

            if (!ushort.TryParse(portStr, out ushort port))
            {
                SetStatus("Invalid port.");
                return;
            }

            SetStatus("Connecting...");
            if (connectButton != null) connectButton.interactable = false;

            LobbyManager.Instance.ConnectToDedicatedServer(address, port);
        }

        public void OnCancelButtonClicked()
        {
            MainMenuUIManager.Instance.ShowMainGamePanel();
        }

        private void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
        }
    }
}
