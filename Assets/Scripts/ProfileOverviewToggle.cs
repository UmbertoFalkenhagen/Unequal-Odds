// Assets/Scripts/UI/ProfileOverlayToggle.cs
using UnityEngine;
using UnequalOdds.Runtime; // GameState, PlayerProfile

namespace UnequalOdds.UI
{
    [RequireComponent(typeof(CharacterSummaryPanel))]
    public class ProfileOverlayToggle : MonoBehaviour
    {
        private CharacterSummaryPanel summary;

        private void Awake()
        {
            summary = GetComponent<CharacterSummaryPanel>();
            // Start hidden (optional)
            gameObject.SetActive(false);
        }

        /// <summary>Toggle panel on/off. Rebinds data when opening.</summary>
        public void TogglePanel()
        {
            bool show = !gameObject.activeSelf;
            gameObject.SetActive(show);

            if (show)
            {
                var profile = GameState.Instance != null ? GameState.Instance.CurrentProfile : null;
                if (profile != null) summary.Bind(profile);
            }
        }

        /// <summary>Explicit open (safe if already open).</summary>
        public void Open()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                var profile = GameState.Instance != null ? GameState.Instance.CurrentProfile : null;
                if (profile != null) summary.Bind(profile);
            }
        }

        /// <summary>Explicit close.</summary>
        public void Close() => gameObject.SetActive(false);
    }
}
