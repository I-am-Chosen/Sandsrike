using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class HealthController : NetworkBehaviour
    {
        public float currentHealth { get; private set; }
        public float maxHealth { get; private set; }

        public bool isAlive => currentHealth > 0;

        public Action OnDeath;

        private CharacterIdentityControl identityControl;
        private bool isDeathEventProcessed = false;

        public void Awake()
        {
            identityControl = GetComponent<CharacterIdentityControl>();
        }

        public void Initialize(float maxHealth)
        {
            currentHealth = maxHealth;
            this.maxHealth = maxHealth;
        }

        private void FixedUpdate()
        {
            if (isAlive == false) return;

            //Update health
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        public void ReceiveDamage(int damage, ulong ownerID)
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                if (identityControl.IsOwner == true)
                {
                    //Broadcast death event
                    if (isDeathEventProcessed == false)
                        ConfirmCharacterDeathRpc(ownerID);

                    isDeathEventProcessed = true;
                }
            }
        }

        public void ReceiveHealth(int hp)
        {
            currentHealth += hp;
        }

        [Rpc(SendTo.Everyone)]
        private void ConfirmCharacterDeathRpc(ulong killerID)
        {
            currentHealth = 0;
            OnDeath?.Invoke();

            //Register bot death (from player)
            if (identityControl.isBot == true && killerID != SettingsManager.Instance.ai.defaultOwnerID)
                GameManager.Instance.RegisterCharacterDeath(killerID);
        }
    }
}
