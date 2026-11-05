using UnityEngine;
using PaintBlend.Domain;

namespace PaintBlend.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class PaintCombatIdentity : MonoBehaviour
    {
        [SerializeField] private string playerId;
        [SerializeField] private PlayerTeam team = PlayerTeam.None;
        [SerializeField] private bool eliminated;

        public string PlayerId => playerId;
        public PlayerTeam Team => team;
        public bool IsEliminated => eliminated;

        public void Assign(string id, PlayerTeam assignedTeam)
        {
            playerId = id;
            team = assignedTeam;
            eliminated = false;
        }

        public void MarkEliminated()
        {
            eliminated = true;
        }

        public void ResetForRound()
        {
            eliminated = false;
        }
    }
}
