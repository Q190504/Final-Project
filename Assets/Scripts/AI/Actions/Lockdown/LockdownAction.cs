using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LockdownAction : HumanAction
{
    private LockdownActionExtraConfig extraConfig;

    private float INF_THRESHOLD;
    private float PROTECT_THRESHOLD;
    private int MAX_INFECTION_LEVEL;
    private float highPopulationWeight;
    private float MAX_PROTECTED_VALUE;
    private float MAX_CELL_SCORE;
    private float MAX_INFECTION_PRESSURE;
    private float MAX_TARGETS;

    private Dictionary<GridCell, Vector2> pressureCache = new();

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

        data.minUtility = 0;
        CalculateMaxUtility();
    }

    private void CalculateMaxUtility()
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

        // Max target infection increase
        float highestTargetInfectionIncreasePercent = 0f;
        foreach (var stage in PropertyDataManager.Instance.GetCellStageDatas())
        {
            highestTargetInfectionIncreasePercent = Mathf.Max(
                highestTargetInfectionIncreasePercent,
                stage.cellStageStats.targetInfectionIncreasePercent);
        }

        // Max cell score
        float maxSpreadPotential = MAX_INFECTION_LEVEL * highestTargetInfectionIncreasePercent * extraConfig.SpreadPotentialWeight;

        float maxSafeNeighborPopulation = highPopulationWeight * 8f
            * extraConfig.SafeNeighborsPopulationWeight;

        MAX_CELL_SCORE = maxSpreadPotential + maxSafeNeighborPopulation + highestStructurePriority;

        // Max infection pressure
        int radius = extraConfig.InfectionPressureRadius;
        float maxPressure = 0f;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > radius)
                    continue;

                maxPressure += MAX_INFECTION_LEVEL / (dist * dist);
            }
        }

        MAX_INFECTION_PRESSURE = maxPressure;

        // Max protected value
        float maxCellProtectValue = highPopulationWeight * MAX_INFECTION_LEVEL + highestStructurePriority;

        MAX_PROTECTED_VALUE = data.maxTargetPerExecution * 8f * maxCellProtectValue;

        // Max utility
        float best = 0f;

        for (int k = 0; k <= data.maxTargetPerExecution; k++)
        {
            float regionUtility =
                k > 0
                ? extraConfig.InfectionPressureWeight
                + extraConfig.RegionCompletionWeight
                + extraConfig.RegionProtectionWeight
                + extraConfig.TargetsScoreWeight
                + extraConfig.SealBonus
                : 0f;

            float frontlineUtility =
                (MAX_CELL_SCORE + MAX_INFECTION_PRESSURE)
                * (data.maxTargetPerExecution - k);

            float total = regionUtility + frontlineUtility;

            best = Mathf.Max(best, total);
        }

        data.maxUtility = best;
    }

    public override float EvaluateCell(GridCell cell, AIContext ctx)
    {
        var stats = cell.Stats;

        if (stats.isLockdown || stats.environment.currentEnvironmentType == EnvironmentType.Mountain) return 0f;

        float infecitonValue = EstimateInfection(cell);
        float spreadPotential = infecitonValue * stats.targetInfectionIncreasePercent * extraConfig.SpreadPotentialWeight;

        float safeNeighborsPopulationScore = SafeNeighborsPopulationScore(cell) * extraConfig.SafeNeighborsPopulationWeight;

        float structureBonus = stats.structure.type != StructureType.None ? stats.structure.currentPriorityToHuman : 0f;

        return
        spreadPotential
        + safeNeighborsPopulationScore
        + structureBonus;
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

    private Vector2 CalculateInfectionPressure(GridCell cell)
    {
        if (pressureCache.TryGetValue(cell, out Vector2 cached))
            return cached;

        Vector2 pressure = Vector2.zero;

        foreach (GridCell other in grid.GetNeighborsInRange(cell, extraConfig.InfectionPressureRadius))
        {
            float inf = EstimateInfection(other);

            if (inf <= INF_THRESHOLD)
                continue;

            Vector2 dir = new(cell.X - other.X, cell.Y - other.Y);

            float dist = dir.magnitude;

            if (dist < 0.01f)
                continue;

            float strength = inf / (dist * dist);

            pressure += dir.normalized * strength;
        }

        pressureCache[cell] = pressure;

        return pressure;
    }

    public override ActionInstance BuildBestInstances(AIContext ctx, SimulationCache simCache)
    {
        pressureCache.Clear();
        HashSet<GridCell> finalTargets = new();
        int targetCount = Mathf.RoundToInt(Mathf.Lerp(data.minTargetPerExecution, MAX_TARGETS, ctx.ThreatLevel));
        bool allProtected = false;

        List<NeedToBeProtectedRegion> regions = BuildNeedToBeProtectedRegions();
        Debug.Log($"regions count: {regions.Count}");

        if (regions.Count > 0)
        {
            allProtected = regions.All(r => HasClosedBoundary(r, simCache));

            if (!allProtected)
            {
                Debug.Log("Not all regions are protected, building region chains...");

                BuildRegionChains(regions, finalTargets, targetCount, simCache, ctx);

                Debug.Log($"finalTargets count after BuildRegionChains: {finalTargets.Count}");

                allProtected = regions.All(r => HasClosedBoundary(r, simCache, finalTargets));
            }
        }

        int remainingTargets = targetCount - finalTargets.Count;

        if (remainingTargets > 0)
        {
            Debug.Log(allProtected ? "Building frontline lockdowns for remaining targets..."
                : "No region to protect, building frontline lockdowns...");

            BuildFrontlineSafeLockdowns(finalTargets, remainingTargets, ctx, simCache);

            Debug.Log($"finalTargets count after BuildFrontlineSafeLockdowns: {finalTargets.Count}");
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

        List<GridCell> frontlineTargets = finalTargets
            .Where(t => !regionCells.Contains(t))
            .ToList();

        if (frontlineTargets.Count > 0)
        {
            raw += CalculateFrontlineRawUtility(frontlineTargets, ctx);
        }

        #endregion

        return new ActionInstance(this, finalTargets.ToList(), raw, NormalizeUtility(raw));
    }

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

    private void BuildFrontlineSafeLockdowns(HashSet<GridCell> targets, int maxTargets, AIContext ctx, SimulationCache simCache)
    {
        List<GridCell> candidates =
            cellList
            .Where(c =>
                !c.Stats.isLockdown
                && !simCache.IsLockdowned(c)
                && EstimateInfection(c) <= INF_THRESHOLD
                && grid
                    .GetNeighborsInRange(c, 1)
                    .Any(n => EstimateInfection(n) > INF_THRESHOLD))
            .OrderByDescending(c => EvaluateCell(c, ctx))
            .ToList();

        foreach (var c in candidates)
        {
            if (targets.Count >= maxTargets)
                break;

            targets.Add(c);
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

            // commit as much as needed
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

                float score = EvaluateCell(cell, ctx);

                int adjacency = CountAdjacent(cell, finalTargets);
                // prioritize cells that connect to existing chain
                score += adjacency * extraConfig.AdjacentToExistingLockdownBonus;
                // prioritize high pressure cells
                score += CalculateInfectionPressure(cell).magnitude;
                // prioritize uncovered boundary gaps
                score += CalculateGapClosingScore(cell, region, simCache);

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
            infectionPressure += CalculateInfectionPressure(boundary).magnitude;
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

    private float CalculateProtectValue(GridCell c)
    {
        CellStats stats = c.Stats;
        return stats.population.weight * (MAX_INFECTION_LEVEL - EstimateInfection(c))
            + (stats.structure.type != StructureType.None ? stats.structure.currentPriorityToHuman : 0);
    }

    //private float CalculateInfectionFacingScore(GridCell cell, NeedToBeProtectedRegion region, SimulationCache simCache)
    //{
    //    Vector2 pressure = CalculateInfectionPressure(cell);

    //    if (pressure.sqrMagnitude < 0.001f)
    //        return 0f;

    //    Vector2 toBoundary = Vector2.zero;

    //    foreach (GridCell boundaryCell in region.BoundaryCells)
    //    {
    //        Vector2 dir = new(boundaryCell.X - cell.X, boundaryCell.Y - cell.Y);

    //        float sqrDist = dir.sqrMagnitude;

    //        if (sqrDist < 0.001f)
    //            continue;

    //        // near boundary => more effective
    //        toBoundary += dir.normalized / sqrDist;
    //    }

    //    if (toBoundary.sqrMagnitude < 0.001f)
    //        return 0f;

    //    pressure.Normalize();
    //    toBoundary.Normalize();

    //    // pressure towards boundary => high score
    //    return Mathf.Max(0f, Vector2.Dot(pressure, toBoundary));
    //}

    private int CountAdjacent(GridCell c, HashSet<GridCell> chain)
    {
        int count = 0;
        foreach (var n in grid.GetNeighborsInRange(c, 1))
            if (chain.Contains(n)) count++;
        return count;
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
        float infectionPressure = 0;
        foreach (var c in targets)
        {
            targetsScore += EvaluateCell(c, ctx);
            infectionPressure += CalculateInfectionPressure(c).magnitude;
        }
        float normalizedTargetsScore = targets.Count > 0 ? targetsScore / (MAX_CELL_SCORE * targets.Count) : 0f;
        float normalizedPressure = targets.Count > 0 ? infectionPressure / (MAX_INFECTION_PRESSURE * targets.Count) : 0f;

        return
            +normalizedPressure * extraConfig.InfectionPressureWeight
            + completionBonus * extraConfig.RegionCompletionWeight
            + normalizedProtected * extraConfig.RegionProtectionWeight
            + normalizedTargetsScore * extraConfig.TargetsScoreWeight
            + sealBonus;
    }

    private float CalculateFrontlineRawUtility(List<GridCell> targets, AIContext ctx)
    {
        float utility = 0f;

        foreach (GridCell cell in targets)
        {
            float score = EvaluateCell(cell, ctx);

            utility += score + CalculateInfectionPressure(cell).magnitude;
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
                extraConfig.lockdownDuration, ctx);
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