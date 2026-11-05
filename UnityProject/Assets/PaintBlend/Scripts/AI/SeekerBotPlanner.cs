using System.Collections.Generic;
using UnityEngine;
using PaintBlend.World;

namespace PaintBlend.AI
{
    public sealed class SeekerBotPlanner
    {
        private readonly Dictionary<string, float> roomSuspicion = new Dictionary<string, float>();
        private readonly HashSet<string> searchedRooms = new HashSet<string>();

        /// <summary>
        /// Consume only observations available to a seeker (sight, noise, paint
        /// evidence, suspicious silhouette). Never pass hidden-player transforms.
        /// </summary>
        public void RecordObservation(RoomMarker room, float suspicion)
        {
            if (room == null) return;
            float value = Mathf.Clamp01(suspicion);
            float current;
            roomSuspicion.TryGetValue(room.RoomId, out current);
            roomSuspicion[room.RoomId] = Mathf.Max(current, value);
        }

        public RoomMarker ChooseNextRoom(IList<RoomMarker> reachableRooms, Vector3 seekerPosition)
        {
            if (reachableRooms == null || reachableRooms.Count == 0) return null;
            RoomMarker best = null;
            float bestScore = float.NegativeInfinity;

            for (int i = 0; i < reachableRooms.Count; i++)
            {
                RoomMarker room = reachableRooms[i];
                if (room == null) continue;
                float suspicion;
                roomSuspicion.TryGetValue(room.RoomId, out suspicion);
                bool searched = searchedRooms.Contains(room.RoomId);
                float distance = Vector3.Distance(seekerPosition, room.transform.position);
                float score = suspicion * 0.75f + (searched ? -0.4f : 0.15f) -
                              Mathf.Clamp01(distance / 100f) * 0.2f;
                if (score > bestScore) { bestScore = score; best = room; }
            }
            if (best != null) searchedRooms.Add(best.RoomId);
            return best;
        }

        public void ResetRound()
        {
            roomSuspicion.Clear();
            searchedRooms.Clear();
        }
    }
}
