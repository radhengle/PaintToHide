using System.Collections.Generic;
using NUnit.Framework;
using PaintBlend.Domain;

namespace PaintBlend.Tests
{
    public sealed class TeamAllocatorTests
    {
        [Test]
        public void TwoParticipants_AreSplitOnePerTeam()
        {
            var roster = new List<MatchParticipant>
            {
                new MatchParticipant { PlayerId = "a" },
                new MatchParticipant { PlayerId = "b" }
            };

            string error;
            Assert.IsTrue(TeamAllocator.TryAssign(roster, 42, out error), error);
            Assert.AreNotEqual(roster[0].Team, roster[1].Team);
        }

        [Test]
        public void SevenParticipants_TeamSizesDifferByAtMostOne()
        {
            var roster = new List<MatchParticipant>();
            for (int i = 0; i < 7; i++) roster.Add(new MatchParticipant { PlayerId = "p" + i });

            string error;
            Assert.IsTrue(TeamAllocator.TryAssign(roster, 123, out error), error);
            int seekers = roster.FindAll(p => p.Team == PlayerTeam.Seekers).Count;
            int hiders = roster.FindAll(p => p.Team == PlayerTeam.Hiders).Count;
            Assert.AreEqual(7, seekers + hiders);
            Assert.LessOrEqual(System.Math.Abs(seekers - hiders), 1);
        }

        [Test]
        public void SingleParticipant_IsRejected()
        {
            var roster = new List<MatchParticipant> { new MatchParticipant { PlayerId = "solo" } };
            string error;
            Assert.IsFalse(TeamAllocator.TryAssign(roster, 1, out error));
            Assert.IsNotEmpty(error);
        }
        [Test]
        public void DuplicatePlayerIds_AreRejectedWithoutChangingTeams()
        {
            var first = new MatchParticipant { PlayerId = "same", Team = PlayerTeam.Hiders };
            var second = new MatchParticipant { PlayerId = "same", Team = PlayerTeam.Seekers };
            var roster = new List<MatchParticipant> { first, second };

            string error;
            Assert.IsFalse(TeamAllocator.TryAssign(roster, 42, out error));
            Assert.That(error, Does.Contain("duplicate PlayerId"));
            Assert.AreEqual(PlayerTeam.Hiders, first.Team);
            Assert.AreEqual(PlayerTeam.Seekers, second.Team);
        }

        [Test]
        public void EmptyPlayerId_IsRejected()
        {
            var roster = new List<MatchParticipant>
            {
                new MatchParticipant { PlayerId = "valid" },
                new MatchParticipant { PlayerId = " " }
            };

            string error;
            Assert.IsFalse(TeamAllocator.TryAssign(roster, 42, out error));
            Assert.That(error, Does.Contain("non-empty PlayerId"));
        }

        [Test]
        public void NullParticipant_IsRejected()
        {
            var roster = new List<MatchParticipant>
            {
                new MatchParticipant { PlayerId = "valid" },
                null
            };

            string error;
            Assert.IsFalse(TeamAllocator.TryAssign(roster, 42, out error));
            Assert.That(error, Does.Contain("null participant"));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public void SupportedRosterSizes_AlwaysProduceBalancedNonEmptyTeams(int count)
        {
            var roster = new List<MatchParticipant>();
            for (int i = 0; i < count; i++)
                roster.Add(new MatchParticipant { PlayerId = "player-" + i });

            string error;
            Assert.IsTrue(TeamAllocator.TryAssign(roster, 2026, out error), error);

            int seekers = roster.FindAll(p => p.Team == PlayerTeam.Seekers).Count;
            int hiders = roster.FindAll(p => p.Team == PlayerTeam.Hiders).Count;
            Assert.GreaterOrEqual(seekers, 1);
            Assert.GreaterOrEqual(hiders, 1);
            Assert.LessOrEqual(System.Math.Abs(seekers - hiders), 1);
        }
    }
}
