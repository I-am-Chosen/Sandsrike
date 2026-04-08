using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class CharacterNetworkTransform : NetworkBehaviour
    {
        //Masks
        //Full synchronization will be enabled, in case if whole block is marked as true. See details below.

        //Rotation block
        [Space()]
        public bool xRotation = true;
        public bool yRotation = true;
        public bool zRotation = true;

        [Space()]
        public bool velocityMagnitude = false;

        [Space()]
        //Regular position smoothness
        public int positionSmoothingFrames = 2;

        //Prediction force, based on previous and current positions
        public float positionExtrapolationFactor = 5f;

        [Space()]
        //Rotation smoothness (no prediction here)
        public float rotationInterpolationFactor = 0.5f;

        private NetworkVariable<float> xRotationValue = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> yRotationValue = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> zRotationValue = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

        //Variables for full synchronization
        private NetworkVariable<Vector3> fullPosition = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<Quaternion> fullRotation = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);

        private NetworkVariable<float> synchronizedVelocity = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

        private float teleportDistance = 2;
        private Vector3 previousPosition;
        private Vector3 positionDampVelocity;
        private Vector3 calculatedDirection;

        public override void OnNetworkSpawn()
        {
            //Synchronize position on spawn
            if (IsOwner)
                fullPosition.Value = transform.localPosition;

            //Local position automatically transforms to global, it there is no parent object. Same with local rotation.
            transform.localPosition = fullPosition.Value;
            previousPosition = transform.localPosition;
        }

        private void FixedUpdate()
        {
            if (IsOwner) //Write 
            {
                fullPosition.Value = transform.localPosition;

                if (xRotation && yRotation && zRotation)
                {
                    fullRotation.Value = transform.localRotation;
                }
                else
                {
                    Vector3 rotation = transform.localRotation.eulerAngles;
                    if (xRotation) xRotationValue.Value = rotation.x;
                    if (yRotation) yRotationValue.Value = rotation.y;
                    if (zRotation) zRotationValue.Value = rotation.z;
                }

                if (velocityMagnitude)
                {
                    float magnitude = (transform.localPosition - previousPosition).magnitude * (1f / Time.fixedDeltaTime);
                    synchronizedVelocity.Value = (float)System.Math.Round(magnitude, 2);
                }

                previousPosition = transform.localPosition;
            }
            else //Read
            {
                Vector3 position = fullPosition.Value;

                if (position != previousPosition)
                {
                    calculatedDirection = (position - previousPosition);
                    previousPosition = position;
                }

                Quaternion rotation = transform.localRotation;
                if (xRotation && yRotation && zRotation)
                {
                    rotation = fullRotation.Value;
                }
                else
                {
                    Vector3 eulerAngles = Vector3.zero;
                    if (xRotation) eulerAngles.x = xRotationValue.Value;
                    if (yRotation) eulerAngles.y = yRotationValue.Value;
                    if (zRotation) eulerAngles.z = zRotationValue.Value;

                    rotation = Quaternion.Euler(eulerAngles);
                }

                //Teleport if we too far from required position
                if ((transform.localPosition - position).magnitude > teleportDistance)
                    transform.localPosition = position;

                Vector3 extrapolationOffset = calculatedDirection.normalized * synchronizedVelocity.Value * positionExtrapolationFactor * Time.fixedDeltaTime;

                //Apply
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, position + extrapolationOffset, ref positionDampVelocity, positionSmoothingFrames * Time.fixedDeltaTime);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, rotation, rotationInterpolationFactor);
            }
        }
    }
}
