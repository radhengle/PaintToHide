using System;
using System.Collections.Generic;
using UnityEngine;
using PaintBlend.Domain;

namespace PaintBlend.Gameplay
{
    /// <summary>
    /// Local practice-mode authority for rapid gameplay iteration. This is not
    /// an online server and must never be used for public matchmaking.
    /// </summary>
    public sealed class LocalPracticePaintBlasterAuthority : MonoBehaviour, IPaintBlasterAuthority
    {
        [SerializeField] private LocalPracticeMatchDirector matchDirector;
        [SerializeField] private LayerMask hitLayers = ~0;
        [SerializeField, Min(1f)] private float maxRange = 35f;
        [SerializeField, Min(0f)] private float reloadSeconds = 1f;
        [SerializeField] private GameObject splashPrefab;

        private readonly Dictionary<string, float> nextAllowedShot = new Dictionary<string, float>();
        public event Action<PaintCombatIdentity> PlayerCaught;
        public event Action<Vector3, Vector3> PaintImpact;

        public void RequestShot(string shooterPlayerId, uint shotSequence, Vector3 origin, Vector3 direction)
        {
            if (matchDirector == null || matchDirector.Phase != GamePhase.ActiveRound) return;
            PaintCombatIdentity shooter = FindIdentity(shooterPlayerId);
            if (shooter == null || shooter.IsEliminated) return;

            float nextTime;
            if (nextAllowedShot.TryGetValue(shooterPlayerId, out nextTime) && Time.time < nextTime)
                return;
            nextAllowedShot[shooterPlayerId] = Time.time + reloadSeconds;

            RaycastHit hit;
            if (!Physics.Raycast(origin, direction.normalized, out hit, maxRange, hitLayers,
                QueryTriggerInteraction.Ignore))
                return;

            PaintImpact?.Invoke(hit.point, hit.normal);
            SpawnSplash(hit.point, hit.normal);

            PaintCombatIdentity target = hit.collider.GetComponentInParent<PaintCombatIdentity>();
            if (target == null || target.IsEliminated || target.Team == shooter.Team) return;

            // Practice-mode rules mirror the intended match rules: only a Seeker
            // can catch a Hider. Hider shots can create visual paint evidence but
            // cannot eliminate a Seeker.
            if (shooter.Team == PlayerTeam.Seekers && target.Team == PlayerTeam.Hiders)
            {
                target.MarkEliminated();
                PlayerCaught?.Invoke(target);
            }
        }

        private PaintCombatIdentity FindIdentity(string id)
        {
            PaintCombatIdentity[] all = FindObjectsOfType<PaintCombatIdentity>();
            for (int i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].PlayerId == id) return all[i];
            return null;
        }

        private void SpawnSplash(Vector3 point, Vector3 normal)
        {
            if (splashPrefab == null) return;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, normal);
            GameObject splash = Instantiate(splashPrefab, point + normal * 0.025f, rotation);
            PaintSplashLifetime lifetime = splash.GetComponent<PaintSplashLifetime>();
            if (lifetime == null) splash.AddComponent<PaintSplashLifetime>();
        }
    }
}
