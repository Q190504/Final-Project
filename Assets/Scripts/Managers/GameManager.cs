using UnityEngine;

public enum GameState
{
    NotStarted,
    Playing,
    Paused,
    Ended,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    [Range(0, 1f)]
    public float endGameDeadRate;

    private GameState gameState;

    [SerializeField] private TimeManager timeManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private SpreadMethodManager spreadMethodManager;
    [SerializeField] private CellInfoUIContentManager cellInfoUIContentManager;
    [SerializeField] private HumanAIManager humanAIManager;
    [SerializeField] private VaccineSystem vaccineSystem;
    [SerializeField] private EvolutionManager evolutionManager;

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
        gameState = GameState.NotStarted;

        uiManager.Init();
        cellInfoUIContentManager.Init();

        mapManager.BuildGrid();

        vaccineSystem.Init();
        spreadMethodManager.Init();
        humanAIManager.Init();
        evolutionManager.Init();

        mapManager.StartMatch();
        humanAIManager.StartMatch();
        timeManager.StartMatch();

        gameState = GameState.Playing;
    }

    public void SetGameState(GameState gameState)
    {
        this.gameState = gameState;
    }

    public GameState GetGameState()
    {
        return gameState;
    }
}