using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    [Range(0, 1f)]
    public float endGameDeadRate;

    [SerializeField] private TimeManager timeManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private SpreadMethodManager spreadMethodManager;
    [SerializeField] private CellInfoUIContentManager cellInfoUIContentManager;
    [SerializeField] private HumanAIManager humanAIManager;
    [SerializeField] private VaccineSystem vaccineSystem;

    void Awake()
    {
        Instance = this;
        if (Instance != this)
        {
            Destroy(this);
        }
    }

    void Start()
    {
        uiManager.Init();
        cellInfoUIContentManager.Init();

        mapManager.BuildGrid();

        vaccineSystem.Init();
        spreadMethodManager.Init();
        humanAIManager.Init();

        mapManager.StartMatch();
        humanAIManager.StartMatch();
        timeManager.StartMatch();
    }
}