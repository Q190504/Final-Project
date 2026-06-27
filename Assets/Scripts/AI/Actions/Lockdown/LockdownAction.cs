using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class LockdownAnalysisCache
{
    public Dictionary<GridCell, int> DistanceToInfection = new();

    public Dictionary<GridCell, int> ComponentId = new();

    public HashSet<GridCell> ArticulationCells = new();

    public Dictionary<GridCell, float> ChokePointScore = new();

    public Dictionary<int, float> ComponentValue = new();

    public float TotalProtectValue = 0;
}

public class LockdownAction : HumanAction
{
    private LockdownActionExtraConfig extraConfig;

    private float INF_THRESHOLD;
    private float PROTECT_THRESHOLD;
    private int MAX_INFECTION_LEVEL;
    private float highPopulationWeight;
    private float MAX_PROTECTED_VALUE;
    private float MAX_CELL_SCORE;
    private float MIN_CELL_SCORE;
    private float MAX_INFECTION_PRESSURE;

    private float MAX_BOUNDARY_CELL_SCORE;

    private bool isBuildForRegion = true;

    private List<NeedToBeProtectedRegion> regions = new();
    private Dictionary<GridCell, Vector2> pressureCache = new();

    private LockdownAnalysisCache analysisCache;
    private Dictionary<GridCell, int> disc = new();
    private Dictionary<GridCell, int> low = new();

    private int dfsTime;

    private MapManager mapManager;
    private Grid<GridCell> grid;
    private GridCell[,] gridArray;
    private List<GridCell> cellList = new();

