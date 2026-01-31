using UnityEngine;

namespace Project.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraPivot;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.6f;
        [SerializeField] private float crouchMultiplier = 0.5f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float riseGravityMultiplier = 1.1f;
        [SerializeField] private float fallGravityMultiplier = 1.6f;
        [SerializeField] private float crouchHeight = 1.0f;

        [Header("Stamina")]
        [SerializeField] private float maxStamina = 5f;
        [SerializeField] private float staminaDrainRate = 1.2f;
        [SerializeField] private float staminaRegenRate = 1.5f;
        [SerializeField] private float staminaRegenDelay = 0.5f;
        [SerializeField] private float minStaminaToSprint = 0.1f;

        [Header("Look")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float pitchMin = -80f;
        [SerializeField] private float pitchMax = 80f;

        private CharacterController controller;
        private float verticalVelocity;
        private float pitch;
        private float standHeight;
        private Vector3 standCenter;
        private Vector3 crouchCenter;
        private float currentStamina;
        private float staminaRegenCooldown;

        public float CurrentStamina => currentStamina;
        public float StaminaNormalized => maxStamina > 0f ? currentStamina / maxStamina : 0f;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            standHeight = controller.height;
            standCenter = controller.center;
            if (crouchHeight <= 0f)
            {
                crouchHeight = standHeight * 0.6f;
            }
            crouchHeight = Mathf.Clamp(crouchHeight, 0.2f, standHeight);
            float heightDelta = standHeight - crouchHeight;
            crouchCenter = new Vector3(standCenter.x, standCenter.y - heightDelta * 0.5f, standCenter.z);

            if (cameraPivot == null && Camera.main != null)
            {
                cameraPivot = Camera.main.transform;
            }

            maxStamina = Mathf.Max(0.1f, maxStamina);
            currentStamina = maxStamina;
            staminaRegenCooldown = 0f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleLook();
            HandleMove();
        }

        private void HandleLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            pitch = Mathf.Clamp(pitch - mouseY, pitchMin, pitchMax);
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        private void HandleMove()
        {
            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            bool wantsCrouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
            float targetHeight = wantsCrouch ? crouchHeight : standHeight;
            Vector3 targetCenter = wantsCrouch ? crouchCenter : standCenter;
            controller.height = Mathf.MoveTowards(controller.height, targetHeight, 6f * Time.deltaTime);
            controller.center = Vector3.Lerp(controller.center, targetCenter, 12f * Time.deltaTime);

            float speed = moveSpeed;
            if (wantsCrouch)
            {
                speed *= crouchMultiplier;
            }
            bool hasMoveInput = input.sqrMagnitude > 0.01f;
            bool wantsSprint = Input.GetKey(KeyCode.LeftShift) && hasMoveInput;
            bool canSprint = currentStamina > minStaminaToSprint;
            if (!wantsCrouch && wantsSprint && canSprint)
            {
                speed *= sprintMultiplier;
                currentStamina = Mathf.Max(0f, currentStamina - staminaDrainRate * Time.deltaTime);
                staminaRegenCooldown = staminaRegenDelay;
            }
            else
            {
                if (staminaRegenCooldown > 0f)
                {
                    staminaRegenCooldown -= Time.deltaTime;
                }
                else
                {
                    currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
                }
            }

            Vector3 move = transform.TransformDirection(input) * speed;

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (controller.isGrounded && Input.GetButtonDown("Jump") && !wantsCrouch)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            float gravityScale = verticalVelocity < 0f ? fallGravityMultiplier : riseGravityMultiplier;
            verticalVelocity += gravity * gravityScale * Time.deltaTime;
            move.y = verticalVelocity;

            controller.Move(move * Time.deltaTime);
        }
    }
}
