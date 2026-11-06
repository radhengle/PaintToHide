using UnityEngine;

namespace PaintBlend.Domain
{
    [CreateAssetMenu(menuName = "Paint & Blend/Match Config", fileName = "MatchConfig")]
    public sealed class MatchConfig : ScriptableObject
    {
        [Header("Lobby and round")]
        [SerializeField, Min(2)] private int minimumPlayers = 2;
        [SerializeField, Min(2)] private int maximumPlayers = 8;
        [SerializeField, Min(0)] private int lobbySeconds = 90;
        [SerializeField, Min(0)] private int preparationSeconds = 90;
        [SerializeField, Min(1)] private int activeRoundSeconds = 180;
        [SerializeField, Min(0)] private int revealSeconds = 30;

        [Header("Character and blaster")]
        [SerializeField, Min(0.1f)] private float nominalCharacterHeight = 1.75f;
        [SerializeField, Min(0f)] private float blasterReloadSeconds = 1f;
        [SerializeField, Min(0f)] private float splashLifetimeSeconds = 2f;

        public int MinimumPlayers => minimumPlayers;
        public int MaximumPlayers => maximumPlayers;
        public int LobbySeconds => lobbySeconds;
        public int PreparationSeconds => preparationSeconds;
        public int ActiveRoundSeconds => activeRoundSeconds;
        public int RevealSeconds => revealSeconds;
        public float NominalCharacterHeight => nominalCharacterHeight;
        public float BlasterReloadSeconds => blasterReloadSeconds;
        public float SplashLifetimeSeconds => splashLifetimeSeconds;

        private void OnValidate()
        {
            minimumPlayers = Mathf.Max(2, minimumPlayers);
            maximumPlayers = Mathf.Max(minimumPlayers, maximumPlayers);
            lobbySeconds = Mathf.Max(0, lobbySeconds);
            preparationSeconds = Mathf.Max(0, preparationSeconds);
            activeRoundSeconds = Mathf.Max(1, activeRoundSeconds);
            revealSeconds = Mathf.Max(0, revealSeconds);
            nominalCharacterHeight = Mathf.Max(0.1f, nominalCharacterHeight);
            blasterReloadSeconds = Mathf.Max(0f, blasterReloadSeconds);
            splashLifetimeSeconds = Mathf.Max(0f, splashLifetimeSeconds);
        }
    }
}
