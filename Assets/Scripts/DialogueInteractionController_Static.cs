using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnequalOdds.GameData;
using UnequalOdds.Gameplay;
using UnequalOdds.Runtime;   // for TurnLogEntry / LoggedOption (safe to keep even if unused)
using UnequalOdds.UI;        // OptionRowUI

public class DialogueInteractionController_Static : MonoBehaviour
{
    [Header("HeaderBar")]
    [SerializeField] private TMP_Text TurnCounter;   // HeaderBar/Turn Counter (TMP)
    [SerializeField] private TMP_Text Header;        // HeaderBar/Header (TMP)
    [SerializeField] private Button Btn_Menu;      // optional

    [Header("Card Texts")]
    [SerializeField] private TMP_Text DialogueCardName_Text;   // DialogueCardName/Text (TMP)
    [SerializeField] private TMP_Text DialogueCardBody_Text;   // CardTextPanel/DialogueCardBody/Text (TMP)

    [Header("StatsPanel")]
    [SerializeField] private TMP_Text Box_BaseTarget;
    [SerializeField] private TMP_Text Box_TargetNow;
    [SerializeField] private TMP_Text Box_RollMod;

    [Header("RollPanel")]
    [SerializeField] private Button Btn_RollDie;
    [SerializeField] private TMP_Text Box_RollResult;

    [Header("OptionsArea (exactly 5)")]
    [SerializeField] private OptionRowUI OptionSlot_1;
    [SerializeField] private OptionRowUI OptionSlot_2;
    [SerializeField] private OptionRowUI OptionSlot_3;
    [SerializeField] private OptionRowUI OptionSlot_4;
    [SerializeField] private OptionRowUI OptionSlot_5;

    [Header("Data Source")]
    [SerializeField] private List<DialogueCard> allCards = new List<DialogueCard>();

    [Header("Colors")]
    [SerializeField] private Color baseGrey = new Color(0.55f, 0.55f, 0.55f, 1f);
    [SerializeField] private Color targetSame = new Color(0.15f, 0.15f, 0.15f, 1f);
    [SerializeField] private Color targetDown = new Color(0.13f, 0.65f, 0.48f, 1f);
    [SerializeField] private Color targetUp = new Color(0.79f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color successGreen = new Color(0.13f, 0.70f, 0.45f, 1f);
    [SerializeField] private Color failRed = new Color(0.85f, 0.25f, 0.25f, 1f);

    private PlayerProfile profile;
    private DialogueCard currentCard;
    private OptionRowUI[] slots;

    // turn context (for header or logging)
    private int currentTurnIndex = 0;
    private int currentTurnsTotal = 10;

    // Events
    public event System.Action<TurnLogEntry> OnResolved;                    // optional logging hook
    public event System.Action<DialogueCard, bool> OnCardFinished;          // fired after 5s + 3s countdown

    private void Awake()
    {
        slots = new[] { OptionSlot_1, OptionSlot_2, OptionSlot_3, OptionSlot_4, OptionSlot_5 };
    }

    private void Start()
    {
        profile = GameState.Instance != null ? GameState.Instance.CurrentProfile : ProfileFactory.CreateRandom();
    }

    public void SetCardPool(List<DialogueCard> cards) => allCards = cards;

    /// <summary>Called by your Life Goal selection to present a random card from the current pool.</summary>
    public void PresentRandomCard(LifeGoal goal, int turnIndex = 0, int turnsTotal = 10)
    {
        currentTurnIndex = turnIndex;
        currentTurnsTotal = turnsTotal;

        if (TurnCounter) TurnCounter.text = $"Turn {turnIndex + 1} / {turnsTotal}";
        if (Header) Header.text = $"Life Goal: {goal}";

        // draw random card from current pool
        var pool = allCards.FindAll(c => c.goal == goal);
        if (pool.Count == 0)
        {
            Debug.LogWarning($"No DialogueCards available for goal {goal}.");
            return;
        }
        currentCard = pool[Random.Range(0, pool.Count)];

        if (DialogueCardName_Text) DialogueCardName_Text.text = currentCard.cardTitle;
        if (DialogueCardBody_Text) DialogueCardBody_Text.text = currentCard.cardBody;

        if (Box_BaseTarget)
        {
            Box_BaseTarget.text = $"Base Target: ? {currentCard.baseTarget}";
            Box_BaseTarget.color = baseGrey;
        }

        BindOptionsToFiveSlots();
        Btn_RollDie.onClick.RemoveAllListeners();
        Btn_RollDie.interactable = true;
        Btn_RollDie.onClick.AddListener(RollAndResolve);

        if (Box_RollResult) Box_RollResult.text = string.Empty;
        RecomputeSummary();
    }

    private void BindOptionsToFiveSlots()
    {
        if (slots == null || slots.Length != 5)
        {
            Debug.LogError("[DialogueCtrl] Option slots array is invalid or not 5.");
            return;
        }
        if (currentCard == null)
        {
            Debug.LogError("[DialogueCtrl] No current card.");
            return;
        }

        int count = currentCard.options != null ? currentCard.options.Count : 0;
        if (count == 0)
        {
            Debug.LogWarning($"[DialogueCtrl] Card '{currentCard.cardTitle}' has 0 options. " +
                             "Did you fill the Options list on the asset?");
            // Show empty, disabled rows so you at least see the UI
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i]) { Debug.LogError($"[DialogueCtrl] Slot {i + 1} not assigned."); continue; }
                slots[i].gameObject.SetActive(true);
                // Optional: add a 'SetupEmpty' helper to OptionRowUI; or just disable its Toggle here:
                // slots[i].SetVisible(true); // if you added such helper
            }
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (!slot)
            {
                Debug.LogError($"[DialogueCtrl] OptionSlot_{i + 1} is not assigned in the Inspector.");
                continue;
            }

