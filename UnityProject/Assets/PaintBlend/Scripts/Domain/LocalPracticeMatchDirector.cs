using System;
using System.Collections.Generic;
using UnityEngine;

namespace PaintBlend.Domain
{
    /// <summary>
    /// Offline/practice-only match clock. This component is deliberately not an
    /// online authority. Networked matches must drive the same phase model from
    /// a trusted server implementation.
    /// </summary>
    public sealed class LocalPracticeMatchDirector : MonoBehaviour
    {
        [SerializeField] private MatchConfig config;
        private readonly List<MatchParticipant> roster = new List<MatchParticipant>();
        private GamePhase phase = GamePhase.Complete;
        private float secondsRemaining;
        private bool running;
        private MatchWinner winner = MatchWinner.None;

        public GamePhase Phase => phase;
        public MatchWinner Winner => winner;
        public float SecondsRemaining => secondsRemaining;
        public IReadOnlyList<MatchParticipant> Roster => roster;
        public bool IsOnlineAuthority => false;

        public event Action<GamePhase, float> PhaseChanged;
        public event Action<IReadOnlyList<MatchParticipant>> RosterAssigned;
        public event Action<MatchParticipant> ParticipantCaught;

        public bool StartPractice(IList<MatchParticipant> participants, out string error)
        {
            error = null;
            if (config == null) { error = "Assign a MatchConfig asset first."; return false; }
            if (participants == null || participants.Count < config.MinimumPlayers)
            {
                error = "The practice match does not meet the minimum player count.";
                return false;
            }
            if (participants.Count > config.MaximumPlayers)
            {
                error = "The roster exceeds the configured maximum player count.";
                return false;
            }

            roster.Clear();
            winner = MatchWinner.None;
            for (int i = 0; i < participants.Count; i++)
            {
                MatchParticipant p = participants[i];
                if (p == null || string.IsNullOrWhiteSpace(p.PlayerId))
                {
                    error = "Every participant needs a stable PlayerId.";
                    roster.Clear();
                    return false;
                }
                p.Team = PlayerTeam.None;
                p.IsCaught = false;
                roster.Add(p);
            }

            running = true;
            SetPhase(GamePhase.Lobby);
            return true;
        }

        private void Update()
        {
            if (!running) return;
            if (secondsRemaining > 0f)
            {
                secondsRemaining = Mathf.Max(0f, secondsRemaining - Time.deltaTime);
                if (secondsRemaining > 0f)
                {
                    PhaseChanged?.Invoke(phase, secondsRemaining);
                    return;
                }
            }
            AdvancePhase();
        }

        /// <summary>
        /// Applies a server-confirmed or local-practice catch. This component is
        /// offline-only; online callers must invoke equivalent logic on the server.
        /// </summary>
        public bool TryMarkHiderCaught(string playerId, out string error)
        {
            error = null;
            if (phase != GamePhase.ActiveRound)
            {
                error = "Players can only be caught during the active round.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(playerId))
            {
                error = "A valid PlayerId is required.";
                return false;
            }

            MatchParticipant target = roster.Find(p => p.PlayerId == playerId);
            if (target == null)
            {
                error = "The player is not part of this match.";
                return false;
            }
            if (target.Team != PlayerTeam.Hiders)
            {
                error = "Only Hiders can be caught.";
                return false;
            }
            if (target.IsCaught)
            {
                error = "This Hider has already been caught.";
                return false;
            }

            target.IsCaught = true;
            ParticipantCaught?.Invoke(target);
            if (CountRemainingHiders() == 0)
            {
                winner = MatchWinner.Seekers;
                SetPhase(GamePhase.Reveal);
            }
            return true;
        }

        private int CountRemainingHiders()
        {
            int remaining = 0;
            for (int i = 0; i < roster.Count; i++)
                if (roster[i].Team == PlayerTeam.Hiders && !roster[i].IsCaught)
                    remaining++;
            return remaining;
        }

        private void AdvancePhase()
        {
            switch (phase)
            {
                case GamePhase.Lobby:
                    SetPhase(GamePhase.TeamAssignment);
                    string error;
                    if (!TeamAllocator.TryAssign(roster, Environment.TickCount, out error))
                    {
                        running = false;
                        Debug.LogError("Paint & Blend team assignment failed: " + error, this);
                        SetPhase(GamePhase.Complete);
                        return;
                    }
                    RosterAssigned?.Invoke(roster);
                    SetPhase(GamePhase.Preparation);
                    break;
                case GamePhase.TeamAssignment:
                    SetPhase(GamePhase.Preparation);
                    break;
                case GamePhase.Preparation:
                    SetPhase(GamePhase.ActiveRound);
                    break;
                case GamePhase.ActiveRound:
                    winner = CountRemainingHiders() > 0 ? MatchWinner.Hiders : MatchWinner.Seekers;
                    SetPhase(GamePhase.Reveal);
                    break;
                case GamePhase.Reveal:
                    running = false;
                    SetPhase(GamePhase.Complete);
                    break;
                default:
                    running = false;
                    break;
            }
        }

        private void SetPhase(GamePhase next)
        {
            phase = next;
            secondsRemaining = MatchPhaseDuration.Seconds(next, config);
            PhaseChanged?.Invoke(phase, secondsRemaining);
        }
    }
}
