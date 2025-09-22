
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnequalOdds.GameData; // CardOption, OptionKind

namespace UnequalOdds.UI
{
    public class OptionRowUI : MonoBehaviour
    {
        [Header("UI refs (assign in Inspector)")]
        [SerializeField] private LayoutElement spacer;     // nudge via preferredHeight
        [SerializeField] private Toggle toggle;
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_Text effectText;
        [SerializeField] private CanvasGroup rowCanvasGroup;
        [SerializeField] private Image background;         // optional tint

        [Header("Visuals")]
        [Tooltip("How many pixels to nudge DOWN when not locked-in.")]
        [SerializeField] private float notLockedYOffset = 12f;

        [Tooltip("Dimmed text color for 'not my background' awareness rows.")]
        [SerializeField] private Color passiveGrey = new Color(0.65f, 0.65f, 0.65f, 1f);

        [Tooltip("Active disadvantage background tint.")]
        [SerializeField] private Color negActiveBg = new Color(0.35f, 0.08f, 0.08f, 1f);

        [Tooltip("Active privilege background tint.")]
        [SerializeField] private Color posActiveBg = new Color(0.10f, 0.30f, 0.18f, 1f);

        [Tooltip("Neutral background tint.")]
        [SerializeField] private Color neutralBg = new Color(0.16f, 0.16f, 0.16f, 1f);

        [SerializeField] private Color textDefault = new Color(0.90f, 0.90f, 0.90f, 1f);
        [SerializeField] private Color textRed = new Color(0.90f, 0.30f, 0.30f, 1f);
        [SerializeField] private Color textGreen = new Color(0.30f, 0.85f, 0.55f, 1f);

        // Public API -----------------------------------------------------------
        public CardOption Option { get; private set; }

        /// <summary>True if player matches the gate (has the relevant background).</summary>
        public bool HasBackground { get; private set; }

        /// <summary>True when this row actually affects target/roll (locked-in).</summary>
        public bool Contributes => isLockedIn;

        /// <summary>Compatibility with older callers. Same as Contributes.</summary>
        public bool IsSelected => Contributes;

        public event Action OnSelectionChanged;

        // Internal state -------------------------------------------------------
        private enum RowState
        {
            DisadvantageActiveLocked,   // has background ? auto-locked, contributes
            DisadvantageNotApplicable,  // no background  ? awareness only
            PrivilegeAvailable,         // has background ? togglable, starts unlocked
            PrivilegeLockedIn,          // toggled on     ? contributes
            PrivilegeUnavailable        // no background  ? awareness only
        }

        private RowState state;
        private bool isLockedIn;

        // Setup ---------------------------------------------------------------
        /// <summary>
        /// Initialize visuals & behavior based on the option and whether the player
        /// matches its gate (hasBackground).
        /// </summary>
        public void Setup(CardOption option, bool hasBackground)
        {
            Option = option;
            HasBackground = hasBackground;

            if (label) label.text = option?.text ?? "(missing text)";

            // baseline visuals
            if (rowCanvasGroup) rowCanvasGroup.alpha = 1f;
            if (background) background.color = neutralBg;
            if (toggle)
            {
                toggle.onValueChanged.RemoveAllListeners();
                toggle.isOn = false;
                toggle.interactable = false;
            }

            // Forced drawback override (non-interactable when applicable)
            if (option != null && option.forcedDrawback)
            {
                if (HasBackground)
                {
                    SetState(RowState.DisadvantageActiveLocked);
                }
                else
                {
                    SetState(RowState.DisadvantageNotApplicable);
                }
                return;
            }

            // Standard state machine
            if (option == null)
            {
                // Defensive fallback: show as awareness row
                SetState(RowState.PrivilegeUnavailable);
                return;
            }

            if (option.kind == OptionKind.Disadvantage)
            {
                if (HasBackground) SetState(RowState.DisadvantageActiveLocked);
                else SetState(RowState.DisadvantageNotApplicable);
            }
            else // Privilege
            {
                if (HasBackground) SetState(RowState.PrivilegeAvailable);
                else SetState(RowState.PrivilegeUnavailable);
            }
        }

