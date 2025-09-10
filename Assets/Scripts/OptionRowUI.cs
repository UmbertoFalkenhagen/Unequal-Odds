using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnequalOdds.GameData;

public class OptionRowUI : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Text effectText;
    [SerializeField] private CanvasGroup rowCanvasGroup;

    public CardOption Option { get; private set; }
    public bool IsSelected => toggle != null && toggle.isOn;

    public event Action OnSelectionChanged;

    public void Setup(CardOption option, bool unlocked)
    {
        Option = option;

        // Texts
        label.text = option.text;
        string effects = "";
        if (option.targetShift != 0)
            effects += (option.targetShift > 0 ? " + " : " ") + $"{option.targetShift} target";
        if (option.rollBonus != 0)
            effects += (effects.Length > 0 ? " | " : "") + $"{(option.rollBonus >= 0 ? "+" : "")}{option.rollBonus} roll";
        effectText.text = effects.Length > 0 ? effects : "—";

        // Forced drawback ? always on and disabled
        if (option.forcedDrawback)
        {
            toggle.isOn = true;
            toggle.interactable = false;
            rowCanvasGroup.alpha = 1f;
        }
        else if (!unlocked)
        {
            // Locked: off, disabled, greyed out
            toggle.isOn = false;
            toggle.interactable = false;
            rowCanvasGroup.alpha = 0.5f;
        }
        else
        {
            // Normal selectable
            toggle.isOn = false;
            toggle.interactable = true;
            rowCanvasGroup.alpha = 1f;
            toggle.onValueChanged.AddListener(_ => OnSelectionChanged?.Invoke());
        }
    }
}
