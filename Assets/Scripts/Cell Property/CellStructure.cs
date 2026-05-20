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

    private GridCell parentCell;

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

            EnableStructure();

            return;
        }
    }

    public void SetAffectedByStructuresListOfCellsInRange(Vector2Int pos, int range)
    {
        if (type == StructureType.None) return;

        Grid<GridCell> grid = MapManager.Instance.GetGrid();
        List<GridCell> effectedCells = grid.GetNeighbourInCircleWithRange(pos.x, pos.y, range);
        effectedCells.Add(parentCell);
        foreach (GridCell cell in effectedCells)
        {
            cell.Stats.AddStructureEffect(type);
            logic.ApplyEffectToCellWhenEnabled(cell);
            logic.DisapplyEffectToCellWhenEnabled(cell);
        }
    }

    public void RemoveAffectedByStructuresListOfCellsInRange(Vector2Int pos, int range)
    {
        if (type == StructureType.None) return;

        Grid<GridCell> grid = MapManager.Instance.GetGrid();
        List<GridCell> effectedCells = grid.GetNeighbourInCircleWithRange(pos.x, pos.y, range);
        effectedCells.Add(parentCell);
        foreach (GridCell cell in effectedCells)
        {
            cell.Stats.RemoveStructureEffect(type);
            logic.ApplyEffectToCellWhenDisabled(cell);
            logic.DisapplyEffectToCellWhenDisabled(cell);
        }
    }

    public void DisableStructure()
    {
        if (type == StructureType.None || !isActive) return;

        isActive = false;

        logic.DisapplyGlobalEffectWhenDisabled();

        if (!hasBeenDisabledBefore)
        {
            hasBeenDisabledBefore = true;

            // TO DO: Add points to the player for the structure being disabled
        }

        originalPriorityToHuman = currentPriorityToHuman;
        currentPriorityToHuman = 0f;

        originalPriorityToMethods = currentPriorityToMethods;
        currentPriorityToMethods = new PriorityToMethods();

        if (effectRange > 0)
            RemoveAffectedByStructuresListOfCellsInRange(new Vector2Int(parentCell.X, parentCell.Y), effectRange);
    }

    public void EnableStructure()
    {
        if (type == StructureType.None || isActive) return;

        isActive = true;

        logic.ApplyGlobalEffectWhenEnabled();

        currentPriorityToHuman = originalPriorityToHuman;
        currentPriorityToMethods = originalPriorityToMethods;

        if (effectRange > 0)
            SetAffectedByStructuresListOfCellsInRange(new Vector2Int(parentCell.X, parentCell.Y), effectRange);
    }

    public void DestroyStructure()
    {
        DisableStructure();
        // TO DO: Add points to the player for the structure being destroyed

        SetDefaultValues();
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