        // External helpers ----------------------------------------------------
        /// <summary>Enable/disable user interaction (used during outcome sequence).</summary>
        public void SetInteractable(bool interactable)
        {
            if (!toggle) return;

            // Only privileges with background are toggleable by the user.
            if (Option != null && Option.kind == OptionKind.Privilege && HasBackground)
                toggle.interactable = interactable;
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        // State handling ------------------------------------------------------
        private void SetState(RowState newState)
        {
            state = newState;

            switch (state)
            {
                case RowState.DisadvantageActiveLocked:
                    isLockedIn = true;
                    if (toggle)
                    {
                        toggle.isOn = true;
                        toggle.interactable = false;
                    }
                    MoveToLockedPosition();
                    ApplyColors(active: true, isNegative: true, emphasizeForOthers: false);
                    SetEffectText(textRed);
                    break;

                case RowState.DisadvantageNotApplicable:
                    isLockedIn = false;
                    if (toggle)
                    {
                        toggle.isOn = false;
                        toggle.interactable = false;
                    }
                    MoveToNotLockedPosition();
                    ApplyColors(active: false, isNegative: true, emphasizeForOthers: true);
                    SetEffectText(textRed);
                    break;

                case RowState.PrivilegeAvailable:
                    isLockedIn = false;
                    if (toggle)
                    {
                        toggle.isOn = false;
                        toggle.interactable = true;
                        toggle.onValueChanged.AddListener(OnToggleChanged);
                    }
                    MoveToNotLockedPosition();
                    ApplyColors(active: false, isNegative: false, emphasizeForOthers: false);
                    SetEffectText(textDefault);
                    break;

                case RowState.PrivilegeLockedIn:
                    isLockedIn = true;
                    if (toggle)
                    {
                        toggle.isOn = true;
                        toggle.interactable = true; // allow turning it off again
                        toggle.onValueChanged.AddListener(OnToggleChanged);
                    }
                    MoveToLockedPosition();
                    ApplyColors(active: true, isNegative: false, emphasizeForOthers: false);
                    SetEffectText(textGreen);
                    break;

                case RowState.PrivilegeUnavailable:
                    isLockedIn = false;
                    if (toggle)
                    {
                        toggle.isOn = false;
                        toggle.interactable = false;
                    }
                    MoveToNotLockedPosition();
                    ApplyColors(active: false, isNegative: false, emphasizeForOthers: true);
                    SetEffectText(textGreen);
                    break;
            }

            OnSelectionChanged?.Invoke();
        }

        private void OnToggleChanged(bool on)
        {
            // Only privileges can toggle
            if (Option == null || Option.kind != OptionKind.Privilege) return;

            if (on) SetState(RowState.PrivilegeLockedIn);
            else SetState(RowState.PrivilegeAvailable);
        }

        // Visuals -------------------------------------------------------------
        private void MoveToLockedPosition()
        {
            if (spacer)
            {
                spacer.preferredHeight = 0f;
                MarkForRebuild();
            }
        }

        private void MoveToNotLockedPosition()
        {
            if (spacer)
            {
                spacer.preferredHeight = Mathf.Max(0f, notLockedYOffset);
                MarkForRebuild();
            }
        }

        private void ApplyColors(bool active, bool isNegative, bool emphasizeForOthers)
        {
            // Background tint
            if (background)
            {
                if (active)
                    background.color = isNegative ? negActiveBg : posActiveBg;
                else
                    background.color = neutralBg;
            }

            // Row alpha (grey-out for “others” awareness)
            if (rowCanvasGroup)
                rowCanvasGroup.alpha = emphasizeForOthers ? 0.6f : 1f;

            // Label color
            if (label)
                label.color = emphasizeForOthers ? passiveGrey : textDefault;
        }

        private void SetEffectText(Color emphasizeColor)
        {
            if (!effectText || Option == null) return;

            string tgt = Option.targetShift != 0
                ? $"{(Option.targetShift > 0 ? "+" : "?")}{Mathf.Abs(Option.targetShift)} target"
                : null;

            string roll = Option.rollBonus != 0
                ? $"{(Option.rollBonus > 0 ? "+" : "?")}{Mathf.Abs(Option.rollBonus)} roll"
                : null;

            string joined = (tgt, roll) switch
            {
                (null, null) => "—",
                (not null, null) => tgt,
                (null, not null) => roll,
                _ => $"{tgt} | {roll}"
            };

            if (emphasizeColor == default) emphasizeColor = textDefault;
            string hex = ColorUtility.ToHtmlStringRGB(emphasizeColor);
            effectText.text = $"<color=#{hex}>{joined}</color>";
        }

        private void MarkForRebuild()
        {
            // Ensure the layout updates immediately
            var rt = transform as RectTransform;
            if (rt != null) LayoutRebuilder.MarkLayoutForRebuild(rt);
        }

        // Editor convenience --------------------------------------------------
#if UNITY_EDITOR
        [ContextMenu("Auto Bind Children")]
        private void AutoBindChildren()
        {
            if (!rowCanvasGroup) rowCanvasGroup = GetComponent<CanvasGroup>();
            if (!background) background = GetComponent<Image>();

            if (!spacer)
            {
                // Try to find a LayoutElement named "Spacer"
                var spacers = GetComponentsInChildren<LayoutElement>(true);
                foreach (var s in spacers)
                {
                    if (s.name.ToLower().Contains("spacer")) { spacer = s; break; }
                }
                if (!spacer && spacers.Length > 0) spacer = spacers[0];
            }

            if (!toggle) toggle = GetComponentInChildren<Toggle>(true);
            if (!label)
            {
                var tmps = GetComponentsInChildren<TMP_Text>(true);
                foreach (var t in tmps)
                    if (t.name.ToLower().Contains("label")) { label = t; break; }
                if (!label && tmps.Length > 0) label = tmps[0];
            }
            if (!effectText)
            {
                var tmps = GetComponentsInChildren<TMP_Text>(true);
                foreach (var t in tmps)
                    if (t.name.ToLower().Contains("effect")) { effectText = t; break; }
                if (!effectText && tmps.Length > 1) effectText = tmps[tmps.Length - 1];
            }

            // Sanity logs
            if (!spacer) Debug.LogWarning($"{name}: Spacer (LayoutElement) not found.");
            if (!toggle) Debug.LogWarning($"{name}: Toggle not found.");
            if (!label) Debug.LogWarning($"{name}: Label (TMP_Text) not found.");
            if (!effectText) Debug.LogWarning($"{name}: EffectText (TMP_Text) not found.");
            if (!rowCanvasGroup) Debug.LogWarning($"{name}: CanvasGroup not found.");
        }

        private void Reset() => AutoBindChildren();
        private void OnValidate() => AutoBindChildren();
#endif
    }
}
