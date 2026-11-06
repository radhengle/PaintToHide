using System.Collections.Generic;
using NUnit.Framework;
using PaintBlend.Domain;
using UnityEngine;

namespace PaintBlend.Tests
{
    public sealed class LocalPracticeMatchDirectorTests
    {
        private GameObject host;
        private MatchConfig config;
        private LocalPracticeMatchDirector director;
        private List<MatchParticipant> roster;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<MatchConfig>();
            host = new GameObject("LocalPracticeMatchDirectorTests");
            director = host.AddComponent<LocalPracticeMatchDirector>();
            typeof(LocalPracticeMatchDirector)
                .GetField("config", System.Reflection.BindingFlags.Instance |
                                    System.Reflection.BindingFlags.NonPublic)
                .SetValue(director, config);

            roster = new List<MatchParticipant>
            {
                new MatchParticipant { PlayerId = "human-a" },
                new MatchParticipant { PlayerId = "human-b" }
            };
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host);
            if (config != null) Object.DestroyImmediate(config);
        }

        private void AdvanceToActiveRound()
        {
            string error;
            Assert.IsTrue(director.StartPractice(roster, out error), error);
            host.SendMessage("AdvancePhase"); // Lobby -> team assignment -> preparation
            Assert.AreEqual(GamePhase.Preparation, director.Phase);
            host.SendMessage("AdvancePhase"); // Preparation -> active round
            Assert.AreEqual(GamePhase.ActiveRound, director.Phase);
        }

        [Test]
        public void CatchingLastHiderAwardsRoundToSeekers()
        {
            AdvanceToActiveRound();
            MatchParticipant hider = roster.Find(p => p.Team == PlayerTeam.Hiders);
            Assert.IsNotNull(hider);

            string error;
            Assert.IsTrue(director.TryMarkHiderCaught(hider.PlayerId, out error), error);
            Assert.IsTrue(hider.IsCaught);
            Assert.AreEqual(MatchWinner.Seekers, director.Winner);
            Assert.AreEqual(GamePhase.Reveal, director.Phase);
        }

        [Test]
        public void ActiveRoundExpiryAwardsHidersWhenOneSurvives()
        {
            AdvanceToActiveRound();

            host.SendMessage("AdvancePhase");
            Assert.AreEqual(MatchWinner.Hiders, director.Winner);
            Assert.AreEqual(GamePhase.Reveal, director.Phase);
        }

        [Test]
        public void CatchOutsideActiveRoundIsRejected()
        {
            string error;
            Assert.IsTrue(director.StartPractice(roster, out error), error);

            Assert.IsFalse(director.TryMarkHiderCaught("human-a", out error));
            Assert.That(error, Does.Contain("active round"));
        }

        [Test]
        public void RepeatedCatchIsRejected()
        {
            roster.Add(new MatchParticipant { PlayerId = "human-c" });
            AdvanceToActiveRound();
            MatchParticipant hider = roster.Find(p => p.Team == PlayerTeam.Hiders);
            string error;

            Assert.IsTrue(director.TryMarkHiderCaught(hider.PlayerId, out error), error);
            Assert.IsFalse(director.TryMarkHiderCaught(hider.PlayerId, out error));
            Assert.That(error, Does.Contain("already been caught"));
        }
    }
}
