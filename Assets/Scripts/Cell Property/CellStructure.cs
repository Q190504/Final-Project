using System;
using System.Collections.Generic;
using UnityEngine;

public class CellStructure
{
    public StructureType type;
    public Structure logic;
    public bool isActive;
    private bool hasBeenDisabledBefore;

    [Header("Base Gameplay Values")]
    public int effectRange;
    public float currentPriorityToHuman;
    public float originalPriorityToHuman;
    public PriorityToMethods currentPriorityToMethods;
    public PriorityToMethods originalPriorityToMethods;

    public int infectionPointWhenDisabled;
    public int evolutionPointWhenDisabled;

    public int infectionPointWhenDestroyed;
    public int evolutionPointWhenDestroyed;

    private int disableTickRemainning = 0;
    private Vector2Int pos;

    private GridCell parentCell;
    private Grid<GridCell> grid;

    public CellStructure()
    {
        SetDefaultValues();
    }

    public void SetStructure(StructureType structureType, GridCell cell)
    {
        parentCell = cell;
        pos = new Vector2Int(parentCell.X, parentCell.Y);
        type = structureType;

        if (type == StructureType.None) return;

        StructureDataSO structureData = PropertyDataManager.Instance.GetStructureData(type);
        if (structureData != null)
        {
            logic = structureData.CreateLogic();
            effectRange = Mathf.FloorToInt(structureData.effectRangePercent * MapManager.Instance.GetBaseSize());
            currentPriorityToHuman = originalPriorityToHuman = structureData.priorityToHuman;
            currentPriorityToMethods = originalPriorityToMethods = structureData.basePriorityToMethods;

            infectionPointWhenDestroyed = structureData.infectionPointWhenDestroyed;
            evolutionPointWhenDestroyed = structureData.evolutionPointWhenDestroyed;

            EnableStructure(parentCell.Stats.stage.type);

            return;
        }
    }

    public void ApplyTickEffectToCellsInRange()
    {
        if (effectRange > 0 && grid != null)
        {
            List<GridCell> effectedCells = grid.GetNeighbourInCircleWithRange(pos.x, pos.y, effectRange);

            foreach (GridCell cell in effectedCells)
            {
                logic.ApplyTickEffectToCell(cell);
            }
        }
    }

    public void SetAffectedByStructuresListOfCellsInRange()
    {
        if (type == StructureType.None || logic == null) return;

        grid = MapManager.Instance.GetGrid();
        List<GridCell> effectedCells = grid.GetNeighbourInCircleWithRange(pos.x, pos.y, effectRange);

        effectedCells.Add(parentCell);
        foreach (GridCell cell in effectedCells)
        {
            cell.Stats.AddStructureEffect(type);
            logic.ApplyEffectToCellWhenEnabled(cell);
            logic.DisapplyEffectToCellWhenEnabled(cell);
        }
    }

    public void RemoveAffectedByStructuresListOfCellsInRange()
    {
        if (type == StructureType.None || logic == null) return;

        grid = MapManager.Instance.GetGrid();
        List<GridCell> effectedCells = grid.GetNeighbourInCircleWithRange(pos.x, pos.y, effectRange);
        effectedCells.Add(parentCell);
        foreach (GridCell cell in effectedCells)
        {
            cell.Stats.RemoveStructureEffect(type);
            logic.ApplyEffectToCellWhenDisabled(cell);
            logic.DisapplyEffectToCellWhenDisabled(cell);
        }
    }

