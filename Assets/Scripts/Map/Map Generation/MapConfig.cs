using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapConfig
{
    public int width = 64;
    public int height = 64;
    public float cellSize = 1f;
    public Vector3 originPosition = Vector3.zero;
    public bool generateRandomSeed;
    public int seed;

    [Range(0.1f, 0.4f)]
    public float maxWaterRatio = 0.3f;

    [Range(0.1f, 0.4f)]
    public float maxMountainRatio = 0.2f;

    public Vector2 urbanRatioRange = new(0.1f, 0.3f);

    public int minUrbans = 2;
    public int maxUrbans = 4;
    public float minUrbanRadiusPercent = 0.03f;
    public float maxUrbanRadiusPercent = 0.8f;

    public int minMountainChains = 0;
    public int MaxMountainChains = 3;
    public float minMountainLengthPercent = 0.01f;
    public float maxMountainLengthPercent = 0.02f;
    public float minMountainRadiusPercent = 0.02f;
    public float maxMountainRadiusPercent = 0.02f;

    public float chanceLakeSpawnOnRiver = 0f;
    public int minLakeCount = 0;
    public int maxLakeCount = 5;
    public float minLakeRadiusPercent = 0.01f;
    public float maxLakeRadiusPercent = 0.04f;

    public int minRiverCount = 0;
    public int maxRiverCount = 5;
    public float minRiverLengthPercent = 0.01f;
    public float maxRiverLengthPercent = 0.05f;
    public float minRiverRadiusPercent = 0.02f;
    public float maxRiverRadiusPercent = 0.02f;

    public float removeEdgesChance = 0f;
    public float temperatureNoiseScale = 0.05f;
    public float populationNoiseScale = 0.03f;

    public int startingInfectedCellCount = 1;
    public List<CellStageData> startingInfectionLevelsList;
}
