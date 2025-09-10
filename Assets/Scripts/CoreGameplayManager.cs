using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnequalOdds.GameData;
using UnequalOdds.Gameplay;
using UnequalOdds.Runtime;

public class CoreGameplayManager : MonoBehaviour
{
    [Header("Dialogue sources")]
    [SerializeField] private List<DialogueCard> allCards = new List<DialogueCard>();

    [Header("UI refs")]
    [SerializeField] private TMP_Text headerGoal;
    [SerializeField] private TMP_Text cardTitle;
    [SerializeField] private TMP_Text cardBody;
    [SerializeField] private TMP_Text baseTargetText;
    [SerializeField] private TMP_Text adjustedTargetText;
    [SerializeField] private TMP_Text netRollModText;
    [SerializeField] private Button rollButton;
    [SerializeField] private Transform optionsParent; // container for option rows
    [SerializeField] private OptionRowUI optionRowPrefab;
    [SerializeField] private GameObject goalSelectPanel; // your existing panel
    [SerializeField] private GameObject dialoguePanel;   // panel with card/opts

    [Header("Run settings")]
    [SerializeField] private int turnsPerRun = 10;

    private PlayerProfile profile;
    private int turnIndex;
    private DialogueCard currentCard;
    private readonly List<OptionRowUI> optionRows = new List<OptionRowUI>();

    // basic turn log
    private readonly List<TurnLogEntry> runLog = new List<TurnLogEntry>();

    private void Start()
    {
        profile = GameState.Instance.CurrentProfile;
        turnIndex = 0;
        ShowGoalSelect();
    }

    private void ShowGoalSelect()
    {
        goalSelectPanel.SetActive(true);
        dialoguePanel.SetActive(false);
        rollButton.onClick.RemoveAllListeners();
    }

    // Wire these from your goal buttons (e.g., OnClick in Inspector)
    public void OnSelectGoal(int goalIndex)
    {
        LifeGoal goal = (LifeGoal)goalIndex;
        currentCard = DrawRandomCard(goal);
        RenderCard(goal, currentCard);
    }

    private DialogueCard DrawRandomCard(LifeGoal goal)
    {
        var pool = allCards.FindAll(c => c.goal == goal);
        if (pool.Count == 0) { Debug.LogWarning($"No cards for {goal}"); return null; }
        int idx = Random.Range(0, pool.Count);
        return pool[idx];
    }

    private void RenderCard(LifeGoal goal, DialogueCard card)
    {
        goalSelectPanel.SetActive(false);
        dialoguePanel.SetActive(true);

        headerGoal.text = goal.ToString();
        cardTitle.text = card.cardTitle;
        cardBody.text = card.cardBody;
        baseTargetText.text = $"Need ? {card.baseTarget}";

        // clear old rows
        foreach (var row in optionRows) Destroy(row.gameObject);
        optionRows.Clear();

        foreach (var opt in card.options)
        {
            var row = Instantiate(optionRowPrefab, optionsParent);
            bool unlocked = opt.gate == null || opt.gate.Evaluate(profile);
            row.Setup(opt, unlocked);
            row.OnSelectionChanged += RecomputeSummary;
            optionRows.Add(row);
        }

        RecomputeSummary();
        rollButton.onClick.RemoveAllListeners();
        rollButton.onClick.AddListener(RollAndResolve);
    }

    private void RecomputeSummary()
    {
        int target = currentCard.baseTarget;
        int rollMod = 0;

        foreach (var row in optionRows)
        {
            if (row.IsSelected)
            {
                target += row.Option.targetShift;
                rollMod += row.Option.rollBonus;
            }
            else if (row.Option.forcedDrawback)
            {
                // forced drawbacks are auto-selected inside OptionRowUI
                target += row.Option.targetShift;
                rollMod += row.Option.rollBonus;
            }
        }

        adjustedTargetText.text = $"Adjusted target: ? {target}";
        netRollModText.text = $"Net roll modifier: {(rollMod >= 0 ? "+" : "")}{rollMod}";
    }

    private void RollAndResolve()
    {
        int target = currentCard.baseTarget;
        int rollMod = 0;
        var chosen = new List<string>();

        foreach (var row in optionRows)
        {
            if (row.IsSelected)
            {
                target += row.Option.targetShift;
                rollMod += row.Option.rollBonus;
                chosen.Add(row.Option.text);
            }
            else if (row.Option.forcedDrawback)
            {
                target += row.Option.targetShift;
                rollMod += row.Option.rollBonus;
                chosen.Add($"[Forced] {row.Option.text}");
            }
        }

        int die = Random.Range(1, 7);
        int total = die + rollMod;
        bool success = total >= target;

        // log
        runLog.Add(new TurnLogEntry
        {
            turn = turnIndex,
            goal = currentCard.goal,
            cardTitle = currentCard.cardTitle,
            baseTarget = currentCard.baseTarget,
            adjustedTarget = target,
            roll = die,
            rollMod = rollMod,
            success = success,
            chosenOptions = chosen
        });

        // TODO: apply outcomes (attribute changes, future flags, etc.)

        // advance
        turnIndex++;
        if (turnIndex >= turnsPerRun)
        {
            // TODO: load Debrief scene, pass runLog (via GameState or serialize)
            ShowGoalSelect(); // placeholder
        }
        else
        {
            ShowGoalSelect();
        }
    }

    // Simple struct for logging
    private struct TurnLogEntry
    {
        public int turn;
        public LifeGoal goal;
        public string cardTitle;
        public int baseTarget;
        public int adjustedTarget;
        public int roll;
        public int rollMod;
        public bool success;
        public List<string> chosenOptions;
    }
}
