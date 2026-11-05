using System;
using UnityEngine;

namespace PaintBlend.UI
{
    /// <summary>
    /// UI-facing menu contract. Matchmaking/session adapters subscribe to these
    /// events; the menu must not pretend a session was created before the backend
    /// confirms it.
    /// </summary>
    public sealed class MainMenuActionRouter : MonoBehaviour
    {
        public event Action PlayOnlineRequested;
        public event Action CreatePrivateRoomRequested;
        public event Action JoinRoomRequested;
        public event Action PracticeRequested;
        public event Action CustomizeRequested;
        public event Action SettingsRequested;

        public void OnPlayOnlinePressed() => PlayOnlineRequested?.Invoke();
        public void OnCreateRoomPressed() => CreatePrivateRoomRequested?.Invoke();
        public void OnJoinRoomPressed() => JoinRoomRequested?.Invoke();
        public void OnPracticePressed() => PracticeRequested?.Invoke();
        public void OnCustomizePressed() => CustomizeRequested?.Invoke();
        public void OnSettingsPressed() => SettingsRequested?.Invoke();
    }
}
