using UnityEngine;

namespace PaintBlend.World
{
    [DisallowMultipleComponent]
    public sealed class RoomMarker : MonoBehaviour
    {
        [SerializeField] private string roomId;
        [SerializeField] private string displayName;
        [SerializeField] private string theme;
        [SerializeField] private int floorIndex;

        public string RoomId => roomId;
        public string DisplayName => displayName;
        public string Theme => theme;
        public int FloorIndex => floorIndex;

        public void Configure(string id, string title, string roomTheme, int floor)
        {
            roomId = id;
            displayName = title;
            theme = roomTheme;
            floorIndex = floor;
        }
    }
}
