// ProfileRevealUI.cs
// -----------------------------------------------------------------------------
// Shows one button per identity attribute. When all ten are revealed, the script
// stores the generated PlayerProfile in the GameState singleton and loads the
// next scene.
// -----------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnequalOdds.GameData;
using UnequalOdds.Runtime;   // EnumDisplayNames, ProfileFactory
using UnequalOdds.Gameplay;

namespace UnequalOdds.UI
{
    public class ProfileRevealUI : MonoBehaviour
    {
        [Header("UI Button References (assign in Inspector)")]
        [SerializeField] private Button birthWealthBtn;
        [SerializeField] private Button countryCtxBtn;
        [SerializeField] private Button localeBtn;
        [SerializeField] private Button skinBtn;
        [SerializeField] private Button genderBtn;
        [SerializeField] private Button orientationBtn;
        [SerializeField] private Button disabilityBtn;
        [SerializeField] private Button parentsEduBtn;
        [SerializeField] private Button firstLangBtn;
        [SerializeField] private Button migrationBtn;

        [Header("Start Game")]
        [SerializeField] private Button startGameButton;        // hidden until all revealed

        [Header("Scene flow")]
        [SerializeField] private string coreSceneName = "CoreGameplay";

        // ---------------------------------------------------------------------
        private PlayerProfile profile;
        private readonly HashSet<Button> revealedButtons = new HashSet<Button>();
        private const int totalButtons = 10;
        private bool isStarting = false;

        // Handy local for formatting enum display names
        private static string D<TEnum>(TEnum v) where TEnum : System.Enum
            => EnumDisplayNames.ToDisplay(v);

        // ---------------------------------------------------------------------
        private void Awake()
        {
            // 1) Deal a random starting profile (all fields valid, non-zero flags).
            profile = ProfileFactory.CreateRandom();
            ProfileFactory.EnsureValid(ref profile); // in case older saves slip in
            if (GameState.Instance != null)
                GameState.Instance.CurrentProfile = profile;

            // 2) Wire button listeners with human-readable display text.
            //    (Keep your current field names exactly as-is.)

            if (birthWealthBtn) birthWealthBtn.onClick.AddListener(() =>
                Reveal(birthWealthBtn, D(profile.birthWealth)));
            if (countryCtxBtn) countryCtxBtn.onClick.AddListener(() =>
                Reveal(countryCtxBtn, D(profile.countryContext)));
            if (localeBtn) localeBtn.onClick.AddListener(() =>
                Reveal(localeBtn, D(profile.locale)));
            if (skinBtn) skinBtn.onClick.AddListener(() =>
                Reveal(skinBtn, D(profile.skin))); // your field name is 'skin'
            if (genderBtn) genderBtn.onClick.AddListener(() =>
                Reveal(genderBtn, D(profile.genderIdentity)));
            if (orientationBtn) orientationBtn.onClick.AddListener(() =>
                Reveal(orientationBtn, D(profile.sexualOrientation)));
            if (disabilityBtn) disabilityBtn.onClick.AddListener(() =>
                Reveal(disabilityBtn, D(profile.disabilityStatus)));
            if (parentsEduBtn) parentsEduBtn.onClick.AddListener(() =>
                Reveal(parentsEduBtn, D(profile.parentsEducation)));
            if (firstLangBtn) firstLangBtn.onClick.AddListener(() =>
                Reveal(firstLangBtn, D(profile.firstLang))); // your field name is 'firstLang'
            if (migrationBtn) migrationBtn.onClick.AddListener(() =>
                Reveal(migrationBtn, D(profile.migrationStatus)));

            // 3) Start Game button setup
            if (startGameButton != null)
            {
                startGameButton.gameObject.SetActive(false); // hidden at start
                startGameButton.onClick.RemoveAllListeners();
                startGameButton.onClick.AddListener(ContinueToGame);
            }
            else
            {
                Debug.LogWarning("ProfileRevealUI: StartGameButton is not assigned.");
            }
        }

        // ---------------------------------------------------------------------
        /// <summary>
        /// Swap the label, disable the button, and advance the reveal counter
        /// (but only once per unique button).
        /// </summary>
        private void Reveal(Button btn, string valueText)
        {
            if (btn == null) return;

            // Guard: already revealed? Do nothing.
            if (revealedButtons.Contains(btn))
                return;

            // Update visuals (find the TMP child on the button)
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = valueText;
            btn.interactable = false;

            // Track reveal.
            revealedButtons.Add(btn);

            // If all revealed, show Start Game button instead of auto-loading.
            if (revealedButtons.Count >= totalButtons && startGameButton != null)
            {
                startGameButton.gameObject.SetActive(true);
                startGameButton.interactable = true;
            }
        }

        private void ContinueToGame()
        {
            if (isStarting) return;
            isStarting = true;

            // Persist profile now so it's ready when the next scene starts.
            if (GameState.Instance == null)
            {
                Debug.LogError("GameState singleton missing in scene. Add GameState object before continuing.");
                isStarting = false;
                return;
            }
            GameState.Instance.CurrentProfile = profile;

            // Start the countdown coroutine.
            StartCoroutine(StartGameCountdown());
        }

        private IEnumerator StartGameCountdown()
        {
            if (startGameButton == null)
            {
                SceneManager.LoadScene(coreSceneName);
                yield break;
            }

            var label = startGameButton.GetComponentInChildren<TextMeshProUGUI>();
            startGameButton.interactable = false;

            for (int i = 3; i >= 1; i--)
            {
                if (label != null) label.text = $"Starting in {i}…";
                yield return new WaitForSecondsRealtime(1f);
            }

            if (label != null) label.text = "Starting…";
            SceneManager.LoadScene(coreSceneName);
        }
    }
}
