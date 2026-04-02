using System.Collections.Generic;
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

    [Header("Prefabs")]
    [SerializeField] private GridCellVisual cellPrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite pauseIcon;
    [SerializeField] private Sprite normalTimeSpeedIcon;
    [SerializeField] private Sprite spedUpTimeIcon;

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

    public void Init()
    {
        timeManager = TimeManager.Instance;
        mapManager = MapManager.Instance;

        pausePanel.SetActive(false);
        timeStateIcon.sprite = normalTimeSpeedIcon;
        tickTimerSlider.minValue = 0;
        tickTimerSlider.maxValue = 0;
        tickTimerSlider.value = 0;
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
}
