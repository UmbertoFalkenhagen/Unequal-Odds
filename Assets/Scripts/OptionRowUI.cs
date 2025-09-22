// Assets/Scripts/UI/OptionRowUI.cs
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnequalOdds.GameData;

namespace UnequalOdds.UI
{
    public class OptionRowUI : MonoBehaviour
    {
        [Header("UI refs (assign in Inspector)")]
        [SerializeField] private LayoutElement spacer;     // nudge via preferredHeight
        [SerializeField] private Toggle toggle;
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_Text conditionText;   // shows the gate reason
        [SerializeField] private TMP_Text effectText;
        [SerializeField] private CanvasGroup rowCanvasGroup;
        [SerializeField] private Image background;
        [Tooltip("Optional border image (9-sliced) for accents).")]
        [SerializeField] private Image border;

        [Header("Layout")]
        [Tooltip("Pixels to nudge DOWN when option is not locked-in.")]
        [SerializeField] private float notLockedYOffset = 12f;

        [Header("Palette / Backgrounds")]
        [SerializeField] private Color bgNeutral = Hex("#1E1F23");
        [SerializeField] private Color bgUnavailable = Hex("#2A2C31");
        [SerializeField] private Color bgPrivOn = Hex("#136F3F");
        [SerializeField] private Color bgPrivOff = Hex("#0F2A1A");
        [SerializeField] private Color bgDisadvOn = Hex("#842029");
        [SerializeField] private Color bgDisadvOff = Hex("#2A2C31");

        [Header("Palette / Text")]
        [SerializeField] private Color txtDefault = Hex("#F2F4F7");
        [SerializeField] private Color txtPassive = Hex("#A0A3A7");
        [SerializeField] private Color effPrivOn = Hex("#4CD08A");
        [SerializeField] private Color effPrivOff = Hex("#89DDB5");
        [SerializeField] private Color effDisadvOn = Hex("#FF7373");
        [SerializeField] private Color effDisadvOff = Hex("#E79A9A");

        [Header("Palette / Borders (optional)")]
        [SerializeField] private Color brdPrivilege = Hex("#2ECC71");
        [SerializeField] private Color brdDisadvantage = Hex("#FF6B6B");
        [SerializeField] private Color brdUnavailable = Hex("#6B7280");

        // Public API
        public CardOption Option { get; private set; }
        public bool HasBackground { get; private set; }
        public bool Contributes => isLockedIn;   // contributes to dice math
        public bool IsSelected => Contributes;   // compatibility
        public event Action OnSelectionChanged;

        private enum RowState
        {
            DisadvantageActiveLocked,   // has bg ? applied (auto-locked)
            DisadvantageNotApplicable,  // no bg  ? awareness only
            PrivilegeAvailable,         // has bg ? togglable, off
            PrivilegeLockedIn,          // has bg ? togglable, on
            PrivilegeUnavailable        // no bg  ? awareness only
        }

        private RowState state;
        private bool isLockedIn;

