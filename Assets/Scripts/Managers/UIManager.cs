using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    [SerializeField] private GameObject topMiddlePanel;
    [SerializeField] private GameObject topLeftPanel;
    [SerializeField] private GameObject topRightPanel;
    [SerializeField] private CellInfoUIContentManager cellInfoViewPanel;
    [SerializeField] private VaccinePanel vaccinePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject evolutionSelectionPanel;
    [SerializeField] private EvolutionTreePanel evolutionTreePanel;
    [SerializeField] private Notification notificationPanel;
    [SerializeField] private GameObject skillsPanel;
    [SerializeField] private SkillDetailPanel skillDetailPanel;
    [SerializeField] private SpreadMethodDetailPanel spreadMethodDetailPanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject confirmExitGamePanel;
    [SerializeField] private GameObject endGamePanel;

    [Header("Points")]
    [SerializeField] private PointTextContainer evolutionPointTextContainer;
    [SerializeField] private PointTextContainer infectionPointTextContainer;

    [Header("Time")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private Slider tickTimerSlider;
    [SerializeField] private Image timeStateIcon;

    [Header("Infected & Dead")]
    [SerializeField] private Slider deadRateSlider;
    [SerializeField] private TMP_Text actualDeadRateText;
    [SerializeField] private TMP_Text actualInfectedText;
    [SerializeField] private TMP_Text actualDeadText;
    [SerializeField] private TMP_Text detectedInfectedText;
    [SerializeField] private TMP_Text detectedDeadText;

    [Header("Cell Info View")]
    [SerializeField] private Transform cellViewParent;

    [Header("Evolution Upgrade Panel")]
    [SerializeField] private Transform evolutionCardParent;
    [SerializeField] private TMP_Text evolutionTierText;
    [SerializeField] private TMP_Text evolutionCostText;
    [SerializeField] private Button evolutionPanelToggleVisibilityButton;

    [Header("Evolution Tree Panel")]
    [SerializeField] private Button evolutionTreePanelToggleVisibilityButton;

    [Header("Spread Methods")]
    [SerializeField] private List<SpreadMethodUI> spreadMethodUIs;

    [Header("Skill UI")]
    [SerializeField] private SkillUI skillUIPrefab;
    [SerializeField] private Transform skillUIContainer;

    [Header("End Game Panel")]
    [SerializeField] private TMP_Text endGamePanelTitleText;
    [SerializeField] private TMP_Text endGameTimeText;

    [Header("Human")]
    [SerializeField] private TMP_Text threatTierText;
    [SerializeField] private List<ThreatTierUITextColor> threatTierTextColors;
    [SerializeField] private HumanActionVisualDatabase humanActionVisualDatabase;
    [SerializeField] private List<HumanActionUI> humanActionUIs;

    [Header("Prefabs")]
    [SerializeField] private GridCellVisual cellPrefab;
    [SerializeField] private EvolutionUpgradeCard evolutionCardPrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite pauseIcon;
    [SerializeField] private Sprite normalTimeSpeedIcon;
    [SerializeField] private Sprite spedUpTimeIcon;

    [Header("Input")]
    [SerializeField] private InputAction toggleSettingPanelAction;

    [Header("Event SOs")]
    [SerializeField] private VoidPublisherSO onEvolutionTierSelectionUIOpened;
    [SerializeField] private VoidPublisherSO onEvolutionTierSelectionUIClosed;
    [SerializeField] private VoidPublisherSO onEvolutionTreePanelOpened;
    [SerializeField] private VoidPublisherSO onEvolutionTreePanelClosed;
    [SerializeField] private BoolPublisherSO onToggleSettingPanel;

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


    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is enabled
    /// </summary>
    void OnEnable()
    {
        HumanAction.OnExecutedVisual += HandleHumanActionVisualOnCell;
        toggleSettingPanelAction.Enable();
    }

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is disabled
    /// </summary>
    void OnDisable()
    {
        HumanAction.OnExecutedVisual -= HandleHumanActionVisualOnCell;
        toggleSettingPanelAction.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (toggleSettingPanelAction.bindings.Count == 0)
        {
            Debug.LogWarning("The Toggle Setting Panel Action does not have a binding set! Make sure that each Input Action has a binding set or the controller will not work!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.GetGameState() == GameState.Ended) return;

        if (toggleSettingPanelAction.triggered)
        {
            SetSettingPanelVisibility(!settingPanel.activeSelf);
        }
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

        vaccinePanel.gameObject.SetActive(false);

        UpdateDeadSlider(0, GameManager.Instance.endGameDeadRate);

        threatTierColorMap = threatTierTextColors.ToDictionary(x => x.threatTier, x => x.color);

        SetEvolutionPointText(0, 0);
        SetInfectionPointText(0, 0);

        evolutionSelectionPanel.SetActive(false);
        evolutionPanelToggleVisibilityButton.gameObject.SetActive(false);
        spreadMethodDetailPanel.gameObject.SetActive(false);
        skillDetailPanel.gameObject.SetActive(false);

        evolutionTreePanel.SetPanelVisibility(false, null);
        SetEvolutionTreePanelToggleButtonVisibility(false);

        notificationPanel.HidePanel();

        SpawnSkillUIs();
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

    #region Spread Method Cooldown

    public void UpdateSpreadMethodCooldownUI(List<SpreadMethodRuntimeData> spreadMethodRuntimeDatas)
    {
        foreach (SpreadMethodRuntimeData methodRuntimeData in spreadMethodRuntimeDatas)
        {
            SpreadMethodUI ui = spreadMethodUIs.Find(x => x.methodType == methodRuntimeData.methodType);
            if (ui != null)
            {
                ui.SetCooldown(methodRuntimeData.remainingTicksToSpread);
            }
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

    public void SetVaccineProgress(float progress, bool isIncreasing)
    {
        vaccinePanel.UpdateData(progress, isIncreasing);
    }

    #endregion

    #region Infected & Dead

    public void UpdateActualInfectedRateAndDeadRate(float actualInfected, float actualInfectedRate, float actualDead, float actualDeadRate)
    {
        int actualInfectedToInt = Mathf.FloorToInt(actualInfected);
        int actualDeadToInt = Mathf.FloorToInt(actualDead);
        int actualInfectedRateToInt = Mathf.FloorToInt(actualInfectedRate * 100);
        int actualDeadRateToInt = Mathf.FloorToInt(actualDeadRate * 100);

        actualInfectedText.text = $"{actualInfectedToInt} - {actualInfectedRateToInt}%";
        actualDeadText.text = $"{actualDeadToInt} - {actualDeadRateToInt}%";
    }

    public void UpdateDetectedInfectedRateAndDeadRate(float detectedInfected, float detectedInfectedRate, float detectedDead, float detectedDeadRate)
    {
        int detectedInfectedToInt = Mathf.FloorToInt(detectedInfected);
        int detectedDeadToInt = Mathf.FloorToInt(detectedDead);
        int detectedInfectedRateToInt = Mathf.FloorToInt(detectedInfectedRate * 100);
        int detectedDeadRateToInt = Mathf.FloorToInt(detectedDeadRate * 100);

        detectedInfectedText.text = $"{detectedInfectedToInt} - {detectedInfectedRateToInt}%";
        detectedDeadText.text = $"{detectedDeadToInt} - {detectedDeadRateToInt}%";
    }

    public void UpdateDeadSlider(float value, float endGameDeadRate)
    {
        int current = Mathf.FloorToInt(value * 100);
        int target = Mathf.FloorToInt(endGameDeadRate * 100);

        actualDeadRateText.text = $"{current}%/{target}%";
        deadRateSlider.value = value;
    }

    #endregion

    #region Notifications

    public void ShowNotification(string message, float duration, Color color)
    {
        notificationPanel.gameObject.SetActive(true);
        notificationPanel.ShowNotification(message, color, duration);
    }

    #endregion

    #region Evolution Upgrades

    public void ShowEvolutionUpgradeNotification(string message, float duration, Color color)
    {
        ShowNotification(message, duration, color);
        evolutionPanelToggleVisibilityButton.gameObject.SetActive(true);
    }

    public void ShowSpecializationSelection(List<EvolutionTreeSO> evolutionTrees)
    {
        ClearCards();

        SetBackgroundUIElementsVisibilityForEvolutionSelection(false, false);
        evolutionSelectionPanel.SetActive(true);
        evolutionPanelToggleVisibilityButton.gameObject.SetActive(false);

        foreach (EvolutionTreeSO evolutionTreeSO in evolutionTrees)
            AddCard(evolutionTreeSO.spreadType);
    }

    public void ToggleEvolutionSelectionPanel()
    {
        bool isVisible = !evolutionSelectionPanel.activeSelf;
        if (isVisible)
            onEvolutionTierSelectionUIOpened.RaiseEvent();
        else
        {
            onEvolutionTierSelectionUIClosed.RaiseEvent();
            SetEvolutionSelectionPanelVisibility(false, true);
        }
    }

    public void ShowEvolutionTier(EvolutionTierData tierData)
    {
        ClearCards();

        SetEvolutionSelectionPanelVisibility(true, true);

        foreach (EvolutionUpgradeNodeSO upgradeNodeSO in tierData.choices)
            AddCard(upgradeNodeSO);
    }

    public void SetEvolutionSelectionPanelVisibility(bool isVisible, bool showTogglePanelButton)
    {
        SetBackgroundUIElementsVisibilityForEvolutionSelection(!isVisible, true);
        evolutionSelectionPanel.SetActive(isVisible);
        evolutionPanelToggleVisibilityButton.gameObject.SetActive(showTogglePanelButton);
    }

    public void AddCard(EvolutionUpgradeNodeSO upgradeNodeSO)
    {
        EvolutionUpgradeCard upgradeCard = Instantiate(evolutionCardPrefab, evolutionCardParent);
        upgradeCard.SetCardInfo(upgradeNodeSO);
    }

    private void SetBackgroundUIElementsVisibilityForEvolutionSelection(bool show, bool showTopLeftPanel)
    {
        topMiddlePanel.SetActive(show);
        topLeftPanel.SetActive(showTopLeftPanel);
        topRightPanel.SetActive(show);
        cellInfoViewPanel.SetVisibility(show);
        skillsPanel.SetActive(show);
        skillDetailPanel.gameObject.SetActive(false);
        spreadMethodDetailPanel.gameObject.SetActive(false);

        if (VaccineSystem.Instance.Stage != VaccineDevelopmentStage.NotStarted)
            vaccinePanel.gameObject.SetActive(show);
        else
            vaccinePanel.gameObject.SetActive(false);

        evolutionTreePanel.SetPanelVisibility(false, null);
    }

    public void OnEvolutionUpgradeFinishSelection()
    {
        SetEvolutionSelectionPanelVisibility(false, false);
        ClearCards();
    }

    public void SetEvolutionSelectionPanelInfo(int tierIndex, int cost)
    {
        evolutionTierText.text = $"Tier {tierIndex}";
        evolutionCostText.text = $"Cost: {cost}";
    }

    #endregion

    #region Evolution Tree

    public void ToggleEvolutionTreePanel()
    {
        bool isVisible = !evolutionTreePanel.gameObject.activeSelf;
        SetEvolutionTreePanel(isVisible);
    }

    public void SetEvolutionTreePanel(bool isVisible)
    {
        SetBackgroundUIElementsVisibilityForEvolutionTree(!isVisible);

        if (isVisible)
        {
            onEvolutionTreePanelOpened.RaiseEvent();
        }
        else
        {
            SetEvolutionTreePanelVisibility(false, null);
            onEvolutionTreePanelClosed.RaiseEvent();
        }
    }

    private void SetBackgroundUIElementsVisibilityForEvolutionTree(bool show)
    {
        topMiddlePanel.SetActive(show);
        topLeftPanel.SetActive(show);
        topRightPanel.SetActive(show);
        cellInfoViewPanel.SetVisibility(show);
        skillsPanel.SetActive(show);
        skillDetailPanel.gameObject.SetActive(false);
        spreadMethodDetailPanel.gameObject.SetActive(false);

        if (VaccineSystem.Instance.Stage != VaccineDevelopmentStage.NotStarted)
            vaccinePanel.gameObject.SetActive(show);
        else
            vaccinePanel.gameObject.SetActive(false);

        evolutionSelectionPanel.SetActive(false);
    }


    public void InitializeEvolutionTree(EvolutionTreeSO evolutionTree, string methodName, Sprite methodIcon)
    {
        List<EvolutionUpgradeNodeSO> nodeSOs = new();

        foreach (EvolutionTierData tierData in evolutionTree.tiers)
        {
            nodeSOs.AddRange(tierData.choices);
        }

        evolutionTreePanel.Initialize(nodeSOs, methodName, methodIcon);
        SetEvolutionTreePanelToggleButtonVisibility(true);
    }

    public void SetEvolutionTreePanelToggleButtonVisibility(bool isVisible)
    {
        evolutionTreePanelToggleVisibilityButton.gameObject.SetActive(isVisible);
    }

    public void SetEvolutionTreePanelVisibility(bool isVisible, List<EvolutionTierInfo> tierInfos)
    {
        evolutionTreePanel.gameObject.SetActive(isVisible);
        evolutionTreePanel.SetPanelVisibility(isVisible, tierInfos);
    }

    public bool IsEvolutionTreePanelInitialized()
    {
        return evolutionTreePanel.IsInitialized();
    }

    public void AddCard(SpreadMethodType methodType)
    {
        EvolutionUpgradeCard upgradeCard = Instantiate(evolutionCardPrefab, evolutionCardParent);

        SpreadMethodDataSO spreadMethodDataSO = SpreadMethodManager.Instance.GetSpreadMethodData(methodType);
        upgradeCard.SetCardInfo(spreadMethodDataSO);
    }

    public void ClearCards()
    {
        if (evolutionCardParent.childCount > 0)
            foreach (Transform child in evolutionCardParent)
                Destroy(child.gameObject);
    }

    #endregion

    public bool IsEvolutionPanelsOpened()
    {
        return evolutionSelectionPanel.activeSelf || evolutionTreePanel.gameObject.activeSelf;
    }

    #region Points

    public void SetEvolutionPointText(int value, int diff)
    {
        evolutionPointTextContainer.SetPoints(value, diff);
    }

    public void SetInfectionPointText(int value, int diff)
    {
        infectionPointTextContainer.SetPoints(value, diff);
    }

    #endregion

    #region Skill Panel

    public void SpawnSkillUIs()
    {
        List<SkillDataSO> skillDataList = SkillManager.Instance.GetAllSkillData();

        foreach (SkillDataSO skillData in skillDataList)
        {
            SkillUI skillUI = Instantiate(skillUIPrefab, skillUIContainer);
            skillUI.SetData(skillData);
        }
    }

    public void UpdateSkillUIs()
    {
        foreach (Transform child in skillUIContainer)
        {
            if (child.TryGetComponent<SkillUI>(out var skillUI))
            {
                IBaseSkill skill = SkillManager.Instance.GetSkill(skillUI.skillType);
                if (skill != null)
                {
                    SkillRuntimeData skillRuntimeData = skill.GetRuntimeData();

                    if (skillRuntimeData != null)
                        skillUI.SetCooldown(skillRuntimeData.remainingCooldownTicks, skill.GetData().cooldownTicks);
                }
            }
        }
    }

    #endregion

    #region Spread Method Detail Panel

    public void ShowSpreadMethodDetailPanel(SpreadMethodType type)
    {
        SpreadMethodRuntimeData data = SpreadMethodManager.Instance.GetRuntimeData(type);
        if (data == null)
        {
            Debug.LogError($"No data found for spread method type {type}");
            return;
        }

        spreadMethodDetailPanel.gameObject.SetActive(true);
        spreadMethodDetailPanel.SetInfo(data);
    }

    public void HideSpreadMethodDetailPanel()
    {
        spreadMethodDetailPanel.gameObject.SetActive(false);
    }

    #endregion

    #region Skill Detail Panel

    public void ShowSkillDetailPanel(SkillType skillType)
    {
        SkillDataSO skillData = SkillManager.Instance.GetSkillData(skillType);
        if (skillData == null)
        {
            Debug.LogError($"No SkillData found for skill type {skillType}");
            return;
        }

        skillDetailPanel.gameObject.SetActive(true);
        skillDetailPanel.SetInfo(skillData);
    }

    public void HideSkillDetailPanel()
    {
        skillDetailPanel.gameObject.SetActive(false);
    }

    public void SetBackgroundUIElementsVisibilityForSelectTargetsForSkill(bool show)
    {
        topMiddlePanel.SetActive(show);
        topRightPanel.SetActive(show);

        if (VaccineSystem.Instance.Stage != VaccineDevelopmentStage.NotStarted)
            vaccinePanel.gameObject.SetActive(show);
        else
            vaccinePanel.gameObject.SetActive(false);

        skillDetailPanel.gameObject.SetActive(false);
        spreadMethodDetailPanel.gameObject.SetActive(false);
        evolutionSelectionPanel.SetActive(false);
        evolutionTreePanel.gameObject.SetActive(false);
    }

    #endregion

    public void SetCellHightlight(List<Vector2Int> cellPositions, bool state)
    {
        foreach (Vector2Int cellPos in cellPositions)
        {
            presenters[cellPos.x, cellPos.y].SetCellTargetedBySkillOverlayVisibility(state);
        }
    }

    #region Setting Panel

    public void SetSettingPanelVisibility(bool isVisible)
    {
        settingPanel.SetActive(isVisible);
        onToggleSettingPanel.RaiseEvent(isVisible);
    }

    public void SetConfirmExitGamePanelVisibility(bool isVisible)
    {
        confirmExitGamePanel.SetActive(isVisible);
    }

    #endregion

    #region End Game Panel

    public void OpenEndGamePanel(bool result)
    {
        SetBackgroundUIElementsVisibilityForEndGamePanel(false);

        if (result)
        {
            endGamePanelTitleText.text = "VICTORY";
        }
        else
        {
            endGamePanelTitleText.text = "DEFEATED";
        }

        int totalDay = TimeManager.Instance.CurrentDay;
        endGameTimeText.text = $"{totalDay} days.";

        endGamePanel.SetActive(true);
    }

    public void CloseEndGamePanel()
    {
        SetBackgroundUIElementsVisibilityForEndGamePanel(true);
        endGamePanel.SetActive(false);
        endGameTimeText.text = "0 days.";
    }

    public void SetBackgroundUIElementsVisibilityForEndGamePanel(bool show)
    {
        topMiddlePanel.SetActive(show);
        topRightPanel.SetActive(show);

        cellInfoViewPanel.SetVisibility(false);
        skillDetailPanel.gameObject.SetActive(false);
        spreadMethodDetailPanel.gameObject.SetActive(false);
        vaccinePanel.gameObject.SetActive(false);

        evolutionSelectionPanel.SetActive(false);
        evolutionTreePanel.gameObject.SetActive(false);
    }

    #endregion
}
