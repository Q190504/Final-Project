using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class HumanAction
{
    protected HumanActionSO data;
    protected bool isActived;
    protected int cooldownTicks = 0;

    public HumanActionSO Data => data;
    public int CooldownTicks => cooldownTicks;
    public bool IsActived => isActived;

    public static event Action<HumanActionExecutionVisualData> OnExecutedVisual;

    protected TimeManager timeManager;

    protected HumanAction(HumanActionSO data)
    {
        this.data = data;
        timeManager = TimeManager.Instance;
        cooldownTicks = 0;
    }

    public abstract float EvaluateCell(GridCell cell, AIContext ctx);

    public abstract float CalculateRawUtility(List<GridCell> targets, AIContext ctx);

    public virtual float NormalizeUtility(float rawUtility)
    {
        if (data.maxUtility - data.minUtility == 0)
            return 0f;
        return Mathf.Clamp01((rawUtility - data.minUtility) / (data.maxUtility - data.minUtility));
    }

    public abstract ActionInstance BuildBestInstances(AIContext ctx, SimulationCache simCache);

    // for estimate effect
    public abstract void ApplyLightSimulation(List<GridCell> targets, AIContext ctx, SimulationCache simCache);

    public virtual void Execute(List<GridCell> targets, AIContext ctx, ThreatTier reducedCooldownTier, float reducedCooldownModifier)
    {
        float dynamicCooldown = data.cooldownTicks;

        if (ctx.ThreatTier == reducedCooldownTier)
            dynamicCooldown = data.cooldownTicks * reducedCooldownModifier;

        int minCooldown = Mathf.Max(1, Mathf.FloorToInt(dynamicCooldown));

        cooldownTicks = minCooldown;

        if (targets != null && targets.Count > 0)
        {
            Debug.Log(
                $"Executed {data.ActionType} on {targets.Count} cell(s):\n" +
                string.Join("\n", targets.Select(c => $"Cell ({c.X}, {c.Y})"))
            );
        }
        else
        {
            Debug.Log($"Executed {data.ActionType}");
        }
    }

    protected void RaiseExecuteVisual(
        List<GridCell> cells,
        HumanActionType actionType)
    {
        OnExecutedVisual?.Invoke(
            new HumanActionExecutionVisualData(
                cells,
                actionType
            )
        );
    }

    public void UpdateCooldown()
    {
        cooldownTicks--;

        if (cooldownTicks < 0)
            cooldownTicks = 0;
    }

    public void SetIsActived(bool isActived)
    {
        this.isActived = isActived;
    }

    public virtual bool CanExecute(AIContext ctx)
    {
        return cooldownTicks == 0;
    }
}