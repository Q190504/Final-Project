using UnityEngine;

public enum GameState
{
    NotStarted,
    Playing,
    Paused,
    Ended,
}

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

    [Header("Game Settings")]
    [Range(0, 1f)]
    public float endGameDeadRate;

    [Header("Refs")]
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private SpreadMethodManager spreadMethodManager;
    [SerializeField] private CellInfoUIContentManager cellInfoUIContentManager;
    [SerializeField] private HumanAIManager humanAIManager;
    [SerializeField] private VaccineSystem vaccineSystem;
    [SerializeField] private EvolutionManager evolutionManager;
    [SerializeField] private SkillManager skillManager;

    [SerializeField] private VoidPublisherSO setBGMWhenMatchStartedSO;

    private GameState gameState;

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
        skillManager.Init();

        mapManager.StartMatch();
        humanAIManager.StartMatch();
        timeManager.StartMatch();

        setBGMWhenMatchStartedSO.RaiseEvent();

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

    public void EndGame()
    {
        gameState = GameState.Ended;
    }
}