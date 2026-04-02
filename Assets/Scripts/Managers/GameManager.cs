using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private SpreadMethodManager spreadMethodManager;
    [SerializeField] private CellInfoUIContentManager cellInfoUIContentManager;

    void Start()
    {
        uiManager.Init();
        cellInfoUIContentManager.Init();

        mapManager.BuildGrid();

        spreadMethodManager.Init();
        timeManager.StartMatch();
    }
}