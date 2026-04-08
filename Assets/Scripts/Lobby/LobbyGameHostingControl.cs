using System.Collections.Generic;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Qos;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using System;
using Unity.Services.Core;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class LobbyGameHostingControl
    {
        private LobbyDataControl dataControl;
        private UnityTransport unityTransport;

        public event Action OnRelayAllocationError;

        public List<string> availableRegions { get; private set; } = new List<string>();

        public LobbyGameHostingControl(LobbyDataControl dataControl, UnityTransport unityTransport)
        {
            this.dataControl = dataControl;
            this.unityTransport = unityTransport;
            UpdateRegions();
        }

        public async Task HostGame(int playersCount)
        {
            string selectedRegion = PlayerDataKeeper.selectedRegion;
            string joinCode;

            try
            {
                //Allocate game session in Relay service
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(playersCount, selectedRegion);

                //Get join code for other players to connect this game
                joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

#if UNITY_WEBGL
                unityTransport.UseWebSockets = true;
#endif

                if (unityTransport.UseWebSockets == true)
                    unityTransport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "wss"));
                else
                    unityTransport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                OnRelayAllocationError?.Invoke();
                return;
            }

            //Start game session
            NetworkManager.Singleton.StartHost();

            //Load game scene
            SceneLoadManager.Instance.LoadNetworkScene(PlayerDataKeeper.selectedScene);

            //Store join code in lobby
            UpdateLobbyOptions options = new UpdateLobbyOptions();
            options.Data = new Dictionary<string, DataObject>()
            {
                {
                  "joinCode", new DataObject(
                  visibility: DataObject.VisibilityOptions.Member,
                  value: joinCode)
                }
            };

            //Send join code
            dataControl.currentLobby = await LobbyService.Instance.UpdateLobbyAsync(dataControl.currentLobby.Id, options);
        }

        public async void JoinHostedGame()
        {
            //Get join code from lobby
            string joinCode = dataControl.currentLobby.Data["joinCode"].Value;

            try
            {
                //Find and join game session
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

#if UNITY_WEBGL
                unityTransport.UseWebSockets = true;
#endif

                //Link current network client to allocated game
                if (unityTransport.UseWebSockets == true)
                    unityTransport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "wss"));
                else
                    unityTransport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                OnRelayAllocationError?.Invoke();
                return;
            }

            //Join game session as client
            NetworkManager.Singleton.StartClient();

            //In this case we don't load scene manually.
            //Server automatically synchronizes active scene with all connected clients instead.
            SceneLoadManager.Instance.SubscribeOnNetworkEvents();
        }

        private async void UpdateRegions()
        {
            //Update regions once in few days
            if (PlayerDataKeeper.lastRegionsUpdateTime + new TimeSpan(SettingsManager.Instance.lobby.regionsUpdateRateHours, 0, 0) > DateTime.Now)
            {
                availableRegions = PlayerDataKeeper.availableRegions;
                return;
            }

            //Wait for initialization
            while (UnityServices.State != ServicesInitializationState.Initialized)
                await Awaitable.FixedUpdateAsync();

            //Wait for sign in
            while (AuthenticationService.Instance.IsSignedIn == false)
                await Awaitable.FixedUpdateAsync();

#if UNITY_WEBGL
            availableRegions = SettingsManager.Instance.lobby.webPlayerRegions;
#else
            //Get regions
            var regionSearchResult = await QosService.Instance.GetSortedQosResultsAsync("relay", null);

            foreach (var result in regionSearchResult)
            {
                Debug.Log("Add region: " + result.Region);
                availableRegions.Add(result.Region);
            }
#endif

            //Save regions
            PlayerDataKeeper.availableRegions = availableRegions;
            PlayerDataKeeper.selectedRegion = availableRegions[0];
            PlayerDataKeeper.lastRegionsUpdateTime = DateTime.Now;
        }
    }
}
