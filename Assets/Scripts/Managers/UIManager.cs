using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Refs")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private Slider tickTimerSlider;
    [SerializeField] private Image timeStateIcon;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Transform cellViewParent;

    [Header("Infected & Dead")]
    [SerializeField] private Slider deadRateSlider;
    [SerializeField] private TMP_Text actualDeadRateText;
    [SerializeField] private TMP_Text actualInfectedText;
    [SerializeField] private TMP_Text actualDeadText;
    [SerializeField] private TMP_Text detectedInfectedText;
    [SerializeField] private TMP_Text detectedDeadText;

    [Header("Prefabs")]
    [SerializeField] private GridCellVisual cellPrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite pauseIcon;
    [SerializeField] private Sprite normalTimeSpeedIcon;
    [SerializeField] private Sprite spedUpTimeIcon;

    [Header("Human")]
    [SerializeField] private TMP_Text threatTierText;
    [SerializeField] private List<ThreatTierUITextColor> threatTierTextColors;

    [SerializeField] private HumanActionVisualDatabase humanActionVisualDatabase;
    [SerializeField] private List<HumanActionUI> humanActionUIs;

    [Header("Vaccine")]
    [SerializeField] private GameObject vaccinePanel;
    [SerializeField] private Slider vaccineProgressSlider;
    [SerializeField] private TMP_Text vaccineProgressText;

    private Dictionary<ThreatTier, Color> threatTierColorMap;

    private CellPresenter[,] presenters;

    private TimeManager timeManager;
    private MapManager mapManager;

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

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        HumanAction.OnExecutedVisual += HandleHumanActionVisualOnCell;
    }

    private void OnDisable()
    {
        HumanAction.OnExecutedVisual -= HandleHumanActionVisualOnCell;
    }

    public void Init()
    {
        timeManager = TimeManager.Instance;
        mapManager = MapManager.Instance;

        pausePanel.SetActive(false);
        timeStateIcon.sprite = normalTimeSpeedIcon;
        tickTimerSlider.minValue = 0;
        tickTimerSlider.maxValue = 1;
        tickTimerSlider.value = 0;

        vaccineProgressText.SetText("0%");
        vaccineProgressSlider.value = 0;
        vaccinePanel.SetActive(false);

        UpdateDeadSlider(0, GameManager.Instance.endGameDeadRate);

        threatTierColorMap = threatTierTextColors.ToDictionary(x => x.threatTier, x => x.color);
    }

    public void SetTimeState()
    {
        if (timeManager.IsPaused)
        {
            pausePanel.SetActive(true);
            timeStateIcon.sprite = pauseIcon;
        }
        else
        {
            pausePanel.SetActive(false);

            if (timeManager.IsSpeedUp)
                timeStateIcon.sprite = spedUpTimeIcon;
            else
                timeStateIcon.sprite = normalTimeSpeedIcon;
        }
    }

    public void SetDayText(string text)
    {
        dayText.SetText($"Day {text}");
    }

    public void SetTickTimer(float value)
    {
        tickTimerSlider.value = value;
    }

    #region Grid Visual

    public void SpawnGridVisual()
    {
        Grid<GridCell> grid = mapManager.GetGrid();
        int width = grid.GetWidth();
        int height = grid.GetHeight();

        presenters = new CellPresenter[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridCell cell = grid.GetCell(x, y);
                GridCellVisual view = SpawnCellView(x, y);

                presenters[x, y] = new CellPresenter(cell, view);
                presenters[x, y].Refresh();
            }
        }
    }

    private GridCellVisual SpawnCellView(int x, int y)
    {
        Grid<GridCell> grid = mapManager.GetGrid();
        int width = grid.GetWidth();
        int height = grid.GetHeight();
        float cellSize = grid.GetCellSize();
        Vector3 originPos = grid.GetOriginPosition();

        GridCellVisual view = Instantiate(cellPrefab, cellViewParent);

        Vector3 worldPos = Utility.GridToWorldPosition(x, y, width, height, cellSize, originPos);
        view.transform.position = worldPos;

        view.name = $"CellView ({x},{y})";

        return view;
    }

    public void UpdateCellsVisual()
    {
        HashSet<Vector2Int> cellsNeedToUpdateVisual = mapManager.GetCellNeedToUpdateVisualList();
        if (cellsNeedToUpdateVisual.Count < 0)
            return;

        foreach (Vector2Int cell in cellsNeedToUpdateVisual)
        {
            presenters[cell.x, cell.y].Refresh();
        }

        cellsNeedToUpdateVisual.Clear();
    }

    #endregion

    #region Cell Visual Animations

    public void SetCellFocusVFXVisibility(GridCell cell, bool state)
    {
        presenters[cell.X, cell.Y].SetCellFocusVFXVisibility(state);
    }

    public void HandleHumanActionVisualOnCell(ActionExecutionVisualData data)
    {
        var preset = humanActionVisualDatabase.Get(data.ActionType);

        foreach (var cell in data.Cells)
        {
            presenters[cell.X, cell.Y].PlayHumanActionVisual(preset);
        }
    }

    #endregion

    #region Human Action Cooldown Visual

    public void ShowHumanActionsOfTheCurrentThreatTier(ThreatTierSO currentThreatTier)
    {
        List<HumanActionType> humanActionTypes = currentThreatTier.availableActions;

        foreach (HumanActionUI humanActionUI in humanActionUIs)
        {
            if (humanActionTypes.Contains(humanActionUI.actionType))
                humanActionUI.SetVisibility(true);
            else
                humanActionUI.SetVisibility(false);
        }
    }

    public void UpdateHumanActionsCooldownUI(List<HumanAction> humanActions)
    {
        foreach (HumanAction action in humanActions)
        {
            HumanActionUI ui = humanActionUIs.Find(x => x.actionType == action.Data.ActionType);

            if (ui != null)
            {
                ui.SetCooldown(action.CooldownTicks);
            }
        }
    }

    public void SetExecutedHumanActionsCooldownUI(List<HumanActionType> humanActionTypes)
    {
        foreach (HumanActionType actionType in humanActionTypes)
        {
            HumanActionUI ui = humanActionUIs.Find(x => x.actionType == actionType);

            if (ui != null)
            {
                ui.SetExecutedIconVisibility(true);
            }
        }
    }

    #endregion

    #region Threat Tier

    public void UpdateThreatTier(ThreatTierSO currentThreatTier)
    {
        threatTierText.text = currentThreatTier.tierName;

        threatTierText.color = threatTierColorMap.TryGetValue(currentThreatTier.tierType, out Color color)
            ? color
            : Color.white;
    }

    #endregion

    #region Vaccine Progress Visual

    public void SetVaccineProgress(float progress)
    {
        vaccineProgressSlider.value = progress;
        vaccineProgressText.SetText($"{Mathf.RoundToInt(progress * 100)}%");
        if (!vaccinePanel.activeSelf)
            vaccinePanel.SetActive(true);
    }

    #endregion

    #region Infected & Dead

    public void UpdateActualInfectedRateAndDeadRate(float actualInfected, float actualInfectedRate, float actualDead, float actualDeadRate)
    {
        int actualInfectedToInt = Mathf.RoundToInt(actualInfected);
        int actualDeadToInt = Mathf.RoundToInt(actualDead);
        int actualInfectedRateToInt = Mathf.RoundToInt(actualInfectedRate * 100);
        int actualDeadRateToInt = Mathf.RoundToInt(actualDeadRate * 100);

        actualInfectedText.text = $"{actualInfectedToInt} - {actualInfectedRateToInt}%";
        actualDeadText.text = $"{actualDeadToInt} - {actualDeadRateToInt}%";
    }

    public void UpdateDetectedInfectedRateAndDeadRate(float detectedInfected, float detectedInfectedRate, float detectedDead, float detectedDeadRate)
    {
        int detectedInfectedToInt = Mathf.RoundToInt(detectedInfected);
        int detectedDeadToInt = Mathf.RoundToInt(detectedDead);
        int detectedInfectedRateToInt = Mathf.RoundToInt(detectedInfectedRate * 100);
        int detectedDeadRateToInt = Mathf.RoundToInt(detectedDeadRate * 100);

        detectedInfectedText.text = $"{detectedInfectedToInt} - {detectedInfectedRateToInt}%";
        detectedDeadText.text = $"{detectedDeadToInt} - {detectedDeadRateToInt}%";
    }

    public void UpdateDeadSlider(float value, float endGameDeadRate)
    {
        int current = Mathf.RoundToInt(value * 100);
        int target = Mathf.RoundToInt(endGameDeadRate * 100);

        actualDeadRateText.text = $"{current}%/{target}%";
        deadRateSlider.value = value;
    }
    #endregion
}
