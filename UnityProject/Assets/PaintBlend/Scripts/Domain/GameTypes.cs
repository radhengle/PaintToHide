using System;
using UnityEngine;

namespace PaintBlend.Domain
{
    public enum GamePhase { Lobby, TeamAssignment, Preparation, ActiveRound, Reveal, Complete }
    public enum PlayerTeam { None, Hiders, Seekers }

    [Serializable]
    public sealed class MatchParticipant
    {
        public string PlayerId;
        public string DisplayName;
        public bool IsBot;
        public PlayerTeam Team;
    }

    public static class MatchPhaseDuration
    {
        public static int Seconds(GamePhase phase, MatchConfig config)
        {
            if (config == null) return 0;
            switch (phase)
            {
                case GamePhase.Lobby: return config.LobbySeconds;
                case GamePhase.Preparation: return config.PreparationSeconds;
                case GamePhase.ActiveRound: return config.ActiveRoundSeconds;
                case GamePhase.Reveal: return config.RevealSeconds;
                default: return 0;
            }
        }
    }
}
