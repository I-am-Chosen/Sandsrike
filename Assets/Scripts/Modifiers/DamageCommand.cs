using Newtonsoft.Json;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    [CreateAssetMenu(fileName = "DamageCommand", menuName = "Modifier/Damage Command", order = 0)]
    public class DamageCommand : LogicScriptableObject
    {
        [JsonProperty("d")]
        public int damage;

        [HideInInspector]
        [JsonProperty("oID")]
        public ulong ownerID;

        public override void Activate(CommandsControlSystem commandTarget)
        {
            commandTarget.GetComponent<HealthController>().ReceiveDamage(damage, ownerID);

            Complete();
        }
    }
}
