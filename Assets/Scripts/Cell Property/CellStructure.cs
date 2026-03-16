using System.Collections.Generic;
using UnityEngine;

public class CellStructure
{
    public StructureType type;
    private bool isActive;

    [Header("Base Gameplay Values")]
    private int effectRange;
    private float currentPriorityToHuman;
    private float originalPriorityToHuman;
    private PriorityToMethods currentPriorityToMethods;
    private PriorityToMethods originalPriorityToMethods;

    private GridCell parentCell;

    public CellStructure()
    {
        isActive = false;
        type = StructureType.None;
        effectRange = 0;
        currentPriorityToHuman = originalPriorityToHuman = 0f;
        currentPriorityToMethods = originalPriorityToMethods = new PriorityToMethods();
    }

    public void SetStructure(StructureType structureType, GridCell cell)
    {
        parentCell = cell;
        type = structureType;

        StructureData structureData = CellPropertyManager.Instance.GetStructureData(type);
        if (structureData != null)
        {
            isActive = true;
            effectRange = Mathf.FloorToInt(structureData.effectRangePercent * MapManager.Instance.GetBaseSize());
            currentPriorityToHuman = originalPriorityToHuman = structureData.priorityToHuman;
            currentPriorityToMethods = originalPriorityToMethods = structureData.basePriorityToMethods;
            SetAffectedByStructuresListOfCellsInRange(new Vector2Int(cell.X, cell.Y), effectRange);
            return;
        }
    }

    public void SetAffectedByStructuresListOfCellsInRange(Vector2Int pos, int range)
    {
        Grid<GridCell> grid = MapManager.Instance.GetGrid();

        for (int i = pos.x - range; i < pos.x + range; i++)
        {
            for (int j = pos.y - range; j < pos.y + range; j++)
            {
                if (grid.IsInBounds(i, j))
                {
                    grid.GetCell(i, j).Stats.affectedByStructures.Add(type);
                }
            }
        }
    }

    public void RemoveAffectedByStructuresListOfCellsInRange(Vector2Int pos, int range)
    {
        Grid<GridCell> grid = MapManager.Instance.GetGrid();
        for (int i = pos.x - range; i < pos.x + range; i++)
        {
            for (int j = pos.y - range; j < pos.y + range; j++)
            {
                if (grid.IsInBounds(i, j))
                {
                    List<StructureType> neighbourCellAffectedByStructuresList = grid.GetCell(i, j).Stats.affectedByStructures;
                    if (neighbourCellAffectedByStructuresList.Contains(type))
                        neighbourCellAffectedByStructuresList.Remove(type);
                }
            }
        }
    }

    public void DisableStructure()
    {
        if (type == StructureType.None)
            return;

        isActive = false;

        originalPriorityToHuman = currentPriorityToHuman;
        currentPriorityToHuman = 0f;

        originalPriorityToMethods = currentPriorityToMethods;
        currentPriorityToMethods = new PriorityToMethods();

        if (effectRange > 0)
            RemoveAffectedByStructuresListOfCellsInRange(new Vector2Int(parentCell.X, parentCell.Y), effectRange);
    }

    public void EnableStructure()
    {
        if (type == StructureType.None)
            return;

        isActive = true;

        currentPriorityToHuman = originalPriorityToHuman;
        currentPriorityToMethods = originalPriorityToMethods;

        if (effectRange > 0)
            SetAffectedByStructuresListOfCellsInRange(new Vector2Int(parentCell.X, parentCell.Y), effectRange);
    }
}
