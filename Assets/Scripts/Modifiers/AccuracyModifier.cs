using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    [CreateAssetMenu(fileName = "AccuracyModifier", menuName = "Modifier/Accuracy Modifier")]
    public class AccuracyModifier : ModifierScriptableObject
    {
        [JsonProperty("am")]
        public float accuracyMultiplier;

        [JsonProperty("d")]
        public float duration;

        public double startTime { get; protected set; }
        public double endTime { get; protected set; }

        public override void Activate(CommandsControlSystem commandTarget)
        {
            base.Activate(commandTarget);

            startTime = NetworkManager.Singleton.ServerTime.Time;
            endTime = startTime + duration;
        }

        public override void FixedUpdate()
        {
            if (NetworkManager.Singleton.ServerTime.Time > endTime)
                Complete();
        }

        public override bool TryMergeModifiers(ModifierScriptableObject inputModifier)
        {
            startTime = (inputModifier as AccuracyModifier).startTime;
            endTime = startTime + duration;

            return true;
        }

        public override float GetCurrentProgress()
        {
            double timePassed = NetworkManager.Singleton.ServerTime.Time - startTime;
            return (float)(timePassed / duration);
        }
    }
}
