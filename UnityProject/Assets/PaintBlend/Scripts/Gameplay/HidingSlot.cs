using UnityEngine;

namespace PaintBlend.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class HidingSlot : MonoBehaviour
    {
        [SerializeField] private string slotId;
        [SerializeField] private Transform poseAnchor;
        [SerializeField] private PoseMask supportedPoses = PoseMask.Stand | PoseMask.Crouch;
        [SerializeField] private string referenceObjectId;
        [SerializeField, Range(0f, 1f)] private float baseConcealment = 0.5f;

        public string SlotId => slotId;
        public Transform PoseAnchor => poseAnchor != null ? poseAnchor : transform;
        public PoseMask SupportedPoses => supportedPoses;
        public string ReferenceObjectId => referenceObjectId;
        public float BaseConcealment => baseConcealment;

        public bool Supports(PoseType pose)
        {
            return (supportedPoses & PoseMaskUtility.ToMask(pose)) != 0;
        }

        public void Configure(string id, string referenceId, PoseMask poses, float concealment)
        {
            slotId = id;
            referenceObjectId = referenceId;
            supportedPoses = poses;
            baseConcealment = Mathf.Clamp01(concealment);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(PoseAnchor.position, 0.18f);
        }
#endif
    }
}
