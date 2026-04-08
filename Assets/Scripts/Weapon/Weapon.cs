using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using Random = UnityEngine.Random;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class Weapon : NetworkBehaviour
    {
        public WeaponType weaponType;

        [Space()]
        public WeaponModelTransformKeeper weaponModelTransformKeeper;

        [Space()]
        public Transform targetingTransform;
        public List<Transform> gunDirectionTransforms = new List<Transform>();

        public WeaponGrip weaponGrip => weaponConfig.weaponGrip;

        public Action OnFire;

        private WeaponConfig weaponConfig;
        private float lastFireTime = 0;

        private CharacterIdentityControl identityControl;
        private CommandsControlSystem commandsControlSystem;

        private void Awake()
        {
            weaponConfig = SettingsManager.Instance.weapon.GetWeaponConfig(weaponType);
            identityControl = transform.root.GetComponent<CharacterIdentityControl>();
            commandsControlSystem = transform.root.GetComponent<CommandsControlSystem>();
        }

        public void Fire()
        {
            float currentFireRate = weaponConfig.fireRate * CalculateFireRateMultiplier();

            //Wait for next fire
            if (lastFireTime + currentFireRate < Time.time)
            {
                ProjectileParameters projectileParameters = new ProjectileParameters();
                projectileParameters.senderID = NetworkObjectId;
                projectileParameters.speed = weaponConfig.bulletSpeed;
                projectileParameters.startTime = Math.Round(NetworkManager.Singleton.ServerTime.Time, 4);
                projectileParameters.startPosition = transform.position;

                //Set bullet direction according to accuracy settings and active modifiers
                float accuracy = weaponConfig.accuracyRange;

                //Create and send bullet for every "gun" in weapon (could be few "guns" in a shotgun)
                for (int i = 0; i < gunDirectionTransforms.Count; i++)
                {
                    float range = (1f - accuracy) * CalculateAccuracyMultiplier();
                    Vector3 accuracyOffset = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
                    projectileParameters.directions.Add((gunDirectionTransforms[i].forward + accuracyOffset).normalized);
                }

                //Bullet owner. defaultOwnerID if it's bot. Requires to register scores in leaderboard.
                ulong ownerID = identityControl.isPlayer ? OwnerClientId : SettingsManager.Instance.ai.defaultOwnerID;

                //Add instant damage command
                DamageCommand damageCommand = ScriptableObject.CreateInstance<DamageCommand>();
                damageCommand.ownerID = ownerID;
                damageCommand.damage = (int)weaponConfig.damage;
                projectileParameters.commands.Add(damageCommand);

                //Broadcast this message to clients
                string serializedData = JsonConvert.SerializeObject(projectileParameters);
                projectileParameters.ClearCommands();

                SendFireRPC(serializedData);

                lastFireTime = Time.time;
            }
        }

        [Rpc(SendTo.Everyone)]
        private void SendFireRPC(string serializedAmmoParameters)
        {
            //Play fire animation
            OnFire?.Invoke();

            Transform firePoint = weaponModelTransformKeeper.firePointTransform;
           
            //Process ammo spawn
            for (int i = 0; i < gunDirectionTransforms.Count; i++)
            {
                ProjectileParameters bulletParameters = JsonConvert.DeserializeObject<ProjectileParameters>(serializedAmmoParameters);

                Transform instantiatedBullet = Instantiate(weaponConfig.bulletPrefab, firePoint.position, gunDirectionTransforms[i].rotation);
                instantiatedBullet.GetComponent<Bullet>().Initialize(bulletParameters, i, firePoint, weaponConfig.muzzleFlashPrefab);
            }
        }

        public void ShowWeapon()
        {
            weaponModelTransformKeeper.weaponModel.gameObject.SetActive(true);
        }

        public void HideWeapon()
        {
            weaponModelTransformKeeper.weaponModel.gameObject.SetActive(false);
        }

        private float CalculateAccuracyMultiplier()
        {
            float resultMultiplier = 1;

            //Handle all the modifiers related to accuracy
            for (int i = 0; i < commandsControlSystem.activeCommands.Count; i++)
            {
                //Check modifier by type
                if (commandsControlSystem.activeCommands[i] is AccuracyModifier accuracyModifier)
                    resultMultiplier *= accuracyModifier.accuracyMultiplier;
            }

            return 1f / resultMultiplier;
        }

        private float CalculateFireRateMultiplier()
        {
            float resultMultiplier = 1;

            //Handle all the modifiers related to fire rate
            for (int i = 0; i < commandsControlSystem.activeCommands.Count; i++)
            {
                //Check modifier by type
                if (commandsControlSystem.activeCommands[i] is FireRateModifier fireRateModifier)
                    resultMultiplier *= fireRateModifier.fireRateMultiplier;
            }

            return 1f / resultMultiplier;
        }
    }
}