            if (i < count)
            {
                var opt = currentCard.options[i];
                if (opt == null)
                {
                    Debug.LogError($"[DialogueCtrl] Option {i + 1} on card '{currentCard.cardTitle}' is null.");
                    slot.gameObject.SetActive(false);
                    continue;
                }

                bool hasBackground = opt.gate == null || opt.gate.Evaluate(profile);
                slot.gameObject.SetActive(true);
                slot.Setup(opt, hasBackground);
                slot.OnSelectionChanged -= RecomputeSummary;
                slot.OnSelectionChanged += RecomputeSummary;
            }
            else
            {
                // hide unused trailing slots
                slot.gameObject.SetActive(false);
            }
        }
    }

    private void RecomputeSummary()
    {
        int target = currentCard.baseTarget;
        int rollMod = 0;

        foreach (var slot in slots)
        {
            if (slot == null || !slot.gameObject.activeSelf || slot.Option == null) continue;
            if (!slot.Contributes) continue;
            target += slot.Option.targetShift;
            rollMod += slot.Option.rollBonus;
        }

        if (Box_TargetNow)
        {
            Box_TargetNow.text = $"Current Target: ? {target}";
            if (target < currentCard.baseTarget) Box_TargetNow.color = targetDown;
            else if (target > currentCard.baseTarget) Box_TargetNow.color = targetUp;
            else Box_TargetNow.color = targetSame;
        }

        if (Box_RollMod)
        {
            string sign = rollMod >= 0 ? "+" : "?";
            Box_RollMod.text = $"Roll Modifiers: {sign}{Mathf.Abs(rollMod)}";
        }
    }

    private void RollAndResolve()
    {
        // lock the roll button to avoid double clicks
        if (Btn_RollDie) Btn_RollDie.interactable = false;

        int target = currentCard.baseTarget;
        int rollMod = 0;

        foreach (var slot in slots)
        {
            if (slot == null || !slot.gameObject.activeSelf || slot.Option == null) continue;
            if (!slot.Contributes) continue;
            target += slot.Option.targetShift;
            rollMod += slot.Option.rollBonus;
        }

        int die = Random.Range(1, 7);
        int total = die + rollMod;
        bool success = total >= target;

        if (Box_RollResult)
        {
            string modStr = rollMod >= 0 ? $"+ Mod(+{rollMod})" : $"+ Mod({rollMod})";
            Box_RollResult.text = $"Result: {die} {modStr} = {total} ? {(success ? "SUCCESS" : "FAIL")}";
        }

        // Optional: build log entry for listeners
        var entry = new TurnLogEntry
        {
            turnIndex = currentTurnIndex,
            goal = currentCard.goal,
            cardTitle = currentCard.cardTitle,
            baseTarget = currentCard.baseTarget,
            adjustedTarget = target,
            die = die,
            rollMod = rollMod,
            success = success
        };
        foreach (var slot in slots)
        {
            if (slot == null || slot.Option == null || !slot.gameObject.activeSelf) continue;
            entry.options.Add(new LoggedOption
            {
                text = slot.Option.text,
                kind = slot.Option.kind,
                hadBackground = slot.HasBackground,
                lockedIn = slot.Contributes,
                targetShift = slot.Option.targetShift,
                rollBonus = slot.Option.rollBonus
            });
        }
        OnResolved?.Invoke(entry);

        foreach (var slot in slots)
            if (slot != null) slot.SetInteractable(false);

        // 5s outcome message ? 3s countdown ? signal GameFlow to return
        StartCoroutine(OutcomeSequence(success));
    }

    private System.Collections.IEnumerator OutcomeSequence(bool success)
    {
        // Show "You succeeded!" / "You failed!" in the card body
        if (DialogueCardBody_Text)
        {
            DialogueCardBody_Text.color = success ? successGreen : failRed;
            DialogueCardBody_Text.text = success ? "You succeeded!" : "You failed!";
        }

        yield return new WaitForSecondsRealtime(5f);

        // 3-second countdown
        for (int i = 3; i >= 1; i--)
        {
            if (DialogueCardBody_Text)
            {
                DialogueCardBody_Text.color = targetSame;
                DialogueCardBody_Text.text = $"Returning in {i}…";
            }
            yield return new WaitForSecondsRealtime(1f);
        }

        // Signal GameFlow to return to selection; pass which card & outcome
        if (OnCardFinished != null)
            OnCardFinished(currentCard, success);
    }
}
