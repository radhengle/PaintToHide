using UnityEngine;

namespace PaintBlend.Gameplay
{
    /// <summary>Client-side input/presentation gate only. Server must enforce the authoritative cooldown.</summary>
    public sealed class PaintBlasterCooldown : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float reloadSeconds = 1f;
        private float nextAllowedShotTime;

        public bool CanFire => Time.time >= nextAllowedShotTime;
        public float RemainingReload => Mathf.Max(0f, nextAllowedShotTime - Time.time);

        public bool TryBeginLocalShot()
        {
            if (!CanFire) return false;
            nextAllowedShotTime = Time.time + Mathf.Max(0f, reloadSeconds);
            return true;
        }
    }
}
