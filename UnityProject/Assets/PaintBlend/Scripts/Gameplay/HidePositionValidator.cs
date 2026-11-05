using UnityEngine;
using UnityEngine.AI;

namespace PaintBlend.Gameplay
{
    public static class HidePositionValidator
    {
        private static readonly Collider[] OverlapBuffer = new Collider[64];

        /// <summary>
        /// Validates the full character capsule at the candidate pose. The slot
        /// anchor must be authored beside (not inside) its reference object.
        /// </summary>
        public static bool TryValidate(
            HidingSlot slot,
            PoseType pose,
            float radius,
            float standingHeight,
            LayerMask solidLayers,
            LayerMask groundLayers,
            Transform playerRoot,
            Vector3 currentPosition,
            out string reason)
        {
            reason = null;
            if (slot == null) { reason = "No hiding slot selected."; return false; }
            if (!slot.Supports(pose)) { reason = "This slot does not support the selected pose."; return false; }
            if (radius <= 0f || standingHeight < radius * 2f)
            {
                reason = "Character capsule dimensions are invalid.";
                return false;
            }

            Vector3 feet = slot.PoseAnchor.position;
            Vector3 bottomSphere;
            Vector3 topSphere;
            float capsuleRadius = radius;

            if (pose == PoseType.LieDown)
            {
                // A lying mannequin occupies a horizontal capsule rather than
                // a short vertical capsule that would invert its end points.
                Vector3 axis = slot.PoseAnchor.right * 0.55f;
                capsuleRadius = Mathf.Min(radius, 0.30f);
                bottomSphere = feet + Vector3.up * capsuleRadius - axis;
                topSphere = feet + Vector3.up * capsuleRadius + axis;
            }
            else
            {
                float poseHeight = Mathf.Max(GetPoseHeight(pose, standingHeight), radius * 2f);
                bottomSphere = feet + Vector3.up * radius;
                topSphere = feet + Vector3.up * (poseHeight - radius);
            }

            int count = Physics.OverlapCapsuleNonAlloc(
                bottomSphere, topSphere, capsuleRadius, OverlapBuffer,
                solidLayers, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Collider hit = OverlapBuffer[i];
                if (hit == null) continue;
                if (playerRoot != null && (hit.transform == playerRoot || hit.transform.IsChildOf(playerRoot)))
                    continue;
                reason = "The complete character volume intersects: " + hit.name;
                ClearBuffer(count);
                return false;
            }
            ClearBuffer(count);

            RaycastHit groundHit;
            Vector3 groundProbe = feet + Vector3.up * 0.15f;
            if (!Physics.Raycast(groundProbe, Vector3.down, out groundHit, 0.5f,
                groundLayers, QueryTriggerInteraction.Ignore))
            {
                reason = "The selected position has no valid floor beneath the player.";
                return false;
            }

            NavMeshHit navHit;
            if (!NavMesh.SamplePosition(feet, out navHit, 0.5f, NavMesh.AllAreas))
            {
                reason = "The selected position is not on a reachable navigation surface.";
                return false;
            }

            if (currentPosition != Vector3.zero)
            {
                NavMeshPath path = new NavMeshPath();
                if (!NavMesh.CalculatePath(currentPosition, navHit.position, NavMesh.AllAreas, path) ||
                    path.status != NavMeshPathStatus.PathComplete)
                {
                    reason = "There is no complete path to this hiding position.";
                    return false;
                }
            }
            return true;
        }

        private static float GetPoseHeight(PoseType pose, float standingHeight)
        {
            switch (pose)
            {
                case PoseType.Crouch: return standingHeight * 0.68f;
                case PoseType.Kneel: return standingHeight * 0.62f;
                case PoseType.Sit: return standingHeight * 0.58f;
                case PoseType.LieDown: return standingHeight * 0.30f;
                default: return standingHeight;
            }
        }

        private static void ClearBuffer(int count)
        {
            for (int i = 0; i < count; i++) OverlapBuffer[i] = null;
        }
    }
}
