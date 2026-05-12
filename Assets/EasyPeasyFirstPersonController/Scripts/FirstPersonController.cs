namespace EasyPeasyFirstPersonController
{
    using UnityEngine;

    public partial class FirstPersonController : MonoBehaviour
    {
        [Header("Settings")]
        public float walkSpeed = 3f;
        public float sprintSpeed = 5f;
        public float crouchSpeed = 1.5f;
        public float jumpSpeed = 4f;
        public float gravity = 9.81f;
        public float slideDuration = 0.7f;
        public float slideSpeed = 6f;
        public float mouseSensitivity = 2f;
        public float strafeTiltAmount = 2f;

        [Header("Camera Settings")]
        public bool useThirdPerson = true;
        public float cameraDistance = 3f;
        public float cameraHeightOffset = 0.5f;

        [Header("References")]
        public Transform playerCamera;
        public Transform cameraParent;
        public Transform groundCheck;
        public LayerMask groundMask;

        [HideInInspector] public CharacterController characterController;
        [HideInInspector] public IInputManager input;
        [HideInInspector] public Vector3 moveDirection;
        [HideInInspector] public bool isGrounded;

        private PlayerBaseState currentState;
        private PlayerStateFactory states;
        private float xRotation = 0f;
        private float currentTilt;
        private float tiltVelocity;
        private float cameraYaw = 0f;

        public PlayerBaseState CurrentState { get => currentState; set => currentState = value; }

        [Header("Visual Settings")]
        public float normalFov = 60f;
        public float sprintFov = 75f;
        public float slideFovBoost = 5f;
        public float fovChangeSpeed = 8f;
        public float bobAmount = 0.001f;
        public float bobSpeed = 10f;
        public float recoilReturnSpeed = 5f;

        [HideInInspector] public Camera cam;
        [HideInInspector] public float targetFov;
        [HideInInspector] public float currentBobIntensity;
        [HideInInspector] public float currentBobSpeed;
        [HideInInspector] public float targetTilt;

        private float bobTimer;
        private float fovVelocity;
        private float originalCamY;

        [Header("Height Settings")]
        public float standingCameraHeight = 3f;
        public float crouchingCameraHeight = 1f;
        public float crouchingCharacterControllerHeight = 1f;
        [HideInInspector] public float standingCharacterControllerHeight = 3f;
        [HideInInspector] public Vector3 standingCharacterControllerCenter = new Vector3(0, 0.9f, 0);
        [HideInInspector] public float targetCameraY;

        [Header("Ledge Settings")]
        public LayerMask ledgeLayer;
        public float ledgeDetectionDistance = 1f;
        private float landingMomentum;

        [Header("Swimming Settings")]
        public float swimSpeed = 4f;
        public float swimSprintSpeed = 6f;
        public float waterDrag = 2f;
        public LayerMask waterMask;
        [HideInInspector] public bool isInWater;

        [Header("Visual Preferences")]
        public bool useFovKick = true;
        public bool useHeadBob = true;
        public bool useCameraTilt = true;
        public bool useClimbTilt = true;

        [Header("Debug")]
        public bool currentStateDebug = true;

        void OnGUI()
        {
            if (currentState != null && Application.isEditor && currentStateDebug)
                GUILayout.Label("Current State: " + currentState.GetType().Name);
        }

        private void Awake()
        {
            cam = playerCamera.GetComponent<Camera>();
            targetFov = normalFov;
            targetCameraY = standingCameraHeight;
            originalCamY = standingCameraHeight;

            if (useThirdPerson && cameraParent != null)
            {
                cameraYaw = 0f;
                cameraParent.localPosition = new Vector3(0f, originalCamY + cameraHeightOffset, 0f);
                cameraParent.localRotation = Quaternion.identity;
                playerCamera.localPosition = new Vector3(0f, 0f, 0f);
                playerCamera.localRotation = Quaternion.identity;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            characterController = GetComponent<CharacterController>();
            standingCharacterControllerHeight = characterController.height;
            standingCharacterControllerCenter = characterController.center;
            input = GetComponent<IInputManager>();
            states = new PlayerStateFactory(this);

            currentState = states.Grounded();
            currentState.EnterState();
        }

        private void Update()
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundMask, QueryTriggerInteraction.Ignore);

            currentState.UpdateState();
            HandleRotation();
            
            // Rotate player towards camera direction when moving in third-person
            if (useThirdPerson && characterController.velocity.magnitude > 0.1f && isGrounded)
            {
                Quaternion targetRotation = Quaternion.Euler(0f, cameraYaw, 0f);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
            }
            
            UpdateVisuals();
        }

        private void HandleRotation()
        {
            float mouseX = input.lookInput.x * mouseSensitivity;
            float mouseY = input.lookInput.y * mouseSensitivity;

            float strafeTilt = useCameraTilt ? (-input.moveInput.x * strafeTiltAmount) : 0;
            float combinedTargetTilt = (useCameraTilt ? targetTilt : 0) + strafeTilt;
            currentTilt = Mathf.SmoothDamp(currentTilt, combinedTargetTilt, ref tiltVelocity, 0.1f);

            if (useThirdPerson)
            {
                cameraYaw -= mouseX;
                cameraParent.localRotation = Quaternion.Euler(0f, cameraYaw, 0f);
                playerCamera.localRotation = Quaternion.Euler(0f, 180f, currentTilt);
            }
            else
            {
                transform.Rotate(Vector3.up * mouseX);

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);
                playerCamera.localRotation = Quaternion.Euler(xRotation, 0, currentTilt);
            }
        }

        public void UpdateVisuals()
        {
            if (!useFovKick)
            {
                targetFov = normalFov;
            }
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFov, ref fovVelocity, 1f / fovChangeSpeed);

            landingMomentum = Mathf.Lerp(landingMomentum, 0, Time.deltaTime * 10f);
            float newY = Mathf.Lerp(cameraParent.localPosition.y, targetCameraY + cameraHeightOffset, Time.deltaTime * 8f);

            if (useThirdPerson)
            {
                float bobOffset = 0f;
                if (useHeadBob && characterController.velocity.magnitude > 0.1f && isGrounded)
                {
                    bobTimer += Time.deltaTime * currentBobSpeed;
                    bobOffset = Mathf.Sin(bobTimer) * currentBobIntensity;
                }
                else
                {
                    bobTimer = 0;
                }

                // Position orbit
                float orbitX = Mathf.Sin(cameraYaw * Mathf.Deg2Rad) * cameraDistance;
                float orbitZ = -Mathf.Cos(cameraYaw * Mathf.Deg2Rad) * cameraDistance;
                cameraParent.localPosition = new Vector3(orbitX, newY + bobOffset, orbitZ);

                // Look at player center
                Vector3 lookTarget = transform.position + Vector3.up * (originalCamY + cameraHeightOffset * 0.5f);
                playerCamera.LookAt(lookTarget);
            }
            else
            {
                if (useHeadBob && characterController.velocity.magnitude > 0.1f && isGrounded)
                {
                    bobTimer += Time.deltaTime * currentBobSpeed;
                    float bobOffset = Mathf.Sin(bobTimer) * currentBobIntensity;
                    cameraParent.localPosition = new Vector3(0f, newY + bobOffset, 0f);
                }
                else
                {
                    bobTimer = 0;
                    cameraParent.localPosition = new Vector3(0f, newY, 0f);
                }
            }
        }
        public bool HasCeiling()
        {
            float radius = characterController.radius * 0.9f;
            Vector3 origin = transform.position + Vector3.up * (characterController.height - radius);
            float checkDistance = standingCharacterControllerHeight - characterController.height + 0.1f;

            return Physics.SphereCast(origin, radius, Vector3.up, out _, checkDistance, groundMask, QueryTriggerInteraction.Ignore);
        }
        public bool CheckLedge(out Vector3 climbPosition)
        {
            climbPosition = Vector3.zero;
            RaycastHit wallHit;
            Vector3 wallOrigin = transform.position + Vector3.up * 1.5f;

            if (Physics.Raycast(wallOrigin, transform.forward, out wallHit, ledgeDetectionDistance, ledgeLayer, QueryTriggerInteraction.Ignore))
            {
                Vector3 ledgeOrigin = wallOrigin + Vector3.up * 0.6f + transform.forward * 0.2f;
                RaycastHit ledgeHit;

                if (!Physics.Raycast(ledgeOrigin, transform.forward, 0.5f, groundMask))
                {
                    if (Physics.Raycast(ledgeOrigin + transform.forward * 0.4f, Vector3.down, out ledgeHit, 1f, groundMask))
                    {
                        climbPosition = ledgeHit.point + Vector3.up * 1f;
                        return true;
                    }
                }
            }
            return false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & waterMask) != 0)
            {
                isInWater = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (((1 << other.gameObject.layer) & waterMask) != 0)
            {
                isInWater = false;
            }
        }

    }
}