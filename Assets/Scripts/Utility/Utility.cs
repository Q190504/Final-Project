using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Utility
{
    public const int minInfectionLevel = 0;
    public const int maxInfectionLevel = 100;

    public const int minInfectionResistance = 0;
    public const int maxInfectionResistance = 100;

    public const int minSterilizationResistance = 0;
    public const int maxSterilizationResistance = 100;

    public const float minTargetInfectionIncreasePercent = 0f;
    public const float maxTargetInfectionIncreasePercent = 1f;

    public const float minDetection = 0f;
    public const float maxDetection = 1f;

    public const int minAdditionalCarrierSpreadChance = 0;
    public const int maxAdditionalCarrierSpreadChance = 100;

    public const float minThreatLevel = 0f;
    public const float maxThreatLevel = 1f;

    public static readonly Vector2Int[] Neighbor8Directions =
    {
        new Vector2Int(-1, -1),
        new Vector2Int( 0, -1),
        new Vector2Int( 1, -1),
        new Vector2Int(-1,  0),
        new Vector2Int( 1,  0),
        new Vector2Int(-1,  1),
        new Vector2Int( 0,  1),
        new Vector2Int( 1,  1),
    };

    public static readonly Vector2Int[] NeighborCardinalDirections =
    {
        new Vector2Int( 0, -1),
        new Vector2Int(-1,  0),
        new Vector2Int( 1,  0),
        new Vector2Int( 0,  1),
    };

    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 pos = Input.mousePosition;
        pos.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(pos);
    }

    public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
    {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }

    public static bool IsPointerOverPanel(List<RectTransform> targets)
    {
        PointerEventData pointerData = new(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            foreach (RectTransform target in targets)
            {
                if (result.gameObject.transform == target ||
                    result.gameObject.transform.IsChildOf(target))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static Vector3 GridToWorldPosition(
        int x, int y,
        int width, int height,
        float cellSize,
        Vector3 origin)
    {
        float offsetX = (width - 1) * cellSize * 0.5f;
        float offsetY = (height - 1) * cellSize * 0.5f;

        return origin + new Vector3(
            x * cellSize - offsetX,
            y * cellSize - offsetY,
            0f
        );
    }

    public static Vector2Int WorldToGridPosition(
        Vector3 worldPos,
        int width,
        int height,
        float cellSize,
        Vector3 origin)
    {
        float offsetX = (width - 1) * cellSize * 0.5f;
        float offsetY = (height - 1) * cellSize * 0.5f;

        Vector3 local = worldPos - origin;

        int x = Mathf.FloorToInt((local.x + offsetX + cellSize * 0.5f) / cellSize);
        int y = Mathf.FloorToInt((local.y + offsetY + cellSize * 0.5f) / cellSize);

        return new Vector2Int(x, y);
    }

    public static int GetCellIndex(int x, int y, int width)
    {
        return x + y * width;
    }

    public static List<Vector2Int> Get8Neighbors(Grid<GridCell> grid, Vector2Int pos)
    {
        List<Vector2Int> neighbors = new(Neighbor8Directions.Length);

        foreach (var dir in Neighbor8Directions)
        {
            Vector2Int neighborPos = pos + dir;

            if (!grid.IsInBounds(neighborPos.x, neighborPos.y))
                continue;

            neighbors.Add(neighborPos);
        }

        return neighbors;
    }

    public static void Get8Neighbors(Grid<GridCell> grid, Vector2Int pos, List<Vector2Int> result)
    {
        result.Clear();

        foreach (var dir in Neighbor8Directions)
        {
            Vector2Int neighborPos = pos + dir;

            if (!grid.IsInBounds(neighborPos.x, neighborPos.y))
                continue;

            result.Add(neighborPos);
        }
    }

    public static Vector2Int GetRandomPosition(Grid<GridCell> grid, System.Random random)
    {
        return new Vector2Int(
            random.Next(0, grid.GetWidth() - 1),
            random.Next(0, grid.GetHeight() - 1));
    }

    public static Vector2Int GetRandomCardinalDirection(System.Random random)
    {
        int dir = random.Next(0, 4);

        return dir switch
        {
            0 => NeighborCardinalDirections[0],
            1 => NeighborCardinalDirections[1],
            2 => NeighborCardinalDirections[2],
            _ => NeighborCardinalDirections[3],
        };
    }

    public static Vector2Int GetRandom8Direction(System.Random random)
    {
        int dir = random.Next(0, 8);

        return dir switch
        {
            0 => Neighbor8Directions[0],
            1 => Neighbor8Directions[1],
            2 => Neighbor8Directions[2],
            3 => Neighbor8Directions[3],
            4 => Neighbor8Directions[4],
            5 => Neighbor8Directions[5],
            6 => Neighbor8Directions[6],
            _ => Neighbor8Directions[7],
        };
    }

    public static bool IsEnviromentMatch(CellStats stats, List<EnvironmentType> environmentTypes)
    {
        foreach (var envType in environmentTypes)
        {
            if (stats.environment.currentEnvironmentType == envType)
                return true;
        }
        return false;
    }

    public static bool IsUrban(EnvironmentType type)
    {
        return type == EnvironmentType.NormalUrban
            || type == EnvironmentType.HotUrban
            || type == EnvironmentType.ColdUrban;
    }

    public static void Shuffle<T>(List<T> list, System.Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1); // 0 <= j <= i
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public static int GetDistance(GridCell a, GridCell b)
    {
        return Mathf.Max(Mathf.Abs(a.X - b.X), Mathf.Abs(a.Y - b.Y));
    }
}