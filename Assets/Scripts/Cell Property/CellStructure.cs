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

            EnableStructure();

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

    public void SetAffectedByStructuresListOfCellsInRange(Vector2Int pos)
    {
        if (type == StructureType.None) return;

        this.pos = pos;

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

    public void RemoveAffectedByStructuresListOfCellsInRange(Vector2Int pos)
    {
        if (type == StructureType.None) return;

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

    public PointsGainedStruct DisableStructure()
    {
        if (type == StructureType.None || !isActive) return new PointsGainedStruct(0, 0);

        isActive = false;

        logic.DisapplyGlobalEffectWhenDisabled();

        if (!hasBeenDisabledBefore)
        {
            hasBeenDisabledBefore = true;

            return new PointsGainedStruct(evolutionPointWhenDisabled, infectionPointWhenDisabled);
        }

        originalPriorityToHuman = currentPriorityToHuman;
        currentPriorityToHuman = 0f;

        originalPriorityToMethods = currentPriorityToMethods;
        currentPriorityToMethods = new PriorityToMethods();

        RemoveAffectedByStructuresListOfCellsInRange(new Vector2Int(parentCell.X, parentCell.Y));

        return new PointsGainedStruct(0, 0);
    }

    public void EnableStructure()
    {
        if (type == StructureType.None || isActive) return;

        isActive = true;

        logic.ApplyGlobalEffectWhenEnabled();

        currentPriorityToHuman = originalPriorityToHuman;
        currentPriorityToMethods = originalPriorityToMethods;

        SetAffectedByStructuresListOfCellsInRange(new Vector2Int(parentCell.X, parentCell.Y));
    }

    public PointsGainedStruct DestroyStructure()
    {
        PointsGainedStruct pointsGained = DisableStructure();

        SetDefaultValues();

        pointsGained.evolutionPoints += evolutionPointWhenDestroyed;
        pointsGained.infectionPoints += infectionPointWhenDestroyed;

        return pointsGained;
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
}
