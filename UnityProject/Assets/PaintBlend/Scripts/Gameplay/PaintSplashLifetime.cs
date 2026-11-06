using UnityEngine;

namespace PaintBlend.Gameplay
{
    /// <summary>Visual-only decal/particle lifetime. It never applies a hit or score.</summary>
    public sealed class PaintSplashLifetime : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float lifetimeSeconds = 2f;
        private void OnEnable() { Destroy(gameObject, lifetimeSeconds); }
    }
}
