using UnityEngine;

namespace PaintBlend.Gameplay
{
    public sealed class ThirdPersonCameraRig : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Camera controlledCamera;
        [SerializeField, Min(1f)] private float distance = 5.2f;
        [SerializeField] private float height = 1.35f;
        [SerializeField] private float lookSensitivity = 150f;
        [SerializeField] private float minPitch = -25f;
        [SerializeField] private float maxPitch = 70f;
        [SerializeField] private LayerMask cameraCollisionLayers = ~0;

        private float yaw;
        private float pitch = 12f;

        private void Awake()
        {
            if (controlledCamera == null) controlledCamera = Camera.main;
            if (target != null) yaw = target.eulerAngles.y;
        }

        public void SetTarget(Transform value)
        {
            target = value;
            if (target != null) yaw = target.eulerAngles.y;
        }

        public void AddLookInput(Vector2 delta)
        {
            yaw += delta.x * lookSensitivity * Time.deltaTime;
            pitch = Mathf.Clamp(pitch - delta.y * lookSensitivity * Time.deltaTime, minPitch, maxPitch);
        }

        private void LateUpdate()
        {
            if (target == null || controlledCamera == null) return;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + Vector3.up * height;
            Vector3 desiredPosition = pivot - rotation * Vector3.forward * distance;

            RaycastHit hit;
            if (Physics.SphereCast(pivot, 0.22f, desiredPosition - pivot, out hit,
                distance, cameraCollisionLayers, QueryTriggerInteraction.Ignore))
                desiredPosition = pivot - rotation * Vector3.forward * Mathf.Max(0.75f, hit.distance - 0.15f);

            controlledCamera.transform.SetPositionAndRotation(desiredPosition, rotation);
        }
    }
}
