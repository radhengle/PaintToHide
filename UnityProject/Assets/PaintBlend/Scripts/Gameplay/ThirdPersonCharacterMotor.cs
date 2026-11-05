using UnityEngine;

namespace PaintBlend.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ThirdPersonCharacterMotor : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Animator animator;
        [SerializeField, Min(0.1f)] private float moveSpeed = 4.3f;
        [SerializeField, Min(0.1f)] private float rotationSharpness = 12f;
        [SerializeField] private float gravity = -22f;
        [SerializeField] private float jumpHeight = 1.0f;

        private CharacterController controller;
        private Vector2 moveInput;
        private float verticalVelocity;
        private bool jumpRequested;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        }

        public void SetMoveInput(Vector2 input) { moveInput = Vector2.ClampMagnitude(input, 1f); }
        public void RequestJump() { jumpRequested = true; }

        private void Update()
        {
            Vector3 forward = cameraTransform != null
                ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized
                : Vector3.forward;
            Vector3 right = cameraTransform != null
                ? Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized
                : Vector3.right;

            Vector3 desired = (forward * moveInput.y + right * moveInput.x);
            if (desired.sqrMagnitude > 1f) desired.Normalize();

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            if (jumpRequested && controller.isGrounded)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpRequested = false;
            verticalVelocity += gravity * Time.deltaTime;

            if (desired.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(desired, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                    rotationSharpness * Time.deltaTime);
            }

            Vector3 velocity = desired * moveSpeed + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
            if (animator != null)
            {
                animator.SetFloat("MoveSpeed", desired.magnitude);
                animator.SetBool("Grounded", controller.isGrounded);
            }
        }
    }
}
