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

    public float targetInfectionIncreasePercent = 0;

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

    //public bool currentHasWater = false;
    //public bool originalHasWater = false;

    public bool canSwitchToDead = false;
    public float toDeadTicksCount = 0;

    public bool canBeDisinfected = false;
    public float disinfectionImmunityTicks = 0;
    public float disinfectionImmunityTicksCount = 0;

    public float priorityToHuman;
    public PriorityToMethods priorityToMethods;

    private MapManager mapManager;
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
            //this.originalHasWater = true;
            //this.currentHasWater = true;
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
    }

    public void SetStats(float populationValue, float tempuratureValue, GridCell cell)
    {
        parentCell = cell;
        this.population.SetPopulation(populationValue);
        this.tempurature.SetTempurature(tempuratureValue);
        this.environment.SetEnvironmentType(population.type, tempurature.type);
        UpdateHumanPriority();
        mapManager = MapManager.Instance;
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
            SetInfectionLevel(0);
            isContagious = false;
            isBlocked = true;
            lockdownRemainingTicks = lockdownDuration;

            lockdownInfectionResistanceModifier = new(infectionResistanceIncreased, InfectionResistanceAdditiveSourceType.Lockdown);
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
    /// Increase or decrease the infection level of the cell by the value
    /// </summary>
    public void UpdateInfectionLevel(int value)
    {
        if (stage.type == CellStageType.Dead)
            return;

        infectionLevel += value;
        if (infectionLevel < Utility.minInfectionLevel) infectionLevel = Utility.minInfectionLevel;
        else if (infectionLevel > Utility.maxInfectionLevel) infectionLevel = Utility.maxInfectionLevel;
        SetStage(stage.SetCellStageType(this.infectionLevel, this));
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

        SetStage(stage.SetCellStageType(this.infectionLevel, this));
    }

    public void SetStage(CellStageType cellStageType)
    {
        if (stage.type == CellStageType.Dead)
            return;

        stage.SetCellStageType(cellStageType, this);

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

        targetInfectionIncreasePercent = cellStageStats.targetInfectionIncreasePercent;
        canBuildStructure = cellStageStats.canBuildStructure;
        canHasCarrier = cellStageStats.canHasCarrier;
        canBeDisinfected = cellStageStats.canBeDisinfected;

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
        infectionResistanceModifiers.Add(modifier);

        RecalculateInfectionResistance();
        return modifier; // return the modifier so that it can be removed later if needed
    }

    public void RemoveInfectionResistanceModifier(InfectionResistanceModifier modifier)
    {
        if (infectionResistanceModifiers.Remove(modifier))
        {
            RecalculateInfectionResistance();
        }
    }

    public InfectionResistanceModifier UpdateInfectionResistanceModifierValue(InfectionResistanceModifier modifier, int newValue)
    {
        if (!infectionResistanceModifiers.Contains(modifier))
        {
            modifier.Value = newValue;
            return AddInfectionResistanceModifier(modifier);
        }
        else
            modifier.Value = newValue;

        RecalculateInfectionResistance();

        return modifier;
    }

    private void RecalculateInfectionResistance()
    {
        int total = 0;

        foreach (var mod in infectionResistanceModifiers)
        {
            total += mod.Value;
        }

        finalInfectionResistance = Mathf.Clamp(
            Mathf.RoundToInt((baseInfectionResistance + total) * infectionResistanceMultiplier),
            Utility.minInfectionResistance,
            Utility.maxInfectionResistance
        );

        if (finalInfectionResistance == Utility.maxInfectionResistance)
            SetStage(CellStageType.Immune);

        parentCell.CheckIsBeingShownInfo();
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

    public bool IsSafe()
    {
        return stage.type == CellStageType.Safe || stage.type == CellStageType.Immune;
    }
}
