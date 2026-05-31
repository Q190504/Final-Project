using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CellStats
{
    public GridCell parentCell;

    public CellPopulation population;
    public CellTempurature tempurature;
    public CellEnvironment environment;
    public CellStructure structure;
    public List<StructureType> affectedByStructures;

    public CellStage stage;
    public int infectionLevel = 0;

    public int baseInfectionResistance = 0;
    public float infectionResistanceMultiplier = 1f;
    public int finalInfectionResistance = 0;
    private List<InfectionResistanceModifier> infectionResistanceModifiers = new();

    public int baseSterilizationResistance = 0;
    public float baseSerilizationResistanceMultiplier = 1f;
    public int finalSterilizationResistance = 0;
    private List<SterilizationResistanceModifier> sterilizationResistanceModifiers = new();

    public float bonusTargetInfectionGainPercent = 0;

    public bool isDetected = false;
    public float baseDetection = 0;
    public float detectionMultiplier = 1f;
    public float finalDetection = 0;
    private List<CellDetectionModifier> detectionAdditiveModifiers = new();

    public bool isContagious = false; // whether the cell can spread infection to other cells
    public bool isBlocked = false; // whether the cell can't be infected by other cells
    public bool isLockdown = false; // whether the cell can't spread infection to other cells and can't be infected by other cells
    public float lockdownRemainingTicks = 0;
    private InfectionResistanceModifier lockdownInfectionResistanceModifier = null;

    public bool canBuildStructure = true;
    public bool canHasCarrier = true;
    public bool hasCarrier = false;
    public float additionalCarrierSpreadChancePercent = 0;

    public bool canSwitchToDead = false;
    public float toDeadTicksCount = 0;

    public bool canBeSterilized = true;
    public int sterilizationImmunityTicks = 0;

    public float priorityToHuman;
    public PriorityToMethods priorityToMethods;

    private int tickInfectedCount = 0;

    #region Surface's upgrade

    private int minTickToBonusSterilizationResistance = -1;
    private SterilizationResistanceModifier sterilizationResistanceModifierIfInfectedForALongTime = null;

    #endregion

    #region Water's upgrade

    private SterilizationResistanceModifier sterilizationResistanceModifierForWaterCell = null;

    #endregion

    private MapManager mapManager;
    private Grid<GridCell> grid;
    private CellDetectionModifier lockdownDetectionModifier = null;

    public CellStats()
    {
        parentCell = null;
        this.population = new CellPopulation();
        this.tempurature = new CellTempurature();
        this.environment = new CellEnvironment();
        this.structure = new CellStructure();
        this.stage = new CellStage();
        affectedByStructures = new List<StructureType>();
        priorityToMethods = new PriorityToMethods();
    }

    public void SetStats(PopulationType population, TemperatureType tempurature, EnvironmentType environment, GridCell cell)
    {
        parentCell = cell;

        affectedByStructures = new List<StructureType>();
        priorityToMethods = new PriorityToMethods();

        this.population.SetPopulation(population);
        this.tempurature.SetTempurature(tempurature);
        this.environment.SetEnvironmentType(environment);

        if (environment == EnvironmentType.Water)
        {
            this.canBuildStructure = false;
            this.canHasCarrier = false;
        }
        else if (environment == EnvironmentType.Mountain)
        {
            this.isBlocked = true;
            this.isContagious = false;
            this.canBuildStructure = false;
            this.canHasCarrier = false;
        }

        UpdateHumanPriority();
        mapManager = MapManager.Instance;
        grid = mapManager.GetGrid();
    }

    public void SetStats(float populationValue, float tempuratureValue, GridCell cell)
    {
        parentCell = cell;
        this.population.SetPopulation(populationValue);
        this.tempurature.SetTempurature(tempuratureValue);
        this.environment.SetEnvironmentType(population.type, tempurature.type);
        UpdateHumanPriority();
        mapManager = MapManager.Instance;
        grid = mapManager.GetGrid();
    }

    public void SetStructure(StructureType structure, GridCell cell)
    {
        this.structure.SetStructure(structure, cell);
        UpdateHumanPriority();
        parentCell.CheckIsBeingShownInfo();
        mapManager.AddCellNeedToUpdateVisual(new Vector2Int(cell.X, cell.Y));
    }

    public void SetTempurature(TemperatureType temperatureType)
    {
        this.tempurature.SetTempurature(temperatureType);
        UpdateHumanPriority();

        parentCell.CheckIsBeingShownInfo();
    }

    public bool HasWater()
    {
        return environment.currentEnvironmentType == EnvironmentType.Water
            || affectedByStructures.Contains(StructureType.WaterFactory);
    }

    public void SetCarrier(bool state)
    {
        hasCarrier = state;

        parentCell.CheckIsBeingShownInfo();
    }

    #region Lockdown

    public void SetIsLockDown(bool value, int infectionResistanceIncreased = 0, float lockdownDuration = 0f, AIContext ctx = null)
    {
        isLockdown = value;
        if (isLockdown)
        {
            if (canBeSterilized)
                SetInfectionLevel(0);

            isContagious = false;
            isBlocked = true;
            lockdownRemainingTicks = lockdownDuration;

            lockdownInfectionResistanceModifier = new(infectionResistanceIncreased, InfectionResistanceAdditiveSourceType.Lockdown, ModifierType.Additive);
            AddInfectionResistanceModifier(lockdownInfectionResistanceModifier);
            lockdownDetectionModifier = AddDetectionModifier(1f, DetectionAdditiveSourceType.Lockdown, ctx);

            mapManager.AddLockdownedCell(new Vector2Int(parentCell.X, parentCell.Y));
        }
        else
        {
            CellStageStats cellStageStats = PropertyDataManager.Instance.GetCellStageData(stage.type).cellStageStats;

            isContagious = cellStageStats.isContagious;
            isBlocked = false;
            lockdownRemainingTicks = 0f;

            if (lockdownDetectionModifier != null)
            {
                RemoveDetectionModifier(lockdownDetectionModifier, ctx);
                lockdownDetectionModifier = null;
            }

            mapManager.RemoveLockdownedCell(new Vector2Int(parentCell.X, parentCell.Y));
        }

        mapManager.AddCellNeedToUpdateVisual(new Vector2Int(parentCell.X, parentCell.Y));
        parentCell.CheckIsBeingShownInfo();
    }

    public void UpdateLockdownDuration(float deltaTime)
    {
        if (isLockdown)
        {
            lockdownRemainingTicks -= deltaTime;
            if (lockdownRemainingTicks <= 0)
            {
                SetIsLockDown(false);
            }
        }
    }

    #endregion

    #region Infection Level

    /// <summary>
    /// Increase or decrease the infection level of the cell by the value. Return infection points and evolution points gained from this update. 
    /// If the cell is already dead, this method will not change the infection level and will return zero points.
    /// </summary>
    public PointsGainedStruct UpdateInfectionLevel(int value)
    {
        if (stage.type == CellStageType.Dead)
            return new PointsGainedStruct(0, 0);

        infectionLevel += value;
        if (infectionLevel < Utility.minInfectionLevel) infectionLevel = Utility.minInfectionLevel;
        else if (infectionLevel > Utility.maxInfectionLevel) infectionLevel = Utility.maxInfectionLevel;

        (CellStageType, PointsGainedStruct) cellStageResult = stage.SetCellStageType(this.infectionLevel, this);

        SetStage(cellStageResult.Item1);
        PointsGainedStruct finalPointsGained = GetFinalPointGained(cellStageResult.Item2, population.type);

        return finalPointsGained;
    }

    /// <summary>
    /// Sets the infection level for the cell.
    /// </summary>
    /// <param name="infectionLevel">The infection level to assign. Must be a non-negative integer representing the severity of infection.</param>
    public void SetInfectionLevel(int infectionLevel)
    {
        if (stage.type == CellStageType.Dead)
            return;

        if (infectionLevel < Utility.minInfectionLevel || infectionLevel > Utility.maxInfectionLevel)
        {
            Debug.LogError($"Infection level must be between {Utility.minInfectionLevel} and {Utility.maxInfectionLevel}.");
            return;
        }

        this.infectionLevel = infectionLevel;

        (CellStageType, PointsGainedStruct) cellStageResult = stage.SetCellStageType(this.infectionLevel, this);

        SetStage(cellStageResult.Item1);
    }

    public void SetStage(CellStageType cellStageType)
    {
        if (stage.type == CellStageType.Dead)
            return;

        mapManager.UpdateInfectedRateAndDeadRate(new Vector2Int(parentCell.X, parentCell.Y),
            cellStageType, population.weight);

        UpdateHumanPriority();

        parentCell.CheckIsBeingShownInfo();
        mapManager.AddCellNeedToUpdateVisual(new Vector2Int(parentCell.X, parentCell.Y));
    }

    public void DetermineInfectionStats(CellStageStats cellStageStats)
    {
        if (stage.type == CellStageType.Dead || stage.type == CellStageType.Immune)
        {
            infectionLevel = cellStageStats.minInfectionValue;
        }

        bonusTargetInfectionGainPercent = cellStageStats.bonusTargetInfectionGainPercent;
        canBuildStructure = cellStageStats.canBuildStructure;
        canHasCarrier = cellStageStats.canHasCarrier;
        canBeSterilized = cellStageStats.canBeSterilized;

        if (stage.type == CellStageType.Safe || stage.type == CellStageType.Immune)
        {
            tickInfectedCount = 0;
            RemoveSterilizationResistanceModifierIfInfectedForALongTime();
        }

        if (!isLockdown)
        {
            isContagious = cellStageStats.isContagious;
            isBlocked = cellStageStats.isBlocked;
        }

        canSwitchToDead = cellStageStats.canSwitchToDead;
        toDeadTicksCount = cellStageStats.tickToDeadCount;

        if (toDeadTicksCount > 0)
        {
            TimeManager.Instance.ScheduleEvent(
                toDeadTicksCount,
                () => SetStage(CellStageType.Dead),
                0);
        }

        stage.priorityToHuman = cellStageStats.priorityToHuman;
        stage.priorityToMethods = cellStageStats.basePriorityToMethods;
    }

    #endregion

    #region Infection Resistance

    public InfectionResistanceModifier AddInfectionResistanceModifier(InfectionResistanceModifier modifier)
    {
        if (!infectionResistanceModifiers.Contains(modifier))
        {
            infectionResistanceModifiers.Add(modifier);
            RecalculateInfectionResistance();
        }

        return modifier; // return the modifier so that it can be removed later if needed
    }

    public void RemoveInfectionResistanceModifier(InfectionResistanceModifier modifier)
    {
        if (infectionResistanceModifiers.Remove(modifier))
        {
            RecalculateInfectionResistance();
        }
    }

    public void UpdateInfectionResistanceMultiplier(float value)
    {
        infectionResistanceMultiplier += value;
        RecalculateInfectionResistance();
    }

    public void RecalculateInfectionResistance()
    {
        float additive = 0;
        float multiplier = infectionResistanceMultiplier;

        foreach (var mod in infectionResistanceModifiers)
        {
            if (mod.ModifierType == ModifierType.Additive)
                additive += mod.Value;
            else
                multiplier += mod.Value;
        }

        finalInfectionResistance = Mathf.Clamp(
            Mathf.FloorToInt((baseInfectionResistance + additive) * multiplier),
            Utility.minInfectionResistance,
            Utility.maxInfectionResistance
        );

        if (finalInfectionResistance == Utility.maxInfectionResistance)
            SetStage(CellStageType.Immune);

        parentCell.CheckIsBeingShownInfo();
    }

    public int GetInfectionResistance()
    {
        RecalculateInfectionResistance();
        return finalInfectionResistance;
    }

    #endregion

    #region Sterilization Resistance

    public int GetSterilizationResistance()
    {
        return finalSterilizationResistance;
    }

    public SterilizationResistanceModifier AddSterilizationResistanceModifier(SterilizationResistanceModifier modifier)
    {
        sterilizationResistanceModifiers.Add(modifier);

        RecalculateSterilizationResistance();
        return modifier; // return the modifier so that it can be removed later if needed
    }

    public void RemoveSterilizationResistanceModifier(SterilizationResistanceModifier modifier)
    {
        if (sterilizationResistanceModifiers.Remove(modifier))
        {
            RecalculateSterilizationResistance();
        }
    }

    public void UpdateSterilizationResistanceMultiplier(float value)
    {
        baseSerilizationResistanceMultiplier += value;
    }

    public void RecalculateSterilizationResistance()
    {
        float additive = 0;
        float multiplier = baseSerilizationResistanceMultiplier;

        foreach (SterilizationResistanceModifier mod in sterilizationResistanceModifiers)
        {
            if (mod.ModifierType == ModifierType.Additive)
                additive += mod.Value;
            else
                multiplier += mod.Value;
        }

        finalSterilizationResistance = Mathf.Clamp(
            Mathf.FloorToInt((baseSterilizationResistance + additive) * multiplier),
            Utility.minSterilizationResistance,
            Utility.maxSterilizationResistance
        );

        parentCell.CheckIsBeingShownInfo();
    }

    public bool HasSterilizationResistance()
    {
        return finalSterilizationResistance > 0;
    }

    public bool HasSterilizationResistanceModifier(SterilizationResistanceModifier modifier)
    {
        return sterilizationResistanceModifiers.Contains(modifier);
    }

    #endregion

    #region Detection

    public CellDetectionModifier AddDetectionModifier(float value, DetectionAdditiveSourceType sourceType, AIContext ctx)
    {
        var modifier = new CellDetectionModifier(value, sourceType);
        detectionAdditiveModifiers.Add(modifier);

        RecalculateDetection(ctx);
        return modifier; // return the modifier so that it can be removed later if needed
    }

    public void RemoveDetectionModifier(CellDetectionModifier modifier, AIContext ctx)
    {
        if (detectionAdditiveModifiers.Remove(modifier))
        {
            RecalculateDetection(ctx);
        }
    }

    public void UpdateDetectionModifierValue(CellDetectionModifier modifier, float newValue, AIContext ctx)
    {
        modifier.Value = newValue;
        RecalculateDetection(ctx);
    }

    public void RecalculateDetection(AIContext ctx)
    {
        if (isDetected) return;

        HumanAIManager human = HumanAIManager.Instance;

        float total = 0;

        foreach (var mod in detectionAdditiveModifiers)
        {
            total += mod.Value;
        }

        baseDetection = ctx.InfectionRateDetected * human.InfectionRateDetectedWeight
            + ctx.DeadRateDetected * human.DeadRateDetectedWeight
            + stage.detectionPercent;

        finalDetection = Mathf.Clamp(
            baseDetection + (total * detectionMultiplier),
            Utility.minDetection,
            Utility.maxDetection
        );

        float random = Random.value;
        if (random < finalDetection)
        {
            isDetected = true;
        }
        else
        {
            isDetected = false;
        }

        parentCell.CheckIsBeingShownInfo();
    }

    #endregion

    public void UpdateHumanPriority()
    {
        priorityToHuman =
            stage.priorityToHuman +
            environment.priorityToHuman +
            population.priorityToHuman +
            structure.currentPriorityToHuman;

        priorityToHuman = Mathf.Clamp01(priorityToHuman);
    }

    #region Structure Effect

    public void AddStructureEffect(StructureType structureType)
    {
        affectedByStructures.Add(structureType);
        parentCell.CheckIsBeingShownInfo();
    }

    public void RemoveStructureEffect(StructureType structureType)
    {
        if (affectedByStructures.Contains(structureType))
            affectedByStructures.Remove(structureType);

        parentCell.CheckIsBeingShownInfo();
    }

    #endregion

    #region Carrier Spread Chance
    public void UpdateIncreaseCarrierSpreadChance(float value)
    {
        additionalCarrierSpreadChancePercent += value;
        if (additionalCarrierSpreadChancePercent < Utility.minAdditionalCarrierSpreadChance)
            additionalCarrierSpreadChancePercent = Utility.minAdditionalCarrierSpreadChance;
        else if (additionalCarrierSpreadChancePercent > Utility.maxAdditionalCarrierSpreadChance)
            additionalCarrierSpreadChancePercent = Utility.maxAdditionalCarrierSpreadChance;
    }

    public void SetIncreaseCarrierSpreadChance(float value)
    {
        additionalCarrierSpreadChancePercent = value;
        if (additionalCarrierSpreadChancePercent < Utility.minAdditionalCarrierSpreadChance)
            additionalCarrierSpreadChancePercent = Utility.minAdditionalCarrierSpreadChance;
        else if (additionalCarrierSpreadChancePercent > Utility.maxAdditionalCarrierSpreadChance)
            additionalCarrierSpreadChancePercent = Utility.maxAdditionalCarrierSpreadChance;
    }

    #endregion

    #region Can Be Sterilized

    public void SetSterilizationImmunityTicks(int sterilizationImmunityTicks = 0)
    {
        if (sterilizationImmunityTicks <= 0)
        {
            canBeSterilized = true;
            this.sterilizationImmunityTicks = 0;
        }
        else
        {
            canBeSterilized = false;
            this.sterilizationImmunityTicks = sterilizationImmunityTicks;
        }

        parentCell.CheckIsBeingShownInfo();
    }

    public void UpdateSterilizationImmunityTicks()
    {
        if (!canBeSterilized && sterilizationImmunityTicks > 0)
        {
            sterilizationImmunityTicks--;
            if (sterilizationImmunityTicks <= 0)
            {
                SetSterilizationImmunityTicks();
                parentCell.CheckIsBeingShownInfo();
            }
        }
    }

    #endregion

    #region Surface's Upgrade

    private void AddSterilizationResistanceModifierIfInfectedForALongTime()
    {
        AddSterilizationResistanceModifier(sterilizationResistanceModifierIfInfectedForALongTime);
    }

    private void RemoveSterilizationResistanceModifierIfInfectedForALongTime()
    {
        if (sterilizationResistanceModifierIfInfectedForALongTime != null
            && sterilizationResistanceModifiers.Contains(sterilizationResistanceModifierIfInfectedForALongTime))
        {
            sterilizationResistanceModifiers.Remove(sterilizationResistanceModifierIfInfectedForALongTime);
            sterilizationResistanceModifierIfInfectedForALongTime = null;
        }
    }

    public void AddSterilizationResistancBonusPercentIfInfectedForALongTime(int minTickToBonusSterilizationResistance,
        SterilizationResistanceModifier sterilizationResistanceModifierIfInfectedForALongTime)
    {
        this.minTickToBonusSterilizationResistance = minTickToBonusSterilizationResistance;
        this.sterilizationResistanceModifierIfInfectedForALongTime = sterilizationResistanceModifierIfInfectedForALongTime;
    }

    #endregion

    #region Water's Upgrade

    public void AddSterilizationResistancBonusPercentIfCellHasWater(SterilizationResistanceModifier sterilizationResistanceModifierForWaterCell)
    {
        this.sterilizationResistanceModifierForWaterCell = sterilizationResistanceModifierForWaterCell;
        AddSterilizationResistanceModifier(sterilizationResistanceModifierForWaterCell);
    }

    public void RemoveSterilizationResistancBonusPercentIfCellHasWater()
    {
        if (sterilizationResistanceModifierForWaterCell != null
            && sterilizationResistanceModifiers.Contains(sterilizationResistanceModifierForWaterCell))
        {
            sterilizationResistanceModifiers.Remove(sterilizationResistanceModifierForWaterCell);
            sterilizationResistanceModifierForWaterCell = null;
        }
    }

    #endregion

    public void UpdateTickInfectedCount()
    {
        if (stage.type != CellStageType.Safe
            || stage.type != CellStageType.Immune
            || stage.type != CellStageType.Dead)
        {
            tickInfectedCount++;

            if (minTickToBonusSterilizationResistance > 0
                && tickInfectedCount >= minTickToBonusSterilizationResistance
                && sterilizationResistanceModifierIfInfectedForALongTime == null)
            {
                AddSterilizationResistanceModifierIfInfectedForALongTime();
            }
        }
    }


    public bool IsSafe()
    {
        return stage.type == CellStageType.Safe || stage.type == CellStageType.Immune;
    }

    public PointsGainedStruct GetFinalPointGained(PointsGainedStruct pointsGained, PopulationType populationType)
    {
        PopulationData populationData = PropertyDataManager.Instance.GetPopulationData(populationType);

        int finalEvolutionPoints = Mathf.FloorToInt(pointsGained.evolutionPoints * populationData.evolutionPointMultiplier);
        int finalInfectionPoints = Mathf.FloorToInt(pointsGained.infectionPoints * populationData.infectionPointMultiplier);

        return new PointsGainedStruct(finalEvolutionPoints, finalInfectionPoints);
    }
}