        // -------------------- Setup --------------------
        public void Setup(CardOption option, bool hasBackground)
        {
            Option = option;
            HasBackground = hasBackground;

            if (label) label.text = option?.text ?? "(missing text)";

            // baseline
            if (rowCanvasGroup) rowCanvasGroup.alpha = 1f;
            if (background) background.color = bgNeutral;
            if (border) border.color = Color.clear;
            if (toggle)
            {
                toggle.onValueChanged.RemoveAllListeners();
                toggle.isOn = false;
                toggle.interactable = false;
            }

            // Forced drawback (hard-lock if applicable)
            if (option != null && option.forcedDrawback)
            {
                if (HasBackground) SetState(RowState.DisadvantageActiveLocked);
                else SetState(RowState.DisadvantageNotApplicable);
                return;
            }

            if (option == null)
            {
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

        public void SetInteractable(bool interactable)
        {
            if (toggle && Option != null && Option.kind == OptionKind.Privilege && HasBackground)
                toggle.interactable = interactable;
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        // -------------------- State engine --------------------
        private void SetState(RowState newState)
        {
            state = newState;

            switch (state)
            {
                case RowState.DisadvantageActiveLocked:
                    isLockedIn = true;
                    if (toggle) { toggle.isOn = true; toggle.interactable = false; }
                    MoveToLockedPosition();
                    ApplyTheme(bgDisadvOn, txtDefault, effDisadvOn, brdDisadvantage, alpha: 1f);
                    SetConditionText(prefix: "Because:", tone: txtDefault);
                    break;

                case RowState.DisadvantageNotApplicable:
                    isLockedIn = false;
                    if (toggle) { toggle.isOn = false; toggle.interactable = false; }
                    MoveToNotLockedPosition();
                    ApplyTheme(bgDisadvOff, txtPassive, effDisadvOff, brdUnavailable, alpha: 0.6f);
                    SetConditionText(prefix: "Not your background:", tone: txtPassive);
                    break;

                case RowState.PrivilegeAvailable:
                    isLockedIn = false;
                    if (toggle) { toggle.isOn = false; toggle.interactable = true; toggle.onValueChanged.AddListener(OnToggleChanged); }
                    MoveToNotLockedPosition();
                    ApplyTheme(bgPrivOff, txtDefault, effPrivOff, brdPrivilege, alpha: 1f);
                    SetConditionText(prefix: "You have:", tone: effPrivOff);
                    break;

                case RowState.PrivilegeLockedIn:
                    isLockedIn = true;
                    if (toggle) { toggle.isOn = true; toggle.interactable = true; toggle.onValueChanged.AddListener(OnToggleChanged); }
                    MoveToLockedPosition();
                    ApplyTheme(bgPrivOn, txtDefault, effPrivOn, brdPrivilege, alpha: 1f);
                    SetConditionText(prefix: "You have:", tone: effPrivOn);
                    break;

                case RowState.PrivilegeUnavailable:
                    isLockedIn = false;
                    if (toggle) { toggle.isOn = false; toggle.interactable = false; }
                    MoveToNotLockedPosition();
                    ApplyTheme(bgUnavailable, txtPassive, effPrivOff, brdUnavailable, alpha: 0.6f);
                    SetConditionText(prefix: "Requires:", tone: txtPassive);
                    break;
            }

            OnSelectionChanged?.Invoke();
        }

        private void OnToggleChanged(bool on)
        {
            if (Option == null || Option.kind != OptionKind.Privilege) return;
            if (on) SetState(RowState.PrivilegeLockedIn);
            else SetState(RowState.PrivilegeAvailable);
        }

        // -------------------- Visual helpers --------------------
        private void MoveToLockedPosition()
        {
            if (!spacer) return;
            spacer.preferredHeight = 0f;
            MarkForRebuild();
        }

        private void MoveToNotLockedPosition()
        {
            if (!spacer) return;
            spacer.preferredHeight = Mathf.Max(0f, notLockedYOffset);
            MarkForRebuild();
        }

        private void ApplyTheme(Color bg, Color labelColor, Color effectColor, Color borderColor, float alpha)
        {
            if (background) background.color = bg;
            if (rowCanvasGroup) rowCanvasGroup.alpha = alpha;

            if (label) label.color = labelColor;
            if (effectText) effectText.color = effectColor;
            if (conditionText) { /* tone set in SetConditionText */ }

            if (border)
            {
                // If you use a 9-sliced border sprite, set its color; else leave clear
                border.color = borderColor;
            }

            // Recompose effect text with ASCII signs to avoid missing glyphs
            SetEffectText(effectColor);
        }

        private void SetEffectText(Color emphasizeColor)
        {
            if (!effectText || Option == null) return;

            string tgt = Option.targetShift != 0
                ? $"{(Option.targetShift > 0 ? "+" : "-")}{Mathf.Abs(Option.targetShift)} target"
                : null;

            string roll = Option.rollBonus != 0
                ? $"{(Option.rollBonus > 0 ? "+" : "-")}{Mathf.Abs(Option.rollBonus)} roll"
                : null;

            string joined = (tgt, roll) switch
            {
                (null, null) => "—",
                (not null, null) => tgt,
                (null, not null) => roll,
                _ => $"{tgt} | {roll}"
            };

            effectText.text = joined;
            effectText.color = emphasizeColor;
        }

        private void SetConditionText(string prefix, Color tone)
        {
            if (!conditionText) return;
            string core = GateConditionUtils.ToReadable(Option?.gate);
            conditionText.text = $"{prefix} {core}";
            conditionText.color = tone;
        }

        private void MarkForRebuild()
        {
            var rt = transform as RectTransform;
            if (rt != null) LayoutRebuilder.MarkLayoutForRebuild(rt);
        }

        // -------------- Utilities --------------
        private static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
            return Color.magenta;
        }

#if UNITY_EDITOR
        [ContextMenu("Auto Bind Children")]
        private void AutoBindChildren()
        {
            if (!rowCanvasGroup) rowCanvasGroup = GetComponent<CanvasGroup>();
            if (!background) background = GetComponent<Image>();
            if (!border) border = GetComponentInChildren<Image>(true) == background ? null : GetComponentInChildren<Image>(true);

            if (!spacer)
            {
                var spacers = GetComponentsInChildren<LayoutElement>(true);
                foreach (var s in spacers)
                    if (s.name.ToLower().Contains("spacer")) { spacer = s; break; }
                if (!spacer && spacers.Length > 0) spacer = spacers[0];
            }

            if (!toggle) toggle = GetComponentInChildren<Toggle>(true);

            var tmps = GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in tmps)
            {
                string n = t.name.ToLower();
                if (!label && n.Contains("label")) label = t;
                else if (!effectText && n.Contains("effect")) effectText = t;
                else if (!conditionText && (n.Contains("condition") || n.Contains("require"))) conditionText = t;
            }
            if (!label && tmps.Length > 0) label = tmps[0];
            if (!effectText && tmps.Length > 1) effectText = tmps[tmps.Length - 1];
        }

        private void Reset() => AutoBindChildren();
        private void OnValidate() => AutoBindChildren();
#endif
    }
}
