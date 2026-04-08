using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class GameplaySettings : MonoBehaviour
    {
        public string defaultGameSceneName;

        [Space]
        public float delayBeforeCountdown;
        public float countdownTime;
        public double gameDuration;
        public int botsCount;
        public float botsSpawnRate;

        private IEnumerator Start()
        {
#if UNITY_SERVER
            // Dedicated server does not use Unity Authentication Services.
            // Set the default scene from config and return immediately.
            PlayerDataKeeper.selectedScene = defaultGameSceneName;
            yield break;
#else
            if (DedicatedServerBootstrap.IsRunning)
            {
                PlayerDataKeeper.selectedScene = defaultGameSceneName;
                yield break;
            }

            yield return new WaitUntil(() => UnityServices.State == ServicesInitializationState.Initialized);
            yield return new WaitUntil(() => AuthenticationService.Instance.IsSignedIn == true);

            if (PlayerDataKeeper.selectedScene == "none")
                PlayerDataKeeper.selectedScene = defaultGameSceneName;
#endif
        }
    }
}