    public PointsGainedStruct DisableStructure(CellStageType? cellStageType = null, bool ignoreConditions = false, int disableTick = 0, bool updateVisualInstantly = false)
    {
        PointsGainedStruct pointsGained = new();

        if (MatchManager.Instance != null)
        {
            GameState gameState = MatchManager.Instance.GetGameState();
            if (!IsValidGameStateToDisableOrDestroy(gameState)) return pointsGained;
        }

        if (type == StructureType.None || !isActive) return pointsGained;

        if (!ignoreConditions)
        {
            if (cellStageType == null)
            {
                Debug.LogError("cellStageType is null");
                return pointsGained;
            }
            else if (cellStageType != CellStageType.Critical)
                return pointsGained;
        }

        isActive = false;

        logic.DisapplyGlobalEffectWhenDisabled();

        // If disabled by skill, no bonus point
        if (disableTick > 0)
            disableTickRemainning = disableTick;
        else
        {
            if (!hasBeenDisabledBefore)
            {
                hasBeenDisabledBefore = true;

                pointsGained.evolutionPoints = evolutionPointWhenDisabled;
                pointsGained.infectionPoints = infectionPointWhenDisabled;
            }
        }

        originalPriorityToHuman = currentPriorityToHuman;
        currentPriorityToHuman = 0f;

        originalPriorityToMethods = currentPriorityToMethods;
        currentPriorityToMethods = new PriorityToMethods();

        RemoveAffectedByStructuresListOfCellsInRange();

        if (updateVisualInstantly)
            parentCell.Stats.AddCellNeedToUpdateVisualInstantly(pos);
        else
            parentCell.Stats.AddCellNeedToUpdateVisualNextTick(pos);

        return pointsGained;
    }

    public PointsGainedStruct DestroyStructure(CellStageType cellStageType, bool ignoreConditions = false, bool updateVisualInstantly = false)
    {
        PointsGainedStruct pointsGained = new();

        if (MatchManager.Instance != null)
        {
            GameState gameState = MatchManager.Instance.GetGameState();
            if (!IsValidGameStateToDisableOrDestroy(gameState)) return pointsGained;
        }

        if (!ignoreConditions && cellStageType != CellStageType.Dead) return pointsGained;
        pointsGained = DisableStructure(cellStageType);

        SetDefaultValues();

        pointsGained.evolutionPoints += evolutionPointWhenDestroyed;
        pointsGained.infectionPoints += infectionPointWhenDestroyed;

        if (updateVisualInstantly)
            parentCell.Stats.AddCellNeedToUpdateVisualInstantly(pos);
        else
            parentCell.Stats.AddCellNeedToUpdateVisualNextTick(pos);
        return pointsGained;
    }

    public void EnableStructure(CellStageType cellStageType, bool ignoreConditions = false, bool updateVisualInstantly = false)
    {
        if (MatchManager.Instance != null)
        {
            GameState gameState = MatchManager.Instance.GetGameState();
            if (!IsValidGameStateToEnable(gameState)) return;
        }

        if (type == StructureType.None || isActive || disableTickRemainning > 0) return;

        if (!ignoreConditions && !SuitableStageToEnableStructure(cellStageType)) return;

        isActive = true;

        logic.ApplyGlobalEffectWhenEnabled();

        currentPriorityToHuman = originalPriorityToHuman;
        currentPriorityToMethods = originalPriorityToMethods;

        SetAffectedByStructuresListOfCellsInRange();

        if (updateVisualInstantly)
            parentCell.Stats.AddCellNeedToUpdateVisualInstantly(pos);
        else
            parentCell.Stats.AddCellNeedToUpdateVisualNextTick(pos);
    }

    private bool SuitableStageToEnableStructure(CellStageType cellStageType)
    {
        return cellStageType == CellStageType.Safe
            || cellStageType == CellStageType.Exposed
            || cellStageType == CellStageType.Infected
            || cellStageType == CellStageType.Immune;
    }

    public void UpdateDisableTime()
    {
        if (isActive || disableTickRemainning <= 0) return;

        disableTickRemainning--;

        if (disableTickRemainning <= 0)
        {
            disableTickRemainning = 0;
            EnableStructure(parentCell.Stats.stage.type);
        }
    }

    private void SetDefaultValues()
    {
        isActive = false;
        hasBeenDisabledBefore = false;
        type = StructureType.None;
        logic = null;
        effectRange = 0;
        currentPriorityToHuman = originalPriorityToHuman = 0f;
        currentPriorityToMethods = originalPriorityToMethods = new PriorityToMethods();
    }

    private bool IsValidGameStateToDisableOrDestroy(GameState gameState)
    {
        return gameState == GameState.Playing || gameState == GameState.Paused;
    }

    private bool IsValidGameStateToEnable(GameState gameState)
    {
        return gameState == GameState.NotStarted
            || gameState == GameState.Playing
            || gameState == GameState.Paused;
    }
}
