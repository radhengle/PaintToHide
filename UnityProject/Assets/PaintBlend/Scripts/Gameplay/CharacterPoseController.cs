using UnityEngine;

namespace PaintBlend.Gameplay
{
    public sealed class CharacterPoseController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private PoseType currentPose = PoseType.Stand;

        public PoseType CurrentPose => currentPose;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            ApplyPose(currentPose);
        }

        public bool TrySetPose(PoseType pose, HidingSlot selectedSlot)
        {
            if (selectedSlot == null || !selectedSlot.Supports(pose)) return false;
            ApplyPose(pose);
            return true;
        }

        public void ApplyPose(PoseType pose)
        {
            currentPose = pose;
            if (animator != null) animator.SetInteger("PoseType", (int)pose);
        }
    }
}
