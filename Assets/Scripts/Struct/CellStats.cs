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
    public int infectionResistance = 0;
    [Range(0, 1)]
    public float targetInfectionIncreasePercent = 0;
    //public int increaseInfectionLevelPerTick = 0;

    public bool isDetected = false;
    [Range(0, 1)]
    public float currentDetectionPercent = 0;
    [Range(0, 1)]
    public float originalDetectionPercent = 0;

    public bool isContagious = true;
    public bool isBlocked = false;
    public bool isLockdown = false;

    public bool canHasStructure = true;
    public bool canHasCarrier = true;
    public bool hasWater = false;
    public bool hasCarrier = false;

    public bool canSwitchToDead = false;
    public float toDeadTicks = 0;
    public float toDeadTicksCount = 0;

    public bool canBeDisinfected = false;
    public float disinfectionImmunityTicks = 0;
    public float disinfectionImmunityTicksCount = 0;

    public float priorityToHuman;
    public PriorityToMethods priorityToMethods;

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

    public void SetStats(PopulationType population, TempuratureType tempurature, EnvironmentType environment, GridCell cell)
    {
        parentCell = cell;

        affectedByStructures = new List<StructureType>();
        priorityToMethods = new PriorityToMethods();

        this.population.SetPopulation(population);
        this.tempurature.SetTempurature(tempurature);
        this.environment.SetEnvironmentType(environment);

        if (environment == EnvironmentType.Water)
        {
            this.hasWater = true;
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
    }

    public void SetStats(float populationValue, float tempuratureValue ,GridCell cell)
    {
        parentCell = cell;
        this.population.SetPopulation(populationValue);
        this.tempurature.SetTempurature(tempuratureValue);
        this.environment.SetEnvironmentType(population.type, tempurature.type);
    }

    public void SetStructure(StructureType structure, GridCell cell)
    {
        this.structure.SetStructure(structure, cell);
    }

    public void SetInfectionLevel(int infectionLevel)
    {
        CellStageType cellStageType = stage.SetCellStageType(infectionLevel);

        if (cellStageType == CellStageType.Safe)
        {

        }
        else if (cellStageType == CellStageType.Exposed)
        {

        }
        else if (cellStageType == CellStageType.Infected)
        {

        }
        else if (cellStageType == CellStageType.Critical)
        {

        }
        else if (cellStageType == CellStageType.Dead)
        {

        }
        else if (cellStageType == CellStageType.Immune)
        {

        }
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
    }
}
