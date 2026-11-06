using UnityEngine;

namespace PaintBlend.Gameplay
{
    /// <summary>
    /// Implemented by the trusted match server/network adapter. A client never
    /// decides hit results or eliminations locally.
    /// </summary>
    public interface IPaintBlasterAuthority
    {
        void RequestShot(string shooterPlayerId, uint shotSequence, Vector3 origin, Vector3 direction);
    }
}
