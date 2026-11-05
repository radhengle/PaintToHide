using UnityEngine;

namespace PaintBlend.Gameplay
{
    public sealed class PaintBlasterController : MonoBehaviour
    {
        [SerializeField] private string playerId;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private PaintBlasterCooldown cooldown;
        [SerializeField] private MonoBehaviour authorityComponent;

        private IPaintBlasterAuthority authority;
        private uint shotSequence;

        private void Awake()
        {
            if (cooldown == null) cooldown = GetComponent<PaintBlasterCooldown>();
            if (aimCamera == null) aimCamera = Camera.main;
            authority = authorityComponent as IPaintBlasterAuthority;
        }

        /// <summary>Call from the mobile Shoot button. Server validates timing, team and hit.</summary>
        public bool TryFire()
        {
            if (cooldown == null || aimCamera == null || muzzle == null || authority == null)
            {
                Debug.LogWarning("PaintBlasterController needs a cooldown, camera, muzzle and authority adapter.", this);
                return false;
            }
            if (!cooldown.TryBeginLocalShot()) return false;

            Ray aimRay = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 direction = aimRay.direction.normalized;
            authority.RequestShot(playerId, ++shotSequence, muzzle.position, direction);
            return true;
        }
    }
}
