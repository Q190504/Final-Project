using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HumanAIManager : MonoBehaviour
{
    public static HumanAIManager Instance;

    [Header("Actions")]
    [SerializeField] private List<HumanActionSO> humanActionDatas;

    [Header("Threat Weights")]
    [Range(0f, 1f), Tooltip("Multiplier for the infection rate detected. This and DeadRateDetectedWeight should sum up to 1.")]
    public float InfectionRateDetectedWeight;
    [Range(0f, 1f), Tooltip("Multiplier for the dead rate detected. This and InfectionRateDetectedWeight should sum up to 1.")]
    public float DeadRateDetectedWeight;

    [Header("Threat Multipliers")]
    public float MaxInfectionPressureMultiplier = 1.5f;
    public float MaxSkillMultiplier = 1.3f;

    [Header("Skill Threat")]
    public float MaxSkillUseThreat = 10f;

    [Header("AI Context Config")]
    public AIContextConfigSO aiContextConfigSO;

    [Header("Settings")]
    public ThreatTier ThreatTierWhenHavingReducedCooldown;
    [Range(0f, 1f), Tooltip("Multiplier for the action's reduced cooldown time when at the final threat tier.")]
    public float ReducedCooldownModifier;

    public AIContext currentContext;

    private Dictionary<HumanActionType, HumanAction> humanActionDataDict;

    private AIContextBuilder contextBuilder;
    private TimeManager timeManager;
    private MapManager mapManager;
    private UIManager uiManager;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void Tick(AIContext ctx)
    {
        mapManager.UpdateMap(ctx);
        contextBuilder.Build(ctx, Mathf.FloorToInt(timeManager.CurrentTick));

        ThreatTierSO currentThreatTier = PropertyDataManager.Instance.GetThreatTierData(ctx.ThreatTier);
        UpdateUI(currentThreatTier);
        UpdateActionsCooldownAndActiveStatus(currentThreatTier);

        int maxActions = currentThreatTier.maxActionsPerTick;

        List<ActionInstance> selectedActions = SelectActions(ctx, maxActions, currentThreatTier);

        Execute(selectedActions, ctx);

        ScheduleNextReaction(1f);
    }

    public List<ActionInstance> SelectActions(AIContext ctx, int actionCount, ThreatTierSO tier)
    {
        List<ActionInstance> result = new();
        List<ActionInstance> actionCandidates = new();

        List<HumanAction> humanActions = tier.availableActions.Select(type => humanActionDataDict[type]).ToList();
        List<HumanAction> actionsToConsider = humanActions
            .OrderByDescending(a => a.Data.priority)
            .ToList();

        HashSet<HumanAction> chosenActions = new();

        SimulationCache simCache = new();
        AIContext simCtx = ctx.Clone();

        // 1. Build first round candidates
        foreach (HumanAction action in actionsToConsider)
        {
            if (!action.CanExecute(simCtx))
                continue;

            var instance = action.BuildBestInstances(simCtx, simCache);

            if (instance != null && instance.IsValid)
            {
                actionCandidates.Add(instance);
            }
        }

        // 2. Select actions iteratively, after each selection, apply light simulation and rebuild candidates if needed
        for (int i = 0; i < actionCount; i++)
        {
            if (actionCandidates.Count == 0)
                break;

            // Re-score with new simCache
            foreach (ActionInstance inst in actionCandidates)
            {
                float strategic = StrategicWeight(inst.Action.Data.ActionType, ctx.ThreatLevel);
                float penalty = ComputePenalty(inst, simCache);

                inst.FinalScore = inst.NormalizedUtility * strategic * penalty;
            }

            Debug.Log($"Has {actionCandidates.Count} candidates:\n" +
    string.Join("\n", actionCandidates.Select(a => $"{a.Action.Data.actionName} - RawUtility: {a.RawUtility}, NormalizedUtility {a.NormalizedUtility}, FinalScore {a.FinalScore}")));

            // Pick best
            ActionInstance best = actionCandidates
                .OrderByDescending(x => x.FinalScore)
                .FirstOrDefault();

            if (best == null)
                break;

            result.Add(best);
            // remove all instances of the selected action
            actionCandidates.RemoveAll(x => x.Action == best.Action);
            chosenActions.Add(best.Action);

            // Apply simulation
            best.Action.ApplyLightSimulation(best.Cells, simCtx, simCache);

            if (i == actionCount - 1)
                break;

            // 3. Rebuild candidates for actions that are affected by the previously selected action
            foreach (HumanAction action in actionsToConsider)
            {
                if (chosenActions.Contains(action))
                    continue;

                if (!action.CanExecute(simCtx))
                    continue;

                // remove all old instances of this action
                actionCandidates.RemoveAll(x => x.Action == action);

                ActionInstance newInstance = action.BuildBestInstances(
                  simCtx,
                  simCache
                );

                // add new
                if (newInstance != null && newInstance.IsValid)
                {
                    actionCandidates.Add(newInstance);
                }
            }
        }

        if (result.Count > 0)
            Debug.Log($"Choose {result.Count} action(s):\n" +
    string.Join("\n", result.Select(r => $"{r.Action.Data.actionName} - RawUtility: {r.RawUtility}, NormalizedUtility {r.NormalizedUtility}, FinalScore {r.FinalScore}")));

        return result;
    }

    private float ComputePenalty(ActionInstance inst, SimulationCache cache)
    {
        float penalty = 1f;

        if (inst.Action.Data.ActionType == HumanActionType.Lockdown)
        {
            foreach (var cell in inst.Cells)
            {
                if (cache.IsLockdowned(cell))
                    return 0f;
            }
        }
        else if (inst.Action.Data.ActionType == HumanActionType.BuildHospital
            || inst.Action.Data.ActionType == HumanActionType.BuildVaccineResearchCenter)
        {
            foreach (var cell in inst.Cells)
            {
                if (cache.HasBuiltStructure(cell))
                    return 0f;
            }
        }
        else if (inst.Action.Data.ActionType == HumanActionType.DevelopVaccine)
        {
            return penalty;
        }
        else if (inst.Action.Data.ActionType == HumanActionType.Sterilize)
        {
            foreach (var cell in inst.Cells)
            {
                return Mathf.Min(penalty, cache.GetPenalty(cell));
            }
        }

        return penalty;
    }

    private float StrategicWeight(HumanActionType type, float threat)
    {
        return type switch
        {
            HumanActionType.Sterilize => Mathf.Lerp(aiContextConfigSO.SterilizeStrategicWeight.x,
            aiContextConfigSO.SterilizeStrategicWeight.y, threat),
            HumanActionType.Lockdown => Mathf.Lerp(aiContextConfigSO.LockdownStrategicWeight.x,
            aiContextConfigSO.LockdownStrategicWeight.y, threat),
            HumanActionType.BuildHospital => Mathf.Lerp(aiContextConfigSO.BuildHospitalStrategicWeight.x,
            aiContextConfigSO.BuildHospitalStrategicWeight.y, threat),
            HumanActionType.BuildVaccineResearchCenter => Mathf.Lerp(aiContextConfigSO.BuildVaccineResearchCenterStrategicWeight.x,
            aiContextConfigSO.BuildVaccineResearchCenterStrategicWeight.y, threat),
            HumanActionType.DevelopVaccine => Mathf.Lerp(aiContextConfigSO.DevelopVaccineStrategicWeight.x,
            aiContextConfigSO.DevelopVaccineStrategicWeight.y, threat),
            _ => 1f,
        };
    }

    private void Execute(List<ActionInstance> selected, AIContext ctx)
    {
        List<HumanActionType> selectedActionTypes = new List<HumanActionType>();

        foreach (ActionInstance c in selected)
        {
            c.Action.Execute(c.Cells, ctx, ThreatTierWhenHavingReducedCooldown, ReducedCooldownModifier);
            selectedActionTypes.Add(c.Action.Data.ActionType);
        }

        UIManager.Instance.SetExecutedHumanActionsCooldownUI(selectedActionTypes);
    }

    private void UpdateActionsCooldownAndActiveStatus(ThreatTierSO tier)
    {
        List<HumanActionType> availableActionsInThisTier = tier.availableActions;

        foreach (HumanAction action in humanActionDataDict.Values.ToList())
        {
            action.UpdateCooldown();

            if (availableActionsInThisTier.Contains(action.Data.ActionType))
                action.SetIsActived(true);
            else
                action.SetIsActived(false);
        }

        UIManager.Instance.UpdateHumanActionsCooldownUI(humanActionDataDict.Values.ToList());
    }

    public void Init()
    {
        timeManager = TimeManager.Instance;
        mapManager = MapManager.Instance;
        uiManager = UIManager.Instance;
        contextBuilder = new AIContextBuilder(mapManager.GetGrid(), aiContextConfigSO);

        CreateActions();
    }

    public void CreateActions()
    {
        if (humanActionDataDict == null)
            humanActionDataDict = new Dictionary<HumanActionType, HumanAction>();

        foreach (HumanActionSO data in humanActionDatas)
        {
            humanActionDataDict[data.ActionType] = data.CreateLogic();
        }
    }

    private void ScheduleNextReaction(float delayTicks)
    {
        timeManager.ScheduleEvent(delayTicks, () =>
        {
            AIContext ctx = new();
            currentContext = ctx;
            Tick(ctx);
        }, EventPriority.HumanAction);
    }

    public void StartMatch()
    {
        ScheduleNextReaction(0f);
    }

    private void UpdateUI(ThreatTierSO currentThreatTier)
    {
        uiManager.UpdateThreatTier(currentThreatTier);
        uiManager.ShowHumanActionsOfTheCurrentThreatTier(currentThreatTier);
    }

    private void OnValidate()
    {
        float multipilers = InfectionRateDetectedWeight + DeadRateDetectedWeight;

        if (Mathf.Abs(multipilers - 1f) > 0.0001f)
            Debug.LogWarning($"Multipliers for threatLevel should sum to 1. Current sum = {multipilers}");
    }
}
