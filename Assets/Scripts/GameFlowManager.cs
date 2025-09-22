using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnequalOdds.GameData;

public class GameFlowManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject Panel_LifeGoalSelection;
    [SerializeField] private GameObject Panel_DialogueInteraction;

    [Header("Controllers")]
    [SerializeField] private DialogueInteractionController_Static dialogueCtrl;

    [Header("Life Goal Buttons")]
    [SerializeField] private Button Btn_LivableJob;
    [SerializeField] private Button Btn_SafeHousing;
    [SerializeField] private Button Btn_MaintainHealth;
    [SerializeField] private Button Btn_FinancialCushion;
    [SerializeField] private Button Btn_CivicVoice;

    [Header("Cards (master list)")]
    [SerializeField] private List<DialogueCard> allCards = new List<DialogueCard>();

    [Header("Turns")]
    [SerializeField] private int turnsPerRun = 10;
    private int turnIndex = 0;

    private readonly Dictionary<LifeGoal, List<DialogueCard>> remainingByGoal =
        new Dictionary<LifeGoal, List<DialogueCard>>();

    private void Awake()
    {
        BuildPools();

        // Wire button clicks
        if (Btn_LivableJob) Btn_LivableJob.onClick.AddListener(() => StartTurn(LifeGoal.LivableJob));
        if (Btn_SafeHousing) Btn_SafeHousing.onClick.AddListener(() => StartTurn(LifeGoal.SafeHousing));
        if (Btn_MaintainHealth) Btn_MaintainHealth.onClick.AddListener(() => StartTurn(LifeGoal.MaintainHealth));
        if (Btn_FinancialCushion) Btn_FinancialCushion.onClick.AddListener(() => StartTurn(LifeGoal.FinancialCushion));
        if (Btn_CivicVoice) Btn_CivicVoice.onClick.AddListener(() => StartTurn(LifeGoal.CivicVoice));
    }

    private void OnEnable()
    {
        if (dialogueCtrl != null)
            dialogueCtrl.OnCardFinished += HandleCardFinished;
    }

    private void OnDisable()
    {
        if (dialogueCtrl != null)
            dialogueCtrl.OnCardFinished -= HandleCardFinished;
    }

    private void Start()
    {
        ShowSelection();
        UpdateGoalButtons();
    }

    // Build per-goal pools from the master list
    private void BuildPools()
    {
        remainingByGoal.Clear();
        remainingByGoal[LifeGoal.LivableJob] = new List<DialogueCard>();
        remainingByGoal[LifeGoal.SafeHousing] = new List<DialogueCard>();
        remainingByGoal[LifeGoal.MaintainHealth] = new List<DialogueCard>();
        remainingByGoal[LifeGoal.FinancialCushion] = new List<DialogueCard>();
        remainingByGoal[LifeGoal.CivicVoice] = new List<DialogueCard>();

        foreach (var card in allCards)
        {
            if (!remainingByGoal.ContainsKey(card.goal))
                remainingByGoal[card.goal] = new List<DialogueCard>();
            remainingByGoal[card.goal].Add(card);
        }
    }

    private void ShowSelection()
    {
        if (Panel_LifeGoalSelection) Panel_LifeGoalSelection.SetActive(true);
        if (Panel_DialogueInteraction) Panel_DialogueInteraction.SetActive(false);
    }

    private void ShowDialogue()
    {
        if (Panel_LifeGoalSelection) Panel_LifeGoalSelection.SetActive(false);
        if (Panel_DialogueInteraction) Panel_DialogueInteraction.SetActive(true);
    }

    private void StartTurn(LifeGoal goal)
    {
        // If this goal is already cleared (no remaining cards), ignore (button should be disabled anyway)
        if (!remainingByGoal.ContainsKey(goal) || remainingByGoal[goal].Count == 0)
            return;

        ShowDialogue();

        // Give the controller the current pool only for this goal
        dialogueCtrl.SetCardPool(remainingByGoal[goal]);
        dialogueCtrl.PresentRandomCard(goal, turnIndex, turnsPerRun);
    }

    private void HandleCardFinished(DialogueCard card, bool success)
    {
        // On success: remove this card from the pool so it won't show up again
        if (success && card != null)
        {
            var pool = remainingByGoal[card.goal];
            pool.Remove(card);
        }

        // Update goal buttons—grey out if a goal has no cards left
        UpdateGoalButtons();

        // Advance turn counter (optional end-of-run handling could go here)
        turnIndex++;
        ShowSelection();
    }

    private void UpdateGoalButtons()
    {
        SetGoalButton(Btn_LivableJob, LifeGoal.LivableJob);
        SetGoalButton(Btn_SafeHousing, LifeGoal.SafeHousing);
        SetGoalButton(Btn_MaintainHealth, LifeGoal.MaintainHealth);
        SetGoalButton(Btn_FinancialCushion, LifeGoal.FinancialCushion);
        SetGoalButton(Btn_CivicVoice, LifeGoal.CivicVoice);
    }

    private void SetGoalButton(Button btn, LifeGoal goal)
    {
        if (!btn) return;

        bool hasRemaining = remainingByGoal.ContainsKey(goal) && remainingByGoal[goal].Count > 0;
        btn.interactable = hasRemaining;

        // Grey-out visual via CanvasGroup (add one to each button in the Inspector), else fallback alpha
        var cg = btn.GetComponent<CanvasGroup>();
        if (cg != null)
            cg.alpha = hasRemaining ? 1f : 0.5f;
        else
            btn.image.color = hasRemaining ? Color.white : new Color(1f, 1f, 1f, 0.5f);
    }
}
