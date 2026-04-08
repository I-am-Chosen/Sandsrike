using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class LobbySettings : MonoBehaviour
    {
        public int defaultPlayerCount = 2;

        public int minPlayers = 1;
        public int maxPlayers = 4;

        [Header("Timeouts")]
        public float waitForPlayersToInitializeDelay = 2;
        public float waitForPlayersReadyResponseDelay = 2;
        public float waitForPlayersToRemoveDelay = 1;

        [Space()]
        public float lobbyHeartbeatRate = 20;
        public float autoRefreshRate = 10;

        [Space()]
        public int regionsUpdateRateHours = 24;

        [Space()]
        public List<string> webPlayerRegions = new List<string>()
        {
            "europe-central2",
            "europe-north1",
            "europe-west4",
            "us-east1",
            "us-central1",
            "us-west1",
            "southamerica-east1",
            "asia-northeast1",
            "asia-south1",
            "asia-southeast1",
            "australia-southeast1",
        };
    }
}
