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
    }
}
