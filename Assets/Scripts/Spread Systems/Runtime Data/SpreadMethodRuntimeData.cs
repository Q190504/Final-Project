using System.Collections.Generic;
using UnityEngine;

public class SpreadMethodRuntimeData
{
    // ===== Base Runtime Stats =====
    public SpreadMethodType methodType;

    public int baseInfectionPower = 0;
    public int additiveInfectionPower = 0;
    public float multiplicativeInfectionPower = 1f;

    public int baseSterilizationResistance = 0;
    public int additiveSterilizationResistance = 0;
    public float multiplicativeSterilizationResistance = 1f;
    public List<SterilizationResistanceModifier> sterilizationResistanceModifiers = new();
    public SterilizationResistanceModifier methodSterilizationResistanceModifier;

    public float baseSpreadChanceWhenBlocked = 0f;
    public float spreadChanceWhenBlockedModifier = 0f;

    public int baseMinDistance = 0;
    public int baseMaxDistance = 0;
    public int bonusMinDistance = 0;
    public int bonusMaxDistance = 0;

    public float remainingTicksToSpread = 0f;
    public float baseTickToSpread = 0f;
    public float tickToSpreadDecreasive = 0f;
    public float tickToSpreadMultiplier = 1f;

    public EnvironmentModifierTable environmentModifiers;
    public TemperatureModifierTable temperatureModifiers;
    public PopulationModifierTable populationModifiers;

    // ===== Final Calculated Stats =====

    public virtual int GetFinalInfectionPower(GridCell originCell, GridCell targetCell)
    {
        CellStats originStats = originCell.Stats;

        float addition = baseInfectionPower + additiveInfectionPower;

        float multiplier = multiplicativeInfectionPower;

        if (environmentModifiers != null)
            multiplier *= environmentModifiers.GetModifier(originStats.environment.currentEnvironmentType);

        if (temperatureModifiers != null)
            multiplier *= temperatureModifiers.GetModifier(originStats.tempurature.type);

        if (populationModifiers != null)
            multiplier *= populationModifiers.GetModifier(originStats.population.type);

        multiplier *= 1 + originStats.bonusTargetInfectionGainPercent;

        float valueFloat = Mathf.Clamp(addition * multiplier, Utility.minInfectionLevel, Utility.maxInfectionLevel);
        int valueInt = Mathf.FloorToInt(valueFloat);

        return valueInt;
    }

    public int GetFinalSterilizationResistance()
    {
        float valueFloat = baseSterilizationResistance + additiveSterilizationResistance;
        float mutiplier = multiplicativeSterilizationResistance;

        foreach (SterilizationResistanceModifier sterilizationResistanceModifier in sterilizationResistanceModifiers)
        {
            if (sterilizationResistanceModifier.ModifierType == ModifierType.Additive)
                valueFloat += sterilizationResistanceModifier.Value;
            else
                valueFloat += sterilizationResistanceModifier.Value;
        }

        int valueInt = Mathf.FloorToInt(valueFloat * mutiplier);
        valueInt = Mathf.Clamp(valueInt, Utility.minSterilizationResistance, Utility.maxSterilizationResistance);
        SterilizationResistanceAdditiveSourceType source = methodType switch
        {
            SpreadMethodType.Surface => SterilizationResistanceAdditiveSourceType.Surface,
            SpreadMethodType.Air => SterilizationResistanceAdditiveSourceType.Air,
            SpreadMethodType.Water => SterilizationResistanceAdditiveSourceType.Water,
            SpreadMethodType.Carrier => SterilizationResistanceAdditiveSourceType.Carrier,
            _ => SterilizationResistanceAdditiveSourceType.Unkown,
        };

        if (methodSterilizationResistanceModifier == null)
        {
            methodSterilizationResistanceModifier = new(valueInt, source, ModifierType.Additive);
        }
        else
        {
            methodSterilizationResistanceModifier.Value = valueInt;
        }

        return valueInt;
    }

