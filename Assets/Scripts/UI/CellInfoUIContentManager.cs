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
    [SerializeField] private Transform container;
    [SerializeField] private Button openButton;
    [SerializeField] private bool showDebug;

    [Header("Entries")]
    [SerializeField] private CellInfoUIEntry posEntry;
    [SerializeField] private CellInfoUIEntry infectionLevelEntry;
    [SerializeField] private CellInfoUIEntry stageEntry;
    [SerializeField] private CellInfoUIEntry infectionResistanceEntry;
    [SerializeField] private CellInfoUIEntry sterilizationResistanceEntry;
    [SerializeField] private CellInfoUIEntry sterilizationImmnuneDaysEntry;
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
    [SerializeField] private List<RectTransform> ignoredPanels;

    [Header("Input")]
    [SerializeField] private InputAction getCellInfoAction;

    private PropertyDataManager cellPropertyManager;
    private MapManager mapManager;
    private UIManager uiManager;

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
            if (grid != null && !Utility.IsPointerOverPanel(ignoredPanels))
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
                    SetVisibility(true, cellPos);
                    GridCell cell = grid.GetCell(cellPos.x, cellPos.y);
                    if (showDebug)
                        cell.DebugStats();
                }
            }
        }
    }

    public void SetVisibility(bool state)
    {
        SetVisibility(state, null);
    }

    public void SetVisibility(bool state, Vector2Int? targetCellPos = null)
    {
        if (state)
        {
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

            if (targetCellPos.HasValue)
            {
                GridCell previousCell = currentCell;

                GridCell targetCell = grid.GetCell(targetCellPos.Value.x, targetCellPos.Value.y);
                if (targetCell.Stats == null)
                {
                    Debug.LogError("Cell or CellStats is null!");
                    return;
                }

                currentCell = targetCell;
                if (previousCell != null && currentCell != previousCell)
                {
                    previousCell.IsBeingShownInfo = false;
                    uiManager.SetCellFocusVFXVisibility(previousCell, false);
                }

                uiManager.SetCellFocusVFXVisibility(currentCell, true);
            }

            if (currentCell == null) return;

            CellStats currentCellStats = currentCell.Stats;
            currentCell.IsBeingShownInfo = true;

            // ===== BASIC =====
            if (showDebug) SetupEntry(posEntry, $"Pos: {currentCell.X}, {currentCell.Y}.");
            SetupEntry(infectionLevelEntry, $"Infection Level: {currentCellStats.infectionLevel}.");

            CellStageData stageData = cellPropertyManager.GetCellStageData(currentCellStats.stage.type);
            SetupEntry(stageEntry, Safe(stageData != null ? $"Stage: {stageData.displayName}." : null), stageData != null ? stageData.sprite : null);

            SetupEntry(infectionResistanceEntry, $"Infection Resistance: {currentCellStats.GetInfectionResistance()}.");
            SetupEntry(sterilizationResistanceEntry, $"Sterilization Resistance: {currentCellStats.GetSterilizationResistance()}.");
            SetupEntry(sterilizationImmnuneDaysEntry, $"Sterilization Immunity Days: {currentCellStats.sterilizationImmunityTicks}.");

            // ===== FLAGS =====
            SetActiveSafe(sterilizationImmnuneDaysEntry, currentCellStats.sterilizationImmunityTicks > 0);
            SetActiveSafe(waterEntry, currentCellStats.HasWater());
            SetActiveSafe(carrierEntry, currentCellStats.hasCarrier);
            SetActiveSafe(isContagiousEntry, currentCellStats.isContagious);
            SetActiveSafe(isBlockedEntry, currentCellStats.isBlocked);
            SetActiveSafe(isLockdownEntry, currentCellStats.isLockdown);
            SetActiveSafe(structureEntry, currentCellStats.structure.type != StructureType.None);

            // ===== DETECTION =====
            if (detectionValueEntry != null)
            {
                if (currentCellStats.isDetected)
                    detectionValueEntry.Setup("Is detected!", detectedSprite);
                else
                {
                    float detectionValue = Mathf.FloorToInt(currentCellStats.finalDetection * 100);
                    detectionValueEntry.Setup(
                       $"Detection percent: {detectionValue}%.",
                       undetectedSprite
                   );
                }
            }

            // ===== ENVIRONMENT =====
            EnvironmentData environmentData = cellPropertyManager.GetEnvironmentData(currentCellStats.environment.currentEnvironmentType);
            SetupEntry(environmentEntry,
                Safe(environmentData != null ? environmentData.displayName : null),
                environmentData != null ? environmentData.sprite : null);

            // ===== POPULATION =====
            PopulationData populationData = cellPropertyManager.GetPopulationData(currentCellStats.population.type);
            SetupEntry(populationEntry, Safe(populationData != null ? populationData.displayName : null));

            // ===== TEMPERATURE =====
            TempuratureData tempData = cellPropertyManager.GetTempuratureData(currentCellStats.tempurature.type);
            SetupEntry(temperatureEntry,
                Safe(tempData != null ? tempData.displayName : null),
                tempData != null ? tempData.sprite : null);

            // ===== STRUCTURE =====
            StructureDataSO structureData = cellPropertyManager.GetStructureData(currentCellStats.structure.type);
            if (currentCellStats.structure.type != StructureType.None)
            {
                SetupEntry(structureEntry,
                    Safe(structureData != null ? structureData.displayName + $" - {(currentCellStats.structure.isActive ? "Active" : "Inactive")}" : null),
                    structureData != null ? structureData.sprite : null);
            }

            // ===== NEAR STRUCTURES =====
            if (currentCellStats.affectedByStructures != null)
            {
                var counts = currentCellStats.affectedByStructures
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
            }
        }

        if (openButton != null)
            openButton.gameObject.SetActive(!state);

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
        cellPropertyManager = PropertyDataManager.Instance;
        mapManager = MapManager.Instance;
        uiManager = UIManager.Instance;

        if (mapManager == null)
        {
            Debug.LogError("MapManager is null!");
            return;
        }

        posEntry.gameObject.SetActive(showDebug);

        SetVisibility(false);
    }
}
