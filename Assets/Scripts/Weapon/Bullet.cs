using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class Bullet : MonoBehaviour
    {
        public float bulletRadius = 0.25f;
        public float bulletLifetime = 1;
        public float trackingFrames = 3f;
        public float lagCompensationFactor = 0.1f;

        private ProjectileParameters projectileParameters;
        private GameObject bulletTrail;

        private Vector3 previousPosition;
        private float bulletSpawnTime = 0;
        private Vector3 direction;

        private bool isWaitingForDestroy = false;

        public void Initialize(ProjectileParameters projectileParameters, int gunIndex, Transform bulletSpawnPointTransform, Transform muzzleFlashPrefab)
        {
            transform.position = bulletSpawnPointTransform.position;
            previousPosition = projectileParameters.startPosition;
            direction = projectileParameters.directions[gunIndex];

            this.projectileParameters = projectileParameters;

            //Create and destroy muzzle flash (with delay)
            Transform muzzleFlash = Instantiate(muzzleFlashPrefab, bulletSpawnPointTransform.position, Quaternion.LookRotation(direction));
            muzzleFlash.parent = bulletSpawnPointTransform;
            Destroy(muzzleFlash.gameObject, 0.1f);

            //Find and deactivate bullet trail (activates at the end of frame)
            bulletTrail = transform.GetChild(0).gameObject;
            bulletTrail.SetActive(false);

            bulletSpawnTime = Time.time;
        }

        void FixedUpdate()
        {
            if (NetworkManager.Singleton == null) return;

            double deltaTime = NetworkManager.Singleton.ServerTime.Time - projectileParameters.startTime;
            float distancePassed = projectileParameters.speed * (float)deltaTime;

            //Calculated bullet position ("server bullet")
            Vector3 serverBulletPosition = projectileParameters.startPosition + direction * distancePassed;

            Vector3 bulletLocalMovementStep = direction * projectileParameters.speed * Time.fixedDeltaTime;

            //Difference between bullet position on server and bullet position on client 
            Vector3 difference = serverBulletPosition - (transform.position + bulletLocalMovementStep);

            //Check if we're not in front of bullet's calculated position
            if (Vector3.Dot(bulletLocalMovementStep, difference) < 0) return;

            Vector3 lagCompensation = difference * lagCompensationFactor;

            //Move closer to server bullet position
            transform.position += Vector3.ClampMagnitude(bulletLocalMovementStep + lagCompensation, projectileParameters.speed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.LookRotation(direction);

            Vector3 movementDelta = previousPosition - serverBulletPosition;

            float trackingDistance = Mathf.Max(movementDelta.magnitude, projectileParameters.speed * trackingFrames * Time.fixedDeltaTime);
            float raycastDistance = Mathf.Min(trackingDistance, distancePassed);

            Vector3 startPoint = serverBulletPosition - direction * raycastDistance;

            //Check bullet hit
            if (isWaitingForDestroy == false && Physics.SphereCast(startPoint, bulletRadius, direction, out RaycastHit hit, raycastDistance))
            {
                bool allowToHandleHit = true;

                //Check if current player didn't hit itself. Doesn't suppose to, but it could be with high ping.
                NetworkObject sender = GameManager.Instance.userControl.FindCharacterByID(projectileParameters.senderID);
                if (sender != null && sender.transform == hit.transform) allowToHandleHit = false;

                if (allowToHandleHit == true && hit.transform.TryGetComponent(out CommandsControlSystem commandsControlSystem))
                {
                    //Receive hit modifiers (broadcast message)
                    if (NetworkManager.Singleton.IsServer)
                    {
                        string serializedCommands = JsonConvert.SerializeObject(projectileParameters.commands);
                        commandsControlSystem.ReceiveCommandsRpc(serializedCommands);
                    }   
                }

                //Destroy bullet (cause it hits something)
                StartCoroutine(RunBulletDestroy(hit.point));
                isWaitingForDestroy = true;
            }

            //Activates bullet trail at the end of frame (it deactivates on spawn)
            //Here could be placed any other visual activation logic
            bulletTrail.gameObject.SetActive(true);

            previousPosition = serverBulletPosition;

            //Destroy bullets by timeout
            if (Time.time > bulletSpawnTime + bulletLifetime)
            {
                Destroy(gameObject);
                projectileParameters.ClearCommands();
            }
        }

        private IEnumerator RunBulletDestroy(Vector3 hitPoint)
        {
            //For better look we can wait for a few frames, to make ammo reach it's hit point.
            //It keeps flying while we wait.
            float delay = (transform.position - hitPoint).magnitude / projectileParameters.speed;
            yield return new WaitForSeconds(delay - Time.fixedDeltaTime);

            //Destroy bullet
            bulletTrail.SetActive(false);   
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            projectileParameters.ClearCommands();
        }
    }
}
