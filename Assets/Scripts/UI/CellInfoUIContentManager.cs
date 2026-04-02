using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CellInfoUIContentManager : MonoBehaviour
{
    public static CellInfoUIContentManager Instance;

    [SerializeField] private CellInfoUIEntry cellInfoUIEntryPrefab;
    [SerializeField] private Image background;
    [SerializeField] private Transform container;
    [SerializeField] private bool showDebug;

    [Header("Entries")]
    [SerializeField] private CellInfoUIEntry posEntry;
    [SerializeField] private CellInfoUIEntry infectionLevelEntry;
    [SerializeField] private CellInfoUIEntry stageEntry;
    [SerializeField] private CellInfoUIEntry infectionResistanceEntry;
    [SerializeField] private CellInfoUIEntry waterEntry;
    [SerializeField] private CellInfoUIEntry carrierEntry;
    [SerializeField] private CellInfoUIEntry isLockdownEntry;
    [SerializeField] private CellInfoUIEntry isContagiousEntry;
    [SerializeField] private CellInfoUIEntry isBlockedEntry;
    [SerializeField] private CellInfoUIEntry detectionValueEntry;
    [SerializeField] private CellInfoUIEntry environmentEntry;
    [SerializeField] private CellInfoUIEntry populationEntry;
    [SerializeField] private CellInfoUIEntry temperatureEntry;
    [SerializeField] private CellInfoUIEntry structureEntry;
    [SerializeField] private CellInfoUIEntry nearStructuresEntry;

    [Header("Refs")]
    [SerializeField] private Sprite undetectedSprite;
    [SerializeField] private Sprite detectedSprite;

    [Header("Input")]
    [SerializeField] private InputAction getCellInfoAction;

    private CellPropertyManager cellPropertyManager;
    private MapManager mapManager;

    private GridCell currentCell;


    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is enabled
    /// </summary>
    void OnEnable()
    {
        getCellInfoAction.Enable();
    }

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is disabled
    /// </summary>
    void OnDisable()
    {
        getCellInfoAction.Disable();
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (getCellInfoAction.bindings.Count == 0)
        {
            Debug.LogWarning("The Get Cell Info Action does not have a binding set! Make sure that each Input Action has a binding set or the controller will not work!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (getCellInfoAction.triggered)
        {
            Grid<GridCell> grid = mapManager.GetGrid();
            if (grid != null)
            {
                Vector3 mouseWorldPos = Utility.GetMouseWorldPosition();
                Vector2Int cellPos = Utility.WorldToGridPosition(
                    mouseWorldPos,
                    grid.GetWidth(),
                    grid.GetHeight(),
                    grid.GetCellSize(),
                    grid.GetOriginPosition()
                );

                if (grid.IsInBounds(cellPos.x, cellPos.y))
                {
                    SetVisibity(true, cellPos);
                    GridCell cell = grid.GetCell(cellPos.x, cellPos.y);
                    if (showDebug)
                        cell.DebugStats();
                }
            }
        }
    }

    public void SetVisibity(bool state, Vector2Int? cellPos = null)
    {
        if (state)
        {
            if (!cellPos.HasValue)
            {
                Debug.LogWarning("cellPos is null. Can't show cell's info.");
                return;
            }

            Grid<GridCell> grid = mapManager.GetGrid();
            if (grid == null)
            {
                Debug.LogError("Grid is null!");
                return;
            }

            if (cellPropertyManager == null)
            {
                Debug.LogError("CellPropertyManager is null!");
                return;
            }

            GridCell previousCell = currentCell;

            GridCell cell = grid.GetCell(cellPos.Value.x, cellPos.Value.y);

            if (cell?.Stats == null)
            {
                Debug.LogError("Cell or CellStats is null!");
                return;
            }

            if (currentCell != previousCell)
            {
                previousCell.IsBeingShownInfo = false;

                currentCell = cell;
                currentCell.IsBeingShownInfo = true;
            }

            CellStats cellStats = cell.Stats;

            // ===== BASIC =====
            if (showDebug) SetupEntry(posEntry, $"Pos: {cell.X}, {cell.Y}.");
            SetupEntry(infectionLevelEntry, $"Infection Level: {cellStats.infectionLevel}.");

            CellStageData stageData = cellPropertyManager.GetCellStageData(cellStats.stage.type);
            SetupEntry(stageEntry, Safe(stageData != null ? $"Stage: {stageData.displayName}." : null), stageData != null ? stageData.sprite : null);

            SetupEntry(infectionResistanceEntry, $"Infection Resistance: {cellStats.currentInfectionResistance}.");

            // ===== FLAGS =====
            SetActiveSafe(waterEntry, cellStats.currentHasWater);
            SetActiveSafe(carrierEntry, cellStats.hasCarrier);
            SetActiveSafe(isContagiousEntry, cellStats.isContagious);
            SetActiveSafe(isBlockedEntry, cellStats.isBlocked);
            SetActiveSafe(isLockdownEntry, cellStats.isLockdown);
            SetActiveSafe(structureEntry, cellStats.structure.type != StructureType.None);

            // ===== DETECTION =====
            if (detectionValueEntry != null)
            {
                if (cellStats.isDetected)
                    detectionValueEntry.Setup("Is detected!", detectedSprite);
                else
                    detectionValueEntry.Setup(
                        $"Detection percent: {cellStats.currentDetectionPercent * 100}%.",
                        undetectedSprite
                    );
            }

            // ===== ENVIRONMENT =====
            EnvironmentData environmentData = cellPropertyManager.GetEnvironmentData(cellStats.environment.currentEnvironmentType);
            SetupEntry(environmentEntry,
                Safe(environmentData != null ? environmentData.displayName : null),
                environmentData != null ? environmentData.sprite : null);

            // ===== POPULATION =====
            PopulationData populationData = cellPropertyManager.GetPopulationData(cellStats.population.type);
            SetupEntry(populationEntry, Safe(populationData != null ? populationData.displayName : null));

            // ===== TEMPERATURE =====
            TempuratureData tempData = cellPropertyManager.GetTempuratureData(cellStats.tempurature.type);
            SetupEntry(temperatureEntry,
                Safe(tempData != null ? tempData.displayName : null),
                tempData != null ? tempData.sprite : null);

            // ===== STRUCTURE =====
            StructureData structureData = cellPropertyManager.GetStructureData(cellStats.structure.type);
            SetupEntry(structureEntry,
                Safe(structureData != null ? structureData.displayName : null),
                structureData != null ? structureData.sprite : null);

            // ===== NEAR STRUCTURES =====
            if (cellStats.affectedByStructures != null)
            {
                var counts = cellStats.affectedByStructures
                    .Where(x => x != StructureType.None)
                    .GroupBy(x => x)
                    .ToDictionary(g => g.Key, g => g.Count());

                if (counts.Count > 0)
                {
                    string text = "Affected by: " + string.Join(", ",
                        counts.Select(kvp => $"{kvp.Value}x {kvp.Key}."));

                    SetupEntry(nearStructuresEntry, text);
                }
                else
                {
                    SetActiveSafe(nearStructuresEntry, false);
                }
            }
            else
            {
                SetActiveSafe(nearStructuresEntry, false);
            }
        }
        else
        {
            if (currentCell != null)
            {
                currentCell.IsBeingShownInfo = false;
                currentCell = null;
            }
        }

        if (background != null)
            background.enabled = state;

        if (container != null)
            container.gameObject.SetActive(state);
    }

    private void SetupEntry(CellInfoUIEntry entry, string text, Sprite sprite = null)
    {
        if (entry == null) return;

        entry.gameObject.SetActive(true);
        entry.Setup(text, sprite);
    }

    private void SetActiveSafe(CellInfoUIEntry entry, bool state)
    {
        if (entry == null) return;

        entry.gameObject.SetActive(state);
    }

    private string Safe(string value)
    {
        return string.IsNullOrEmpty(value) ? "Null" : value;
    }

    public void Init()
    {
        cellPropertyManager = CellPropertyManager.Instance;
        mapManager = MapManager.Instance;

        if (mapManager == null)
        {
            Debug.LogError("MapManager is null!");
            return;
        }

        posEntry.gameObject.SetActive(showDebug);

        SetVisibity(false);
    }
}
