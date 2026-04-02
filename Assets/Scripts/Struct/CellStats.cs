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
    public int currentInfectionResistance = 0;
    public int originalInfectionResistance = 0;
    [Range(0, 1)]
    public float targetInfectionIncreasePercent = 0;

    public bool isDetected = false;
    [Range(0, 1)]
    public float currentDetectionPercent = 0;
    [Range(0, 1)]
    public float originalDetectionPercent = 0;

    public bool isContagious = false; // whether the cell can spread infection to other cells 
    public bool isBlocked = false; // whether the cell can't be infected by other cells
    public bool isLockdown = false; // whether the cell can't spread infection to other cells and can't be infected by other cells

    public bool canHasStructure = true;
    public bool canHasCarrier = true;
    public bool currentHasWater = false;
    public bool originalHasWater = false;
    public bool hasCarrier = false;

    public bool canSwitchToDead = false;
    public float toDeadTicksCount = 0;

    public bool canBeDisinfected = false;
    public float disinfectionImmunityTicks = 0;
    public float disinfectionImmunityTicksCount = 0;

    public float priorityToHuman;
    public PriorityToMethods priorityToMethods;

    private MapManager mapManager;

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
            this.originalHasWater = true;
            this.currentHasWater = true;
            this.canHasStructure = false;
            this.canHasCarrier = false;
        }
        else if (environment == EnvironmentType.Mountain)
        {
            this.isBlocked = true;
            this.isContagious = false;
            this.canHasStructure = false;
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
    }

    //public void SetEnvironment(EnvironmentType environmentType)
    //{
    //    this.environment.SetEnvironmentType(environmentType);
    //    if (environmentType == EnvironmentType.Water)
    //    {
    //        this.originalHasWater = true;
    //        this.currentHasWater = true;
    //        this.canHasStructure = false;
    //        this.canHasCarrier = false;
    //    }
    //    else if (environmentType == EnvironmentType.Mountain)
    //    {
    //        this.isBlocked = true;
    //        this.isContagious = false;
    //        this.canHasStructure = false;
    //        this.canHasCarrier = false;
    //    }
    //    UpdateHumanPriority();

    //    parentCell.CheckIsBeingShownInfo();
    //}

    public void SetTempurature(TemperatureType temperatureType)
    {
        this.tempurature.SetTempurature(temperatureType);
        UpdateHumanPriority();

        parentCell.CheckIsBeingShownInfo();
    }

    public void SetCurrentWater(bool state)
    {
        currentHasWater = state;

        parentCell.CheckIsBeingShownInfo();
    }

    public void SetOriginalWater(bool state)
    {
        originalHasWater = state;
        currentHasWater = state;

        parentCell.CheckIsBeingShownInfo();
    }

    public void SetCarrier(bool state)
    {
        hasCarrier = state;

        parentCell.CheckIsBeingShownInfo();
    }

    public void SetIsLockDown(bool value)
    {
        this.isLockdown = value;
        if (this.isLockdown)
        {
            isContagious = false;
            isBlocked = true;
        }
        else
        {
            isContagious = true;
            isBlocked = false;
        }

        parentCell.CheckIsBeingShownInfo();
    }

    #region Infection Level

    /// <summary>
    /// Increase or decrease the infection level of the cell by the value
    /// </summary>
    public void UpdateInfectionLevel(int value)
    {
        infectionLevel += value;
        if (infectionLevel < 0) infectionLevel = 0;
        else if (infectionLevel > 100) infectionLevel = 100;

        CellStageType cellStageType = stage.SetCellStageType(infectionLevel, this);

        UpdateGameInfectionStats(cellStageType);

        UpdateHumanPriority();
        if (isDetected)
        {
            //human.UpdateInfectionStats();
        }

        parentCell.CheckIsBeingShownInfo();
    }

    /// <summary>
    /// Sets the infection level for the cell.
    /// </summary>
    /// <param name="infectionLevel">The infection level to assign. Must be a non-negative integer representing the severity of infection.</param>
    public void SetInfectionLevel(int infectionLevel)
    {
        if (infectionLevel < 0 || infectionLevel > 100)
        {
            Debug.LogError("Infection level must be between 0 and 100.");
            return;
        }

        this.infectionLevel = infectionLevel;
        CellStageType cellStageType = stage.SetCellStageType(this.infectionLevel, this);

        UpdateGameInfectionStats(cellStageType);

        UpdateHumanPriority();
        if (isDetected)
        {
            //human.UpdateInfectionStats();
        }

        parentCell.CheckIsBeingShownInfo();
    }

    public void SetStage(CellStageType cellStageType)
    {
        stage.SetCellStageType(cellStageType, this);

        UpdateGameInfectionStats(cellStageType);

        UpdateHumanPriority();
        if (isDetected)
        {
            //human.UpdateInfectionStats();
        }

        parentCell.CheckIsBeingShownInfo();
    }

    public void DetermineInfectionFlags(CellStageStats cellStageStats)
    {
        targetInfectionIncreasePercent = cellStageStats.targetInfectionIncreasePercent;
        float increaseDetectionPercent = cellStageStats.detectionPercent - originalDetectionPercent;

        currentDetectionPercent += increaseDetectionPercent;
        originalDetectionPercent += increaseDetectionPercent;

        int increaseInfectionResistance = cellStageStats.infectionResistance - originalInfectionResistance;
        currentInfectionResistance += increaseInfectionResistance;
        originalInfectionResistance += increaseInfectionResistance;

        if (!isLockdown)
        {
            isContagious = cellStageStats.isContagious;
            isBlocked = cellStageStats.isBlocked;
        }

        canSwitchToDead = cellStageStats.canSwitchToDead;
        toDeadTicksCount = cellStageStats.tickToDeadCount;

        if(toDeadTicksCount > 0)
        {
            TimeManager.Instance.ScheduleEvent(
                toDeadTicksCount,
                () => SetStage(CellStageType.Dead),
                0);
        }

        stage.priorityToHuman = cellStageStats.priorityToHuman;
        stage.priorityToMethods = cellStageStats.basePriorityToMethods;
    }

    private void UpdateGameInfectionStats(CellStageType cellStageType)
    {
        switch (cellStageType)
        {
            case CellStageType.Safe:
            case CellStageType.Immune:
                //if previous stage == Exposed || Infected || Critical
                //mapManager.UpdateInfectedRate(-population.weight);
                break;
            case CellStageType.Exposed:
                mapManager.UpdateInfectedRate(population.weight);
                break;
            case CellStageType.Dead:
                mapManager.UpdateDeadRate(population.weight);
                break;
            default:
                break;
        }
    }

    #endregion

    #region Infection Resistance

    /// <summary>
    /// Increase or decrease the original infection resistance of the cell by the value
    /// 
    public void UpdateOriginalInfectionResistance(int value)
    {
        originalInfectionResistance += value;
        currentInfectionResistance += value;

        if (originalInfectionResistance < 0) originalInfectionResistance = 0;
        else if (originalInfectionResistance > 100) originalInfectionResistance = 100;

        if (currentInfectionResistance < 0) currentInfectionResistance = 0;
        else if (currentInfectionResistance > 100) currentInfectionResistance = 100;

        parentCell.CheckIsBeingShownInfo();
    }

    /// <summary>
    /// Sets the original infection resistance for the cell.
    /// </summary>
    /// <param name="infectionResistance">The infection resistance to assign. Must be a non-negative integer.</param>

    public void SetOriginalInfectionResistance(int infectionResistance)
    {
        if (infectionResistance < 0 || infectionResistance > 100)
        {
            Debug.LogError("Infection resistance must be between 0 and 100.");
            return;
        }

        this.originalInfectionResistance = infectionResistance;
    }

    /// <summary>
    /// Increase or decrease the current infection resistance of the cell by the value
    /// 
    public void UpdateCurrentlInfectionResistance(int value)
    {
        currentInfectionResistance += value;

        if (currentInfectionResistance < 0) currentInfectionResistance = 0;
        else if (currentInfectionResistance > 100) currentInfectionResistance = 100;

        parentCell.CheckIsBeingShownInfo();
    }

    /// <summary>
    /// Sets the current infection resistance for the cell.
    /// </summary>
    /// <param name="infectionResistance">The infection resistance to assign. Must be a non-negative integer.</param>

    public void SetCurrentInfectionResistance(int infectionResistance)
    {
        if (infectionResistance < 0 || infectionResistance > 100)
        {
            Debug.LogError("Infection resistance must be between 0 and 100.");
            return;
        }

        this.currentInfectionResistance = infectionResistance;

        parentCell.CheckIsBeingShownInfo();
    }

    #endregion

    public void UpdateDetectionRate()
    {
        originalDetectionPercent = currentDetectionPercent;
        //currentDetectionPercent = human.infectionRateDetected * human.infectionRateDetectedWeight
        //    + human.deadRateDetected * human.deadRateDetectedWeight
        //    + stage.cellStageStats.detectionPercent;

        if (Random.value < currentDetectionPercent)
        {
            isDetected = true;
            //human.Detected(this);
            // virus update detection list
        }

        parentCell.CheckIsBeingShownInfo();
    }

    public void UpdateHumanPriority()
    {
        priorityToHuman =
            stage.priorityToHuman +
            environment.priorityToHuman +
            population.priorityToHuman +
            structure.currentPriorityToHuman;

        priorityToHuman = Mathf.Clamp01(priorityToHuman);
    }
}
