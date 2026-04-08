using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class SettingsManager : Singleton<SettingsManager>
    {
        public CommonSettings common;
        public LobbySettings lobby;
        public PlayerSettings player;
        public AISettings ai;
        public WeaponSettings weapon;
        public GameplaySettings gameplay;
        new public CameraSettings camera;

        void Awake()
        {
            JsonConvert.DefaultSettings = GenerateJsonSerializerSettings;

            ScriptableObjectJson.ClearRegister();

            ScriptableObjectJson.RegisterTypeAlias("WSC", typeof(WeaponSwitchCommand));
            ScriptableObjectJson.RegisterTypeAlias("HC", typeof(HealCommand));
            ScriptableObjectJson.RegisterTypeAlias("DC", typeof(DamageCommand));

            ScriptableObjectJson.RegisterTypeAlias("AM", typeof(AccuracyModifier));
            ScriptableObjectJson.RegisterTypeAlias("FRM", typeof(FireRateModifier));
            ScriptableObjectJson.RegisterTypeAlias("SM", typeof(SpeedModifier));

            DontDestroyOnLoad(this);
        }

        private JsonSerializerSettings GenerateJsonSerializerSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();

            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;

            //Vector2 serialization
            settings.Converters.Add(new JsonVector2Converter());

            //Vector3 serialization
            settings.Converters.Add(new JsonVector3Converter());

            //Color serialization
            settings.Converters.Add(new JsonColorConverter());

            //ScriptableObjectJson serialization
            settings.Converters.Add(new JsonScriptableObjectCreator());

            //Add new converters here

            return settings;
        }
    }
}
