using UnityEngine;

public interface IBaseSkill
{
    void TickCooldown(float deltaTime);
    SkillDataSO GetData();
    SkillRuntimeData GetRuntimeData();
    bool CanCast();
    bool IsOnCooldown();
}

public abstract class BaseSkill<TData, TRuntime> : IBaseSkill
    where TData : SkillDataSO
    where TRuntime : SkillRuntimeData
{
    protected TData Data { get; }

    protected TRuntime RuntimeData { get; }

    protected BaseSkill(TData data, TRuntime runtimeData)
    {
        Data = data;
        RuntimeData = runtimeData;
    }

    public virtual bool IsOnCooldown()
    {
        return RuntimeData.remainingCooldownTicks > 0;
    }

    public virtual bool CanAfford()
    {
        return PointsManager.Instance.HaveEnoughInfectionPoints(Data.infectionPointCost);
    }

    public virtual bool CanCast()
    {
        GameState gameState = GameManager.Instance.GetGameState();
        if (gameState == GameState.NotStarted || gameState == GameState.Ended)
            return false;

        if (IsOnCooldown())
        {
            UIManager.Instance.ShowNotification("Skill isn't ready!", 2f, Color.red);
            return false;
        }

        if (!CanAfford())
        {
            UIManager.Instance.ShowNotification("Not enough Infection Point!", 2f, Color.red);
            return false;
        }

        return true;
    }

    public virtual void StartCooldown()
    {
        RuntimeData.remainingCooldownTicks = Data.cooldownTicks;
    }

    public virtual void TickCooldown(float deltaTime)
    {
        if (RuntimeData.remainingCooldownTicks > 0)
        {
            RuntimeData.remainingCooldownTicks -= deltaTime;

            if (RuntimeData.remainingCooldownTicks < 0)
                RuntimeData.remainingCooldownTicks = 0;
        }
    }

    public SkillRuntimeData GetRuntimeData()
    {
        return RuntimeData;
    }

    public SkillDataSO GetData()
    {
        return Data;
    }
}
