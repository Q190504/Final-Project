using System.Collections.Generic;
using UnityEngine;

public class CellStructure
{
    public StructureType type;
    public Structure logic;
    private bool isActive;
    private bool hasBeenDestroyedBefore;

    [Header("Base Gameplay Values")]
    public int effectRange;
    public float currentPriorityToHuman;
    public float originalPriorityToHuman;
    public PriorityToMethods currentPriorityToMethods;
    public PriorityToMethods originalPriorityToMethods;

    private GridCell parentCell;

    public CellStructure()
    {
        isActive = false;
        hasBeenDestroyedBefore = false;
        type = StructureType.None;
        logic = null;
        effectRange = 0;
        currentPriorityToHuman = originalPriorityToHuman = 0f;
        currentPriorityToMethods = originalPriorityToMethods = new PriorityToMethods();
    }

    public void SetStructure(StructureType structureType, GridCell cell)
    {
        parentCell = cell;
        type = structureType;

        if (type == StructureType.None) return;

        StructureDataSO structureData = CellPropertyManager.Instance.GetStructureData(type);
        if (structureData != null)
        {
            isActive = true;
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

        for (int i = pos.x - range; i < pos.x + range; i++)
        {
            for (int j = pos.y - range; j < pos.y + range; j++)
            {
                if (grid.IsInBounds(i, j))
                {
                    GridCell cell = grid.GetCell(i, j);
                    cell.Stats.AddStructureEffect(type);
                    logic.ApplyEffectToCell(cell);
                }
            }
        }
    }

    public void RemoveAffectedByStructuresListOfCellsInRange(Vector2Int pos, int range)
    {
        if (type == StructureType.None) return;

        Grid<GridCell> grid = MapManager.Instance.GetGrid();
        for (int i = pos.x - range; i < pos.x + range; i++)
        {
            for (int j = pos.y - range; j < pos.y + range; j++)
            {
                if (grid.IsInBounds(i, j))
                {
                    GridCell cell = grid.GetCell(i, j);
                    cell.Stats.RemoveStructureEffect(type);
                    logic.DisapplyEffectToCell(cell);
                }
            }
        }
    }

    public void DisableStructure()
    {
        if (type == StructureType.None) return;

        isActive = false;

        logic.DisapplyGlobalEffect();

        if (!hasBeenDestroyedBefore)
        {
            hasBeenDestroyedBefore = true;

            //Add points to the player for the structure being removed
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
        if (type == StructureType.None) return;

        isActive = true;

        logic.ApplyGlobalEffect();

        currentPriorityToHuman = originalPriorityToHuman;
        currentPriorityToMethods = originalPriorityToMethods;

        if (effectRange > 0)
            SetAffectedByStructuresListOfCellsInRange(new Vector2Int(parentCell.X, parentCell.Y), effectRange);
    }
}
