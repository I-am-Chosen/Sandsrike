using Newtonsoft.Json;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    [CreateAssetMenu(fileName = "HealCommand", menuName = "Modifier/Heal Command", order = 1)]
    public class HealCommand : LogicScriptableObject
    {
        [JsonProperty("h")]
        public int health;

        public override void Activate(CommandsControlSystem commandTarget)
        {
            commandTarget.GetComponent<HealthController>().ReceiveHealth(health);

            Complete();
        }
    }
}
