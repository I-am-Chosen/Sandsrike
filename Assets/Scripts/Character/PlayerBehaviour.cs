using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class PlayerBehaviour : NetworkBehaviour
    {
        private float movementSpeed = 5;
        private float smoothMovementTime = 0.1f;

        private WeaponControlSystem weaponControlSystem;
        private ShooterInputControls inputActions;
        private HealthController healthController;
        private CommandsControlSystem commandsControlSystem;
        private RigidbodyCharacterController rigidbodyCharacterController;
        private CharacterIdentityControl identityControl;

        private Camera mainCamera;
        private Plane plane;

        private bool isMobile = false;
        private Vector3 movementVelocity = Vector3.zero;
        private Vector3 currentMovementInput;

        private void Awake()
        {
            //Register spawned player object (bots need it to find player)
            GameManager.Instance.userControl.AddPlayerObject(NetworkObject);
        }

        private void Start()
        {
            //Basic components
            weaponControlSystem = GetComponent<WeaponControlSystem>();
            healthController = GetComponent<HealthController>();
            commandsControlSystem = GetComponent<CommandsControlSystem>();
            rigidbodyCharacterController = GetComponent<RigidbodyCharacterController>();
            identityControl = GetComponent<CharacterIdentityControl>();

            //Camera and aiming
            plane = new Plane(Vector3.up, weaponControlSystem.lineOfSightTransform.localPosition);
            mainCamera = Camera.main;
            if (IsOwner && mainCamera != null)
                mainCamera.GetComponent<GameCameraController>()?.ActivateCameraMovement();

            //Settings
            int modelIndex = identityControl.spawnParameters.modelIndex;
            PlayerConfig config = SettingsManager.Instance.player.configs[modelIndex];

            //Health
            healthController.Initialize(config.health);
            healthController.OnDeath += () =>
            {
                GetComponent<CharacterDeathController>().RunDestroyScenario(true);

                if (IsOwner == true) GameManager.Instance.UI.ShowEndOfGamePopup();
            };

            //Inputs
            inputActions = new ShooterInputControls();
            inputActions.Player.Look.Enable();
            inputActions.Player.Move.Enable();
            inputActions.Player.Fire.Enable();

            movementSpeed = config.movementSpeed;

            isMobile = Application.isMobilePlatform;

            gameObject.name = "Player: " + identityControl.spawnParameters.name;
        }

        void FixedUpdate()
        {
            if (IsOwner == false) return;

            //Stop any movement when game ends
            if (GameManager.Instance.gameState == GameState.GameIsOver) rigidbodyCharacterController.Stop();

            //Stop any movement when player is dead
            if (healthController.isAlive == false) rigidbodyCharacterController.Stop();

            //Wait for game to start
            if (GameManager.Instance.gameState != GameState.ActiveGame) return;

            if (isMobile)
                HandleMobileInput();
            else
                HandleKeyboardInput();
        }

        private void HandleKeyboardInput()
        {
            Vector2 mouseInput = inputActions.Player.Look.ReadValue<Vector2>();
            Ray ray = mainCamera.ScreenPointToRay(mouseInput);

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector3 lookDirection = hitPoint - transform.position;
                lookDirection.y = 0;

                //Point line of sight in direction of cursor
                weaponControlSystem.lineOfSightTransform.localRotation = Quaternion.LookRotation(lookDirection);

                //Draw line of sight direction
                if (Application.isEditor)
                    Debug.DrawRay(weaponControlSystem.lineOfSightTransform.position, weaponControlSystem.lineOfSightTransform.forward, Color.red);
            }

            //Keyboard inputs
            Vector2 positionInput = inputActions.Player.Move.ReadValue<Vector2>().normalized;
            currentMovementInput = Vector3.SmoothDamp(currentMovementInput, positionInput, ref movementVelocity, smoothMovementTime);

            //Update movement speed according to currently active modifiers
            float currentSpeed = CalculateSpeedMultiplier() * movementSpeed;

            //Move (using physics)
            rigidbodyCharacterController.Move(new Vector3(currentMovementInput.x, 0, currentMovementInput.y), currentSpeed);

            //Fire weapon
            if (inputActions.Player.Fire.inProgress)
                weaponControlSystem.Fire();
        }

        private void HandleMobileInput()
        {
            if (inputActions.Player.Look.inProgress)
            {
                //Screen joystick inputs
                Vector2 lookJoystickInput = inputActions.Player.Look.ReadValue<Vector2>();
                weaponControlSystem.lineOfSightTransform.localRotation = Quaternion.LookRotation(new Vector3(lookJoystickInput.x, 0, lookJoystickInput.y));

                //Fire weapon
                weaponControlSystem.Fire();
            }

            //Screen joystick inputs
            Vector2 moveJoystickInput = inputActions.Player.Move.ReadValue<Vector2>().normalized;
            currentMovementInput = Vector3.SmoothDamp(currentMovementInput, moveJoystickInput, ref movementVelocity, smoothMovementTime);

            //Update movement speed according to currently active modifiers
            float currentSpeed = CalculateSpeedMultiplier() * movementSpeed;

            //Move (using physics)
            rigidbodyCharacterController.Move(new Vector3(currentMovementInput.x, 0, currentMovementInput.y), currentSpeed);
        }

        private float CalculateSpeedMultiplier()
        {
            float resultMultiplier = 1;

            //Handle all the modifiers related to speed
            for (int i = 0; i < commandsControlSystem.activeCommands.Count; i++)
            {
                //Check modifier by type
                if (commandsControlSystem.activeCommands[i] is SpeedModifier speedModifier)
                    resultMultiplier *= speedModifier.speedMultiplier;
            }

            return resultMultiplier;
        }

        public override void OnDestroy()
        {
            inputActions?.Disable();
        }
    }
}
