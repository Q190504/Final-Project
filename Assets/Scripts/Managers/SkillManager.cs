using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [SerializeField] private float evolutionPointMultiplier = 1;
    [SerializeField] private float infectionPointMultiplier = 1;

    [SerializeField] private List<SkillDataSO> skillDatas;

    [Header("Input")]
    [SerializeField] private InputAction selectTargetAction;

    [Header("Refs")]
    [SerializeField] private List<RectTransform> ignoredPanels;

    [Header("Event SOs")]
    public VoidPublisherSO onTargetSkillSelectTargetSO;
    public VoidPublisherSO onTargetSkillFinishSO;

    private List<IBaseSkill> skills = new();

    private Dictionary<SkillType, SkillDataSO> skillDataDict;

    public bool IsSelectingSkill => selectedSkill != null;

    private IBaseSkill selectedSkill;
    private bool isSelectingTarget;
    private List<GridCell> targets = new();

    private MapManager mapManager;
    private Grid<GridCell> grid;

    private void Awake()
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
        selectTargetAction.Enable();
    }

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is disabled
    /// </summary>
    void OnDisable()
    {
        selectTargetAction.Disable();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (selectTargetAction.bindings.Count == 0)
        {
            Debug.LogWarning("The Select Target Action does not have a binding set! Make sure that each Input Action has a binding set or the controller will not work!");
        }
    }

    private void Update()
    {
        SetTargetCellsHighlightOverlay(false);
        targets.Clear();

        GameState gameState = GameManager.Instance.GetGameState();
        if (gameState == GameState.Paused
            && isSelectingTarget
            && selectedSkill != null
            && selectedSkill is ITargetSkill)
        {
            if (grid != null)
            {
                Vector3 mouseWorldPos = Utility.GetMouseWorldPosition();

                // check if mouse is not over UI
                if (!Utility.IsPointerOverPanel(ignoredPanels))
                {
                    Vector2Int cellPos = Utility.WorldToGridPosition(
                      mouseWorldPos,
                      grid.GetWidth(),
                      grid.GetHeight(),
                      grid.GetCellSize(),
                      grid.GetOriginPosition()
                    );

                    if (grid.IsInBounds(cellPos.x, cellPos.y))
                    {
                        GridCell cell = grid.GetCell(cellPos.x, cellPos.y);

                        ITargetSkill currentSkill = selectedSkill as ITargetSkill;
                        int radius = currentSkill.GetTargetSkillRuntimeData().targetSkillExtraConfig.targetRadius;
                        targets = grid.GetNeighborsInRange(cell, radius);
                        targets.Add(cell);

                        SetTargetCellsHighlightOverlay(true);

                        if (selectTargetAction.triggered)
                        {
                            foreach (GridCell target in targets)
                            {
                                if (!currentSkill.IsValidTarget(target))
                                {
                                    UIManager.Instance.ShowNotification("Invalid target!", 2f, Color.red);
                                    return;
                                }
                            }

                            currentSkill.Execute(cell);
                            OnTargetSkillFinishExecuteOrDeselected();
                        }
                    }
                }
                else
                {
                    if (targets != null && targets.Count > 0)
                    {
                        SetTargetCellsHighlightOverlay(false);
                        targets = null;
                    }
                }
            }
        }
    }

    public void SetTargetCellsHighlightOverlay(bool state)
    {
        if (targets != null && targets.Count > 0)
        {
            List<Vector2Int> cellPositions = new();
            foreach (GridCell cell in targets)
            {
                cellPositions.Add(new Vector2Int(cell.X, cell.Y));
            }

            UIManager.Instance.SetCellHightlight(cellPositions, state);
        }
    }

    public void TickCooldowns(float deltaTime)
    {
        foreach (IBaseSkill skill in skills)
        {
            skill.TickCooldown(deltaTime);
        }

        UIManager.Instance.UpdateSkillUIs();
    }

    #region Init

    public void Init()
    {
        CreateDicts();
        mapManager = MapManager.Instance;
        grid = mapManager.GetGrid();
        targets = new List<GridCell>();

        CreateSkills();
    }

    public void CreateSkills()
    {
        skills.Clear();

        foreach (SkillDataSO skilldData in skillDatas)
        {
            IBaseSkill method = skilldData.CreateSkill(skilldData.CreateRuntimeData());
            skills.Add(method);
        }
    }

    private void CreateDicts()
    {
        skillDataDict = new Dictionary<SkillType, SkillDataSO>();
        foreach (SkillDataSO skillData in skillDatas)
        {
            skillDataDict.Add(skillData.type, skillData);
        }
    }

    #endregion

    #region Getters

    public int GetAllSkillsUsedCount()
    {
        int count = 0;
        foreach (IBaseSkill skill in skills)
            count += skill.GetRuntimeData().skillUseCount;

        return count;
    }

    public float GetEvolutionPointMultiplier()
    {
        return evolutionPointMultiplier;
    }

    public float GetInfectionPointMultiplier()
    {
        return infectionPointMultiplier;
    }

    public List<SkillDataSO> GetAllSkillData()
    {
        return skillDatas;
    }

    public SkillDataSO GetSkillData(SkillType skillType)
    {
        if (skillDataDict.ContainsKey(skillType))
            return skillDataDict[skillType];

        return null;
    }

    public IBaseSkill GetSkill(SkillType skillType)
    {
        return skills.Find(s => s.GetData().type == skillType);
    }

    #endregion

    #region Execute Target Skills

    public void TryUseSkill(SkillType skillType)
    {
        if (selectedSkill != null && selectedSkill.GetData().type == skillType)
        {
            DeselectCurrentSkill();
            return;
        }

        DeselectCurrentSkill();

        foreach (IBaseSkill skill in skills)
        {
            if (skill.GetData().type != skillType)
                continue;

            if (!skill.CanCast())
                return;

            selectedSkill = skill;

            if (selectedSkill is IInstantSkill instantSkill)
            {
                instantSkill.Execute();

                selectedSkill = null;
            }
            else if (selectedSkill is ITargetSkill)
            {
                isSelectingTarget = true;
                onTargetSkillSelectTargetSO.RaiseEvent();
            }

            return;
        }
    }

    private void DeselectCurrentSkill()
    {
        if (selectedSkill != null)
        {
            OnTargetSkillFinishExecuteOrDeselected();
        }
    }

    private void OnTargetSkillFinishExecuteOrDeselected()
    {
        SetTargetCellsHighlightOverlay(false);
        isSelectingTarget = false;
        selectedSkill = null;
        targets.Clear();
        onTargetSkillFinishSO.RaiseEvent();
    }

    #endregion
}