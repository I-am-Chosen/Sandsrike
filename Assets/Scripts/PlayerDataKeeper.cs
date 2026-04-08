using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public static class PlayerDataKeeper
    {
        //Unique authentication name
        public static string authProfileName { get; set; }

        private static string platformAlias = "Netcode ";

        public static string name
        {
            get
            {
                if (PlayerPrefs.GetString(platformAlias + authProfileName + "playerName", string.Empty) == string.Empty)
                    PlayerPrefs.SetString(platformAlias + authProfileName + "playerName", "Player" + UnityEngine.Random.Range(1, 9999));

                return PlayerPrefs.GetString(platformAlias + authProfileName + "playerName");
            }
            set => PlayerPrefs.SetString(platformAlias + authProfileName + "playerName", value);
        }

        public static string selectedRegion
        {
            get => PlayerPrefs.GetString(platformAlias + authProfileName + "selectedRegion", string.Empty);
            set => PlayerPrefs.SetString(platformAlias + authProfileName + "selectedRegion", value);
        }

        public static List<string> availableRegions
        {
            get => JsonConvert.DeserializeObject<List<string>>(PlayerPrefs.GetString(platformAlias + authProfileName + "availableRegions", "[]"));
            set => PlayerPrefs.SetString(platformAlias + authProfileName + "availableRegions", JsonConvert.SerializeObject(value));
        }
        public static DateTime lastRegionsUpdateTime
        {
            get => Convert.ToDateTime(PlayerPrefs.GetString(platformAlias + authProfileName + "lastRegionsUpdateTime", DateTime.MinValue.ToString()));
            set => PlayerPrefs.SetString(platformAlias + authProfileName + "lastRegionsUpdateTime", value.ToString());
        }

        public static int selectedColor
        {
            get => PlayerPrefs.GetInt(platformAlias + authProfileName + "selectedColor", -1);
            set => PlayerPrefs.SetInt(platformAlias + authProfileName + "selectedColor", value);
        }

        public static int selectedPrefab
        {
            get => PlayerPrefs.GetInt(platformAlias + authProfileName + "selectedPrefab", 0);
            set => PlayerPrefs.SetInt(platformAlias + authProfileName + "selectedPrefab", value);
        }

        public static string selectedScene
        {
            get => PlayerPrefs.GetString(platformAlias + authProfileName + "selectedScene", "none");
            set => PlayerPrefs.SetString(platformAlias + authProfileName + "selectedScene", value);
        }
    }
}