    public float GetFinalSpreadChanceWhenBlocked()
    {
        float value = baseSpreadChanceWhenBlocked + spreadChanceWhenBlockedModifier;
        value = Mathf.Clamp01(value);

        return value;
    }

    public int GetFinalMinDistance()
    {
        return baseMinDistance + bonusMinDistance;
    }

    public int GetFinalMaxDistance()
    {
        return baseMaxDistance + bonusMaxDistance;
    }

    public float GetFinalTickToSpread()
    {
        return CalculateFinalTickToSpread();
    }

    public void DecreaseTickToSpreadPercent(float decreasePercent)
    {
        if (decreasePercent <= 0)
            return;

        float oldDuration = CalculateFinalTickToSpread();

        tickToSpreadDecreasive += baseTickToSpread * decreasePercent;

        float newDuration = CalculateFinalTickToSpread();

        ScaleRemainingTime(oldDuration, newDuration);
    }

    public void IncreaseTickToSpreadPercent(float increasePercent)
    {
        if (increasePercent <= 0)
            return;

        float oldDuration = CalculateFinalTickToSpread();

        tickToSpreadDecreasive -= baseTickToSpread * increasePercent;

        float newDuration = CalculateFinalTickToSpread();

        ScaleRemainingTime(oldDuration, newDuration);
    }

    private float CalculateFinalTickToSpread()
    {
        float value = baseTickToSpread;

        value -= tickToSpreadDecreasive;

        value *= tickToSpreadMultiplier;

        return Mathf.Max(0f, value);
    }

    private void ScaleRemainingTime(float oldDuration, float newDuration)
    {
        if (oldDuration <= 0)
            return;

        remainingTicksToSpread *= newDuration / oldDuration;
    }

    public SpreadMethodRuntimeData(SpreadMethodDataSO methodDataSO)
    {
        baseInfectionPower = methodDataSO.baseConfig.baseInfectionPower;
        additiveInfectionPower = 0;
        multiplicativeInfectionPower = 1;

        baseSterilizationResistance = methodDataSO.baseConfig.baseSterilizationResistance;
        additiveSterilizationResistance = 0;
        multiplicativeSterilizationResistance = 1;

        baseSpreadChanceWhenBlocked = methodDataSO.baseConfig.spreadChanceWhenBlocked;
        spreadChanceWhenBlockedModifier = 0f;

        baseMinDistance = methodDataSO.baseConfig.minDistance;
        baseMaxDistance = methodDataSO.baseConfig.maxDistance;
        bonusMinDistance = 0;
        bonusMaxDistance = 0;

        baseTickToSpread = methodDataSO.baseConfig.tickToSpread;
        tickToSpreadDecreasive = 0;
        tickToSpreadMultiplier = 1f;
        remainingTicksToSpread = GetFinalTickToSpread();

        InitModifiersTables(methodDataSO.baseConfig);
    }

    public void InitModifiersTables(SpreadMethodConfig config)
    {
        if (config.environmentModifiers != null)
            environmentModifiers = config.environmentModifiers.Clone();
        if (config.temperatureModifiers != null)
            temperatureModifiers = config.temperatureModifiers.Clone();
        if (config.populationModifiers != null)
            populationModifiers = config.populationModifiers.Clone();

        environmentModifiers.Initialize();
        temperatureModifiers.Initialize();
        populationModifiers.Initialize();
    }

    #region Getters for UI

    public int GetBaseInfectionPower()
    {
        float addition = baseInfectionPower + additiveInfectionPower;

        float multiplier = multiplicativeInfectionPower;

        float valueFloat = Mathf.Clamp(addition * multiplier, Utility.minInfectionLevel, Utility.maxInfectionLevel);
        int valueInt = Mathf.FloorToInt(valueFloat);

        return valueInt;
    }

    public List<TemperatureModifierEntry> GetTemperatureModifiers()
    {
        return temperatureModifiers.GetEntries();
    }

    public List<PopulationModifierEntry> GetPopulationModifiers()
    {
        return populationModifiers.GetEntries();
    }

    #endregion
}