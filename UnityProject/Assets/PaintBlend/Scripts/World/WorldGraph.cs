using System.Collections.Generic;
using UnityEngine;

namespace PaintBlend.World
{
    /// <summary>Runtime adjacency graph for the continuous room/corridor network.</summary>
    [DisallowMultipleComponent]
    public sealed class WorldGraph : MonoBehaviour
    {
        [SerializeField] private float horizontalConnectionTolerance = 1.2f;
        private readonly List<RoomMarker> rooms = new List<RoomMarker>();

        public IReadOnlyList<RoomMarker> Rooms => rooms;

        public void Rebuild()
        {
            rooms.Clear();
            RoomMarker[] found = GetComponentsInChildren<RoomMarker>(true);
            for (int i = 0; i < found.Length; i++)
                if (found[i] != null) rooms.Add(found[i]);
        }

        public List<RoomMarker> GetNeighbours(RoomMarker source)
        {
            var result = new List<RoomMarker>();
            if (source == null) return result;
            Vector3 origin = source.transform.position;

            for (int i = 0; i < rooms.Count; i++)
            {
                RoomMarker candidate = rooms[i];
                if (candidate == null || candidate == source || candidate.FloorIndex != source.FloorIndex)
                    continue;

                Vector3 delta = candidate.transform.position - origin;
                bool sideBySide = Mathf.Abs(delta.x) > 0.1f &&
                                  Mathf.Abs(delta.z) <= horizontalConnectionTolerance;
                bool frontBack = Mathf.Abs(delta.z) > 0.1f &&
                                 Mathf.Abs(delta.x) <= horizontalConnectionTolerance;
                if ((sideBySide || frontBack) &&
                    (Mathf.Abs(delta.x) <= 23f && Mathf.Abs(delta.z) <= 21f))
                    result.Add(candidate);
            }
            return result;
        }

        public List<RoomMarker> FindRoute(RoomMarker start, RoomMarker destination)
        {
            var route = new List<RoomMarker>();
            if (start == null || destination == null) return route;
            var previous = new Dictionary<RoomMarker, RoomMarker>();
            var queue = new Queue<RoomMarker>();
            var visited = new HashSet<RoomMarker>();
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                RoomMarker current = queue.Dequeue();
                if (current == destination) break;
                List<RoomMarker> neighbours = GetNeighbours(current);
                for (int i = 0; i < neighbours.Count; i++)
                {
                    RoomMarker next = neighbours[i];
                    if (!visited.Add(next)) continue;
                    previous[next] = current;
                    queue.Enqueue(next);
                }
            }

            if (!visited.Contains(destination)) return route;
            RoomMarker step = destination;
            route.Add(step);
            while (step != start)
            {
                if (!previous.TryGetValue(step, out step)) return new List<RoomMarker>();
                route.Add(step);
            }
            route.Reverse();
            return route;
        }
    }
}
