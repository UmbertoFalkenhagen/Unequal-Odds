// ProfileRevealUI.cs
// -----------------------------------------------------------------------------
// Shows one button per identity attribute. When all ten are revealed, the script
// stores the generated PlayerProfile in the GameState singleton and loads the
// next scene.
//
// Place this script on an empty GameObject (e.g., “ProfileUI”) in the
// ProfileReveal scene, and assign all ten Button references in the Inspector.
// -----------------------------------------------------------------------------

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnequalOdds.GameData;
using UnequalOdds.Gameplay;
using UnequalOdds.Runtime;
using System.Collections;

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

        // ---------------------------------------------------------------------
        private void Awake()
        {
            // 1. Deal a random starting profile.
            profile = ProfileFactory.CreateRandom();

            // 2. Wire button listeners.
            birthWealthBtn.onClick.AddListener(() =>
                Reveal(birthWealthBtn, profile.birthWealth.ToString()));
            countryCtxBtn.onClick.AddListener(() =>
                Reveal(countryCtxBtn, profile.countryContext.ToString()));
            localeBtn.onClick.AddListener(() =>
                Reveal(localeBtn, profile.locale.ToString()));
            skinBtn.onClick.AddListener(() =>
                Reveal(skinBtn, profile.skin.ToString()));
            genderBtn.onClick.AddListener(() =>
                Reveal(genderBtn, profile.genderIdentity.ToString()));
            orientationBtn.onClick.AddListener(() =>
                Reveal(orientationBtn, profile.sexualOrientation.ToString()));
            disabilityBtn.onClick.AddListener(() =>
                Reveal(disabilityBtn, profile.disabilityStatus.ToString()));
            parentsEduBtn.onClick.AddListener(() =>
                Reveal(parentsEduBtn, profile.parentsEducation.ToString()));
            firstLangBtn.onClick.AddListener(() =>
                Reveal(firstLangBtn, profile.firstLang.ToString()));
            migrationBtn.onClick.AddListener(() =>
                Reveal(migrationBtn, profile.migrationStatus.ToString()));

            // Start Game button setup
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
            // Guard: already revealed? Do nothing.
            if (revealedButtons.Contains(btn))
                return;

            // Update visuals.
            btn.GetComponentInChildren<TextMeshProUGUI>().text = valueText;
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
                // Fallback: no button assigned, just load.
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

            // Optional: brief "Starting..." flash
            if (label != null) label.text = "Starting…";

            SceneManager.LoadScene(coreSceneName);
        }
    }
}
