using Newtonsoft.Json;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    [CreateAssetMenu(fileName = "WeaponSwitchCommand", menuName = "Modifier/Weapon Switch Command", order = 2)]
    public class WeaponSwitchCommand : LogicScriptableObject
    {
        [JsonProperty("w")]
        public WeaponType weapon;

        public override void Activate(CommandsControlSystem commandTarget)
        {
            if (commandTarget.TryGetComponent(out WeaponControlSystem weaponControlSystem))
            {
                if (weaponControlSystem.IsOwner == true)
                    weaponControlSystem.ActivateWeaponRpc(weapon);
            }

            Complete();
        }
    }
}
