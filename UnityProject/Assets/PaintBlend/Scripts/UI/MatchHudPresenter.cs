using UnityEngine;
using UnityEngine.UI;
using PaintBlend.Domain;

namespace PaintBlend.UI
{
    public sealed class MatchHudPresenter : MonoBehaviour
    {
        [SerializeField] private Text phaseLabel;
        [SerializeField] private Text timerLabel;
        [SerializeField] private Text seekerCountLabel;
        [SerializeField] private Text hiderCountLabel;
        [SerializeField] private LocalPracticeMatchDirector matchDirector;

        private void OnEnable()
        {
            if (matchDirector != null) matchDirector.PhaseChanged += OnPhaseChanged;
            RefreshRoster();
        }

        private void OnDisable()
        {
            if (matchDirector != null) matchDirector.PhaseChanged -= OnPhaseChanged;
        }

        private void Update()
        {
            if (matchDirector != null && timerLabel != null)
                timerLabel.text = FormatTime(matchDirector.SecondsRemaining);
        }

        public void RefreshRoster()
        {
            if (matchDirector == null) return;
            int seekers = 0;
            int hiders = 0;
            foreach (MatchParticipant p in matchDirector.Roster)
            {
                if (p.Team == PlayerTeam.Seekers) seekers++;
                else if (p.Team == PlayerTeam.Hiders) hiders++;
            }
            if (seekerCountLabel != null) seekerCountLabel.text = "Seekers  " + seekers;
            if (hiderCountLabel != null) hiderCountLabel.text = "Hiders  " + hiders;
        }

        private void OnPhaseChanged(GamePhase phase, float remaining)
        {
            if (phaseLabel != null) phaseLabel.text = PhaseName(phase);
            if (timerLabel != null) timerLabel.text = FormatTime(remaining);
            RefreshRoster();
        }

        private static string PhaseName(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.Lobby: return "Waiting for Players";
                case GamePhase.TeamAssignment: return "Assigning Teams";
                case GamePhase.Preparation: return "Paint & Hide";
                case GamePhase.ActiveRound: return "Hide & Seek";
                case GamePhase.Reveal: return "Reveal";
                default: return "Match Complete";
            }
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.Max(0, Mathf.CeilToInt(seconds));
            return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
        }
    }
}
