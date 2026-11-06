using UnityEngine;

namespace PaintBlend.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerScaleContract : MonoBehaviour
    {
        public const float NominalHeightMetres = 1.75f;
        public const float DefaultRadiusMetres = 0.32f;
        [SerializeField] private bool configureCapsuleOnAwake = true;
        private CharacterController characterController;

        public CharacterController Controller => characterController;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (configureCapsuleOnAwake)
            {
                characterController.height = NominalHeightMetres;
                characterController.radius = DefaultRadiusMetres;
                characterController.center = new Vector3(0f, NominalHeightMetres * 0.5f, 0f);
            }

            if ((transform.lossyScale - Vector3.one).sqrMagnitude > 0.0001f)
                Debug.LogWarning("Paint & Blend competitive player roots must remain at unit scale.", this);
        }

        public bool HasValidScale()
        {
            return (transform.lossyScale - Vector3.one).sqrMagnitude <= 0.0001f &&
                   characterController != null &&
                   Mathf.Abs(characterController.height - NominalHeightMetres) <= 0.02f;
        }
    }
}
