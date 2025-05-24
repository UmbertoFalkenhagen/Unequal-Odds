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

        [Header("Scene flow")]
        [SerializeField] private string coreSceneName = "CoreGameplay";

        // ---------------------------------------------------------------------
        private PlayerProfile profile;
        private readonly HashSet<Button> revealedButtons = new HashSet<Button>();
        private const int totalButtons = 10;

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

            // Check completion.
            if (revealedButtons.Count >= totalButtons)
                ContinueToGame();
        }

        private void ContinueToGame()
        {
            // Persist profile for the next scene.
            GameState.Instance.CurrentProfile = profile;

            // Load the core gameplay scene (must be in Build Settings).
            SceneManager.LoadScene(coreSceneName);
        }
    }
}