    public LockdownAction(HumanActionSO data, LockdownActionExtraConfig extraConfig) : base(data)
    {
        this.extraConfig = extraConfig;
        mapManager = MapManager.Instance;
        grid = mapManager.GetGrid();
        gridArray = grid.GetGrid();
        foreach (GridCell cell in gridArray)
            cellList.Add(cell);

        MAX_INFECTION_LEVEL = Utility.maxInfectionLevel;

        CalculateMinANdMaxUtility();
    }
    private void CalculateMinANdMaxUtility()
    {
        CellStageData safeStageData = PropertyDataManager.Instance.GetCellStageData(CellStageType.Safe);
        INF_THRESHOLD = safeStageData.cellStageStats.maxInfectionValue;

        PopulationData highPopulationData = PropertyDataManager.Instance.GetPopulationData(PopulationType.High);
        highPopulationWeight = highPopulationData.weight;

        PROTECT_THRESHOLD = highPopulationWeight * (MAX_INFECTION_LEVEL - INF_THRESHOLD);

        // Max structure priority
        float highestStructurePriority = 0f;
        foreach (var s in PropertyDataManager.Instance.GetStructureDatas())
        {
            highestStructurePriority = Mathf.Max(highestStructurePriority, s.priorityToHuman);
        }

        float maxSafeNeighborPopulation = highPopulationWeight * 8f
            * extraConfig.SafeNeighborsPopulationWeight;

        // Max infection pressure
        int radius = extraConfig.DetectRadius;
        float maxPressure = 0f;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int dist = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));

                maxPressure += MAX_INFECTION_LEVEL / (dist * dist);
            }
        }

        MAX_INFECTION_PRESSURE = maxPressure;

        int maxNeighbors = 8;
        int maxFrontierStrength = (maxNeighbors / 2) * (maxNeighbors - (maxNeighbors / 2));
        float maxFrontierScore = maxFrontierStrength * extraConfig.FrontierCellWeight;

        MIN_CELL_SCORE = -extraConfig.CurrentCellInfectionPenaltyWeight;

        MAX_CELL_SCORE = Mathf.Max(MAX_INFECTION_PRESSURE * extraConfig.InfectionPressureWeight, maxFrontierScore)
            + maxSafeNeighborPopulation * extraConfig.SafeNeighborsPopulationWeight
            + highestStructurePriority;

        float maxGapClosingScore = maxNeighbors * 10f;
        float maxThreatScore = 1f;
        float maxChokePointScore = 1f;
        MAX_BOUNDARY_CELL_SCORE = maxThreatScore + maxGapClosingScore + maxChokePointScore;

        // Max protected value
        float maxCellProtectValue = highPopulationWeight * MAX_INFECTION_LEVEL + highestStructurePriority;

        MAX_PROTECTED_VALUE = data.maxTargetPerExecution * 8f * maxCellProtectValue;

        // Min utility
        data.minUtility = MIN_CELL_SCORE * data.maxTargetPerExecution;

        // Max utility
        float best = 0f;

        for (int k = 0; k <= data.maxTargetPerExecution; k++)
        {
            float regionUtility =
                k > 0
                ? extraConfig.RegionCompletionWeight
                + extraConfig.RegionProtectionWeight
                + extraConfig.TargetsScoreWeight
                + extraConfig.SealBonus
                : 0f;

            float frontlineUtility = MAX_BOUNDARY_CELL_SCORE * (data.maxTargetPerExecution - k);

            float total = regionUtility + frontlineUtility;

            best = Mathf.Max(best, total);
        }

        data.maxUtility = best;

        //Debug.Log($"min {data.minUtility}");
        //Debug.Log($"max {data.maxUtility}");
    }

    public override float EvaluateCell(GridCell cell, AIContext ctx)
    {
        var stats = cell.Stats;

        if (!isBuildForRegion)
        {
            foreach (NeedToBeProtectedRegion region in regions)
                if (region.BoundaryCells.Contains(cell))
                    return 0f;
        }

        if (stats.isLockdown ||
            stats.environment.currentEnvironmentType == EnvironmentType.Mountain)
            return 0f;

        float safeNeighborsPopulationScore = SafeNeighborsPopulationScore(cell) * extraConfig.SafeNeighborsPopulationWeight;

        float structureBonus = stats.structure.type != StructureType.None ? stats.structure.currentPriorityToHuman : 0f;

        float infectionPenalty = EstimateInfection(cell) * EstimateInfection(cell) * extraConfig.CurrentCellInfectionPenaltyWeight;

        if (isBuildForRegion)
        {
            float infectionPressure = CalculateInfectionPressure(cell) * extraConfig.InfectionPressureWeight;

            return infectionPressure
            + safeNeighborsPopulationScore
            + structureBonus
            - infectionPenalty;
        }
        else
        {
            float frontierScore = CalculateFrontierScore(cell) * extraConfig.FrontierCellWeight;

            return frontierScore
            + safeNeighborsPopulationScore
            + structureBonus
            - infectionPenalty;
        }
    }

    private float EstimateInfection(GridCell c)
    {
        CellStats cellStats = c.Stats;
        if (cellStats.isDetected) return cellStats.infectionLevel;

        float sum = 0;
        float weightSum = 0;

        foreach (var n in grid.GetNeighborsInRange(c, 1))
        {
            CellStats neighbourStats = n.Stats;
            float w = neighbourStats.isDetected ? 1f : extraConfig.UndetectedCellWeight;
            sum += neighbourStats.infectionLevel * w;
            weightSum += w;
        }

        return (weightSum > 0) ? sum / weightSum : 0;
    }

    private float SafeNeighborsPopulationScore(GridCell c)
    {
        float weight = 0;
        foreach (var n in grid.GetNeighborsInRange(c, 1))
        {
            CellStats neighborStats = n.Stats;
            if (neighborStats.isDetected)
            {
                if (!neighborStats.isContagious)
                    weight += neighborStats.population.weight;
            }
            else
            {
                weight += neighborStats.population.weight * extraConfig.UndetectedCellWeight;
            }
        }

        return weight;
    }

    private float CalculateInfectionPressure(GridCell cell)
    {
        float pressure = 0f;

        foreach (GridCell other in grid.GetNeighborsInRange(cell, extraConfig.DetectRadius))
        {
            if (other == cell)
                continue;

            float infection = EstimateInfection(other);

            if (infection <= INF_THRESHOLD)
                continue;

            float dist = Vector2Int.Distance(new Vector2Int(cell.X, cell.Y), new Vector2Int(other.X, other.Y));

            pressure += infection / (dist * dist);
        }

        return pressure;
    }

    // If cell is safe & near infected cells -> high score
    private float CalculateFrontierScore(GridCell cell)
    {
        int infected = 0;
        int safe = 0;

        foreach (var n in grid.GetNeighborsInRange(cell, 1))
        {
            if (EstimateInfection(n) > INF_THRESHOLD)
                infected++;
            else
                safe++;
        }

        if (infected == 0 || safe == 0)
            return 0f;

        float frontierStrength = infected * safe;

        float infectionFactor = 1f - EstimateInfection(cell) / MAX_INFECTION_LEVEL;

        return frontierStrength * infectionFactor;
    }

    public override ActionInstance BuildBestInstances(AIContext ctx, SimulationCache simCache)
    {
        HashSet<GridCell> finalTargets = new();
        int finalTargetCount = Mathf.FloorToInt(Mathf.Lerp(data.minTargetPerExecution, data.maxTargetPerExecution, ctx.ThreatLevel));

        BuildAnalysisCache();

        BuildNeedToBeProtectedRegions();

        bool allProtected = false;

        regions = BuildNeedToBeProtectedRegions();
        Debug.Log($"regions count: {regions.Count}");

        if (regions.Count > 0)
        {
            allProtected = regions.All(r => HasClosedBoundary(r, simCache));

            if (!allProtected)
            {
                Debug.Log("Not all regions are protected, building region chains...");

                int regionsTargetsCount = finalTargetCount / 2;
                BuildRegionChains(regions, finalTargets, regionsTargetsCount, simCache, ctx);

                Debug.Log($"finalTargets count after BuildRegionChains: {finalTargets.Count}");

                allProtected = regions.All(r => HasClosedBoundary(r, simCache, finalTargets));
            }
        }

        int containmentTargetCount = finalTargetCount - finalTargets.Count;

        if (containmentTargetCount > 0)
        {
            Debug.Log(allProtected ? "Building containment lockdowns for remaining targets..."
                : "No region to protect, building containment lockdowns...");

            BuildContainmentLockdowns(finalTargets, containmentTargetCount, ctx, simCache);

            Debug.Log($"finalTargets count after BuildContainmentLockdowns: {finalTargets.Count}");
        }

        if (finalTargets.Count == 0)
        {
            return new ActionInstance(this)
            {
                IsValid = false
            };
        }

        float raw = 0f;
        HashSet<GridCell> regionCells = new();

        #region Region utility

        foreach (var region in regions)
        {
            List<GridCell> regionTargets = finalTargets
                    .Where(t => region.BoundaryCells.Contains(t))
                    .ToList();

            if (regionTargets.Count == 0)
                continue;

            raw += CalculateRegionRawUtility(regionTargets, region, ctx, simCache);

            foreach (var c in regionTargets)
            {
                regionCells.Add(c);
            }
        }

        #endregion

        #region Frontline utility

        List<GridCell> containmentTargets = finalTargets
            .Where(t => !regionCells.Contains(t))
            .ToList();

        if (containmentTargets.Count > 0)
        {
            raw += CalculateFrontlineRawUtility(containmentTargets, ctx);
        }

        #endregion

        return new ActionInstance(this, finalTargets.ToList(), raw, NormalizeUtility(raw));
    }

    #region Build Cache

    private void BuildAnalysisCache()
    {
        analysisCache = new();

        BuildDistanceMap();

        BuildConnectedComponents();

        BuildArticulationPoints();

        CalculateChokeScores();
    }

    private void BuildDistanceMap()
    {
        Queue<GridCell> queue = new();

        foreach (GridCell cell in cellList)
        {
            if (EstimateInfection(cell) > INF_THRESHOLD)
            {
                analysisCache.DistanceToInfection[cell] = 0;
                queue.Enqueue(cell);
            }
            else
            {
                analysisCache.DistanceToInfection[cell] = int.MaxValue;
            }
        }

        while (queue.Count > 0)
        {
            GridCell current = queue.Dequeue();

            int currentDist = analysisCache.DistanceToInfection[current];

            foreach (GridCell neighbour in grid.GetNeighborsInRange(current, 1))
            {
                if (neighbour.Stats.isLockdown)
                    continue;

                int newDist = currentDist + 1;

                if (newDist >= analysisCache.DistanceToInfection[neighbour])
                    continue;

                analysisCache.DistanceToInfection[neighbour] = newDist;

                queue.Enqueue(neighbour);
            }
        }
    }

    private void BuildConnectedComponents()
    {
        int nextId = 0;
        analysisCache.TotalProtectValue = 0;

        HashSet<GridCell> visited = new();

        foreach (GridCell cell in cellList)
        {
            if (visited.Contains(cell))
                continue;

            if (cell.Stats.isLockdown)
                continue;

            float componentValue = 0;

            Queue<GridCell> queue = new();

            queue.Enqueue(cell);

            visited.Add(cell);

            while (queue.Count > 0)
            {
                GridCell current = queue.Dequeue();

                analysisCache.ComponentId[current] = nextId;

                componentValue += CalculateProtectValue(current);

                foreach (GridCell neighbour in grid.GetNeighborsInRange(current, 1))
                {
                    if (visited.Contains(neighbour))
                        continue;

                    if (neighbour.Stats.isLockdown)
                        continue;

                    visited.Add(neighbour);

                    queue.Enqueue(neighbour);
                }
            }

            analysisCache.ComponentValue[nextId] = componentValue;
            analysisCache.TotalProtectValue += componentValue;

            nextId++;
        }
    }

    private void BuildArticulationPoints()
    {
        disc.Clear();
        low.Clear();

        dfsTime = 0;

        foreach (GridCell cell in cellList)
        {
            if (cell.Stats.isLockdown)
                continue;

            if (!disc.ContainsKey(cell))
            {
                TarjanDFS(cell, null);
            }
        }
    }

    private void TarjanDFS(GridCell current, GridCell parent)
    {
        disc[current] = low[current] = ++dfsTime;

        int children = 0;

        foreach (GridCell neighbour in grid.GetNeighborsInRange(current, 1))
        {
            if (neighbour.Stats.isLockdown)
                continue;

            if (!disc.ContainsKey(neighbour))
            {
                children++;

                TarjanDFS(neighbour, current);

                low[current] = Mathf.Min(low[current], low[neighbour]);

                if (parent != null && low[neighbour] >= disc[current])
                {
                    analysisCache.ArticulationCells.Add(current);
                }
            }
            else if (neighbour != parent)
            {
                low[current] = Mathf.Min(low[current], disc[neighbour]);
            }
        }

        if (parent == null && children > 1)
        {
            analysisCache.ArticulationCells.Add(current);
        }
    }

    private void CalculateChokeScores()
    {
        foreach (GridCell cell in analysisCache.ArticulationCells)
        {
            float score = 0;
            HashSet<int> connectedComponents = new();

            foreach (GridCell neighbor in grid.GetNeighborsInRange(cell, 1))
            {
                if (!analysisCache.ComponentId.ContainsKey(neighbor))
                    continue;

                int component = analysisCache.ComponentId[neighbor];
                connectedComponents.Add(component);
            }

            foreach (int component in connectedComponents)
            {
                score += analysisCache.ComponentValue[component];
            }

            analysisCache.ChokePointScore[cell] = score / analysisCache.TotalProtectValue;
        }
    }

    #endregion

    #region Build Regions

    private List<NeedToBeProtectedRegion> BuildNeedToBeProtectedRegions()
    {
        List<NeedToBeProtectedRegion> regions = new();
        HashSet<GridCell> visited = new();

        BuildHighPriorityRegions(regions, visited);
        BuildStructureOnlyRegions(regions, visited);

        return regions;
    }

    private void BuildHighPriorityRegions(List<NeedToBeProtectedRegion> regions, HashSet<GridCell> visited)
    {
        List<GridCell> seeds = cellList
            .Where(c => !visited.Contains(c)
            && c.Stats.stage.type != CellStageType.Dead
            && CalculateProtectValue(c) >= PROTECT_THRESHOLD)
            .ToList();

        foreach (GridCell seed in seeds)
        {
            if (visited.Contains(seed))
                continue;

            NeedToBeProtectedRegion region = new();
            Queue<GridCell> queue = new();

            queue.Enqueue(seed);
            visited.Add(seed);

            while (queue.Count > 0)
            {
                GridCell cell = queue.Dequeue();

                region.Cells.Add(cell);
                region.TotalProtectValue += CalculateProtectValue(cell);

                foreach (GridCell neighbor in grid.GetNeighborsInRange(cell, 1))
                {
                    if (visited.Contains(neighbor))
                        continue;

                    CellStats neighborStats = neighbor.Stats;

                    if (neighborStats.stage.type == CellStageType.Dead)
                        continue;

                    bool isHighProtect = CalculateProtectValue(neighbor) >= PROTECT_THRESHOLD;

                    bool hasStructure = neighborStats.structure.type != StructureType.None;

                    if (!isHighProtect && !hasStructure)
                        continue;

                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            if (region.Cells.Count == 0)
                continue;

            ComputeBoundary(region);
            regions.Add(region);
        }
    }

    private void BuildStructureOnlyRegions(List<NeedToBeProtectedRegion> regions, HashSet<GridCell> visited)
    {
        List<GridCell> seeds = cellList
            .Where(c => !visited.Contains(c)
                && c.Stats.stage.type != CellStageType.Dead
                && c.Stats.structure.type != StructureType.None
                && CalculateProtectValue(c) < PROTECT_THRESHOLD)
            .ToList();

        foreach (GridCell seed in seeds)
        {
            if (visited.Contains(seed))
                continue;

            NeedToBeProtectedRegion region = new();
            Queue<GridCell> queue = new();

            queue.Enqueue(seed);
            visited.Add(seed);

            while (queue.Count > 0)
            {
                GridCell cell = queue.Dequeue();

                region.Cells.Add(cell);
                region.TotalProtectValue += CalculateProtectValue(cell);

                foreach (GridCell neighbor in grid.GetNeighborsInRange(cell, 1))
                {
                    if (visited.Contains(neighbor))
                        continue;

                    CellStats neighborStats = neighbor.Stats;

                    if (neighborStats.stage.type == CellStageType.Dead)
                        continue;

                    bool isStructureOnly = neighborStats.structure.type != StructureType.None
                        && CalculateProtectValue(neighbor) < PROTECT_THRESHOLD;

                    if (!isStructureOnly)
                        continue;

                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            if (region.Cells.Count == 0)
                continue;

            ComputeBoundary(region);
            regions.Add(region);
        }
    }

    private void ComputeBoundary(NeedToBeProtectedRegion region)
    {
        region.BoundaryCells.Clear();

        HashSet<GridCell> regionCells = region.Cells.ToHashSet();

        foreach (GridCell regionCell in region.Cells)
        {
            foreach (GridCell neighbor in grid.GetNeighborsInRange(regionCell, 1))
            {
                // boundary must NOT belong to region
                if (regionCells.Contains(neighbor))
                    continue;

                //// skip infected cells
                //if (EstimateInfection(neighbor) > INF_THRESHOLD)
                //    continue;

                region.BoundaryCells.Add(neighbor);
            }
        }
    }

    private void BuildRegionChains(List<NeedToBeProtectedRegion> regions, HashSet<GridCell> finalTargets, int totalTargets, SimulationCache simCache,
    AIContext ctx)
    {
        isBuildForRegion = true;
        int remainingTargets = totalTargets;

        regions = regions
            .OrderByDescending(r => CalculateRegionPriority(r, simCache))
            .ToList();

        foreach (NeedToBeProtectedRegion region in regions)
        {
            if (remainingTargets <= 0)
                break;

            // skip sealed region
            if (HasClosedBoundary(region, simCache))
                continue;

            int before = finalTargets.Count;

            BuildChainForRegion(region, remainingTargets, finalTargets, simCache, ctx);

            int used = finalTargets.Count - before;

            remainingTargets -= used;

            // stop if exhausted
            if (remainingTargets <= 0)
                break;
        }
    }

    private void BuildChainForRegion(NeedToBeProtectedRegion region, int quota, HashSet<GridCell> finalTargets, SimulationCache simCache,
    AIContext ctx)
    {
        while (quota > 0)
        {
            GridCell best = null;
            float bestScore = float.MinValue;

            foreach (GridCell cell in region.BoundaryCells)
            {
                if (cell.Stats.isLockdown
                    || simCache.IsLockdowned(cell))
                    continue;

                if (finalTargets.Contains(cell))
                    continue;

                float score = EvaluateBoundaryCell(cell, region, simCache);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = cell;
                }
            }

            if (best == null)
                break;

            finalTargets.Add(best);
            quota--;
        }
    }

    private float EvaluateBoundaryCell(GridCell cell, NeedToBeProtectedRegion region, SimulationCache simCache)
    {
        if (EstimateInfection(cell) > INF_THRESHOLD)
            return -extraConfig.CurrentCellInfectionPenaltyWeight;

        float score = 0;

        score += CalculateGapClosingScore(cell, region, simCache);

        score += 1f / (analysisCache.DistanceToInfection[cell] + 1);

        score += analysisCache.ChokePointScore.GetValueOrDefault(cell);

        return score;
    }

    private float CalculateRegionPriority(NeedToBeProtectedRegion region, SimulationCache simCache)
    {
        if (region.BoundaryCells.Count == 0)
            return 0f;

        int lockedCount = 0;

        foreach (GridCell boundary in region.BoundaryCells)
        {
            if (boundary.Stats.isLockdown
                || simCache.IsLockdowned(boundary))
            {
                lockedCount++;
            }
        }

        float completionRatio = (float)lockedCount / region.BoundaryCells.Count;

        // nonlinear finish incentive
        float completionBonus = Mathf.Pow(completionRatio, 2f);

        float infectionPressure = 0f;

        foreach (GridCell boundary in region.BoundaryCells)
        {
            infectionPressure += CalculateInfectionPressure(boundary);
        }

        return region.TotalProtectValue
            + infectionPressure * extraConfig.InfectionPressureWeight
            + completionBonus * extraConfig.RegionCompletionWeight;
    }

    private float CalculateGapClosingScore(GridCell cell, NeedToBeProtectedRegion region, SimulationCache simCache)
    {
        int nearbyLocked = 0;

        foreach (GridCell n in grid.GetNeighborsInRange(cell, 1))
        {
            if (!region.BoundaryCells.Contains(n))
                continue;

            if (n.Stats.isLockdown || simCache.IsLockdowned(n))
            {
                nearbyLocked++;
            }
        }

        // bridge gaps
        return nearbyLocked * 10f;
    }

    private bool HasClosedBoundary(NeedToBeProtectedRegion region, SimulationCache simCache, HashSet<GridCell> additionalCells = null)
    {
        if (region.BoundaryCells.Count == 0)
            return false;

        Queue<GridCell> queue = new();
        HashSet<GridCell> visited = new();

        // --------------------
        // Local bounding box
        // --------------------
        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;

        foreach (GridCell c in region.BoundaryCells)
        {
            minX = Mathf.Min(minX, c.X);
            maxX = Mathf.Max(maxX, c.X);

            minY = Mathf.Min(minY, c.Y);
            maxY = Mathf.Max(maxY, c.Y);
        }

        const int padding = 2;

        minX = Mathf.Max(0, minX - padding);
        maxX = Mathf.Min(grid.GetWidth() - 1, maxX + padding);

        minY = Mathf.Max(0, minY - padding);
        maxY = Mathf.Min(grid.GetHeight() - 1, maxY + padding);

        // --------------------
        // Enqueue local border
        // --------------------
        for (int x = minX; x <= maxX; x++)
        {
            TryEnqueue(grid.GetCell(x, minY));

            TryEnqueue(grid.GetCell(x, maxY));
        }

        for (int y = minY; y <= maxY; y++)
        {
            TryEnqueue(grid.GetCell(minX, y));

            TryEnqueue(grid.GetCell(maxX, y));
        }

        // --------------------
        // Flood fill
        // --------------------
        while (queue.Count > 0)
        {
            GridCell current = queue.Dequeue();

            // region reachable => not sealed
            if (region.Cells.Contains(current))
            {
                return false;
            }

            foreach (GridCell neighbor in grid.GetNeighborsInRange(current, 1))
            {
                // stay inside local box
                if (neighbor.X < minX
                    || neighbor.X > maxX
                    || neighbor.Y < minY
                    || neighbor.Y > maxY)
                    continue;

                TryEnqueue(neighbor);
            }
        }

        return true;

        void TryEnqueue(GridCell cell)
        {
            if (cell == null)
                return;

            if (!visited.Add(cell))
                return;

            // lockdown = wall
            if (cell.Stats.isLockdown
                || simCache.IsLockdowned(cell))
                return;

            if (additionalCells != null && additionalCells.Contains(cell))
                return;

            // mountain = wall
            if (cell.Stats.environment.currentEnvironmentType == EnvironmentType.Mountain)
                return;

            queue.Enqueue(cell);
        }
    }

    #endregion

    #region Build For Frontline

    private void BuildContainmentLockdowns(HashSet<GridCell> finalTargets, int maxTargets, AIContext ctx, SimulationCache simCache)
    {
        isBuildForRegion = false;

        HashSet<GridCell> allBoundaryCells = new();

        if (regions != null)
        {
            foreach (NeedToBeProtectedRegion region in regions)
            {
                foreach (GridCell cell in region.BoundaryCells)
                    allBoundaryCells.Add(cell);
            }
        }

        List<GridCell> candidates = cellList
            .Where(c =>
                !c.Stats.isLockdown
                && c.Stats.environment.currentEnvironmentType != EnvironmentType.Mountain
                && !simCache.lockdownedCells.Contains(c)
                && !finalTargets.Contains(c)
                && !allBoundaryCells.Contains(c))
            .OrderByDescending(c => EvaluateContainmentCell(c))
            .ToList();

        if (candidates.Count == 0)
            return;

        for (int i = 0; i < maxTargets && i < candidates.Count; i++)
        {
            finalTargets.Add(candidates[i]);
        }
    }

    private float EvaluateContainmentCell(GridCell cell)
    {
        if (EstimateInfection(cell) > INF_THRESHOLD)
            return -extraConfig.CurrentCellInfectionPenaltyWeight;

        float choke = analysisCache.ChokePointScore.GetValueOrDefault(cell);

        float distanceToInfectionValue = 1f / (analysisCache.DistanceToInfection[cell] + 1);

        float componentValue = analysisCache.ComponentValue[analysisCache.ComponentId[cell]];

        return
            choke * extraConfig.ChokePointWeight +
            distanceToInfectionValue * extraConfig.DistanceToInfectionWeight +
            componentValue * extraConfig.ComponentValueWeight;
    }

    #endregion

    private float CalculateProtectValue(GridCell c)
    {
        CellStats stats = c.Stats;
        return stats.population.weight * (MAX_INFECTION_LEVEL - EstimateInfection(c))
            + (stats.structure.type != StructureType.None ? stats.structure.currentPriorityToHuman : 0);
    }

    public override float CalculateRawUtility(List<GridCell> targets, AIContext ctx)
    {
        return 0f;
    }

    private float CalculateRegionRawUtility(List<GridCell> targets, NeedToBeProtectedRegion region, AIContext ctx, SimulationCache simCache)
    {
        float completionBonus = CalculateCompletionBonus(region, targets, simCache);

        bool willBeSealed = WillBeSealed(region, targets, simCache);
        float sealBonus = willBeSealed ? extraConfig.SealBonus : 0f;

        float protectedValue = CalculateProtectedValue(targets, region);
        float normalizedProtected = protectedValue / MAX_PROTECTED_VALUE;

        float targetsScore = 0f;
        foreach (var c in targets)
        {
            targetsScore += EvaluateBoundaryCell(c, region, simCache);
        }

        float normalizedTargetsScore = targets.Count > 0 ? targetsScore / (MAX_BOUNDARY_CELL_SCORE * targets.Count) : 0f;

        return
            +completionBonus * extraConfig.RegionCompletionWeight
            + normalizedProtected * extraConfig.RegionProtectionWeight
            + normalizedTargetsScore * extraConfig.TargetsScoreWeight
            + sealBonus;
    }

    private float CalculateFrontlineRawUtility(List<GridCell> targets, AIContext ctx)
    {
        float utility = 0f;
        foreach (GridCell cell in targets)
        {
            utility += EvaluateContainmentCell(cell);
        }

        return utility;
    }

    private float CalculateCompletionBonus(NeedToBeProtectedRegion region, List<GridCell> targets, SimulationCache simCache)
    {
        int coveredBefore = 0;
        int coveredAfter = 0;

        foreach (GridCell boundaryCell in region.BoundaryCells)
        {
            bool before = boundaryCell.Stats.isLockdown
                || simCache.IsLockdowned(boundaryCell);

            bool after = before
                || targets.Contains(boundaryCell);

            if (before)
                coveredBefore++;

            if (after)
                coveredAfter++;
        }

        float beforeRatio = (float)coveredBefore / region.BoundaryCells.Count;
        float afterRatio = (float)coveredAfter / region.BoundaryCells.Count;

        float beforeValue = Mathf.Pow(beforeRatio, 2f);
        float afterValue = Mathf.Pow(afterRatio, 2f);

        return Mathf.Max(0f, afterValue - beforeValue);
    }

    private bool WillBeSealed(NeedToBeProtectedRegion region, List<GridCell> targets, SimulationCache simCache)
    {
        bool before = HasClosedBoundary(region, simCache);

        SimulationCache fake = simCache.Clone();

        foreach (var c in targets)
        {
            fake.AddLockdowned(c);
        }

        bool after = HasClosedBoundary(region, fake);

        return !before && after;
    }

    private float CalculateProtectedValue(List<GridCell> targets, NeedToBeProtectedRegion region)
    {
        float value = 0f;

        HashSet<GridCell> visited = new();

        foreach (GridCell target in targets)
        {
            foreach (GridCell n in grid.GetNeighborsInRange(target, 1))
            {
                if (!region.Cells.Contains(n))
                    continue;

                if (!visited.Add(n))
                    continue;

                value += CalculateProtectValue(n);
            }
        }

        return value;
    }

    public override void Execute(List<GridCell> cells, AIContext ctx, ThreatTier reducedCooldownTier, float reducedCooldownModifier)
    {
        foreach (GridCell cell in cells)
        {
            cell.Stats.SetIsLockDown(true, extraConfig.InfectionResistanceIncreasedAfterLockdown,
                extraConfig.LockdownDuration, ctx);
        }

        RaiseExecuteVisual(cells, HumanActionType.Lockdown);

        base.Execute(cells, ctx, reducedCooldownTier, reducedCooldownModifier);
    }

    public override void ApplyLightSimulation(List<GridCell> targets, AIContext simCtx, SimulationCache simCache)
    {
        foreach (var cell in targets)
        {
            simCache.AddLockdowned(cell);
        }
    }
}