using System;
using System.Collections.Generic;

namespace PaintBlend.Domain
{
    public static class TeamAllocator
    {
        /// <summary>Shuffles a roster and assigns near-equal Hider/Seeker teams.</summary>
        public static bool TryAssign(IList<MatchParticipant> participants, int seed, out string error)
        {
            error = null;
            if (participants == null || participants.Count < 2)
            {
                error = "At least two participants are required.";
                return false;
            }

            var indices = new List<int>(participants.Count);
            for (int i = 0; i < participants.Count; i++)
            {
                if (participants[i] == null)
                {
                    error = "Roster contains a null participant.";
                    return false;
                }
                indices.Add(i);
            }

            var random = new Random(seed);
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int swap = indices[i];
                indices[i] = indices[j];
                indices[j] = swap;
            }

            // Start with one on each team. Extra participants alternate to keep
            // the team-size difference at no more than one.
            int seekerCount = 0;
            int hiderCount = 0;
            for (int i = 0; i < indices.Count; i++)
            {
                bool assignSeeker = i == 0 || (i % 2 == 0 && seekerCount <= hiderCount);
                MatchParticipant participant = participants[indices[i]];
                participant.Team = assignSeeker ? PlayerTeam.Seekers : PlayerTeam.Hiders;
                if (assignSeeker) seekerCount++;
                else hiderCount++;
            }

            if (seekerCount == 0 || hiderCount == 0)
            {
                error = "Team assignment must produce at least one player on each team.";
                return false;
            }
            return true;
        }
    }
}
