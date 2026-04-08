using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class SceneLoadManager : Singleton<SceneLoadManager>
    {
        [SerializeField]
        private string firstSceneToLoad = "MainMenu";

        private void Awake()
        {
            DontDestroyOnLoad(this);

            if (firstSceneToLoad.Length > 0)
                LoadRegularScene(firstSceneToLoad, true);
        }

        public void SubscribeOnNetworkEvents()
        {
            //On host prepared scene to load
            NetworkManager.Singleton.SceneManager.OnSynchronize += (clientId) =>
            {
                //Works on client side only
                if (NetworkManager.Singleton.LocalClientId == clientId)
                    SceneManager.LoadScene("LoadingScene");

            };

            //On host loading scene
            NetworkManager.Singleton.SceneManager.OnLoad += (clientId, sceneName, mode, sceneLoadOperation) =>
            {
                StartCoroutine(ProcessNetworkSceneLoading(sceneLoadOperation));
            };
        }

        public void LoadNetworkScene(string sceneName)
        {
            //Switch to loading scene first
            SceneManager.LoadScene("LoadingScene");

            SubscribeOnNetworkEvents();
            NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Additive);
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        public void LoadRegularScene(string sceneName, bool useLoadScreen = true)
        {
            StartCoroutine(ProcessRegularSceneLoading(sceneName, useLoadScreen));
        }

        private IEnumerator ProcessNetworkSceneLoading(AsyncOperation asyncOperation)
        {
            yield return asyncOperation;

            SceneManager.UnloadSceneAsync("LoadingScene");
        }

        private IEnumerator ProcessRegularSceneLoading(string sceneToLoad, bool useLoadScene = true)
        {
            if (useLoadScene)
                SceneManager.LoadScene("LoadingScene");

            yield return SceneManager.LoadSceneAsync(sceneToLoad);
        }
    }
}