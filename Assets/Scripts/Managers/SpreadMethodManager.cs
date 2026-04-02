using System.Collections.Generic;
using UnityEngine;

public class SpreadMethodManager : MonoBehaviour
{
    public static SpreadMethodManager Instance;

    [SerializeField] private List<SpreadMethodDataSO> spreadMethodDatas;

    private List<ISpreadMethod> spreadMethods = new();

    private Dictionary<SpreadMethodType, SpreadMethodDataSO> spreadMethodDataDict;

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

    public List<SpreadMethodDataSO> GetAllSpreadMethodDatas()
    {
        return spreadMethodDatas;
    }

    public void CreateDicts()
    {
        if (spreadMethodDataDict == null)
        {
            spreadMethodDataDict = new Dictionary<SpreadMethodType, SpreadMethodDataSO>();
            foreach (SpreadMethodDataSO data in spreadMethodDatas)
            {
                spreadMethodDataDict[data.baseConfig.methodType] = data;
            }
        }
    }

    public SpreadMethodDataSO GetSpreadMethodData(SpreadMethodType type)
    {
        if (spreadMethodDataDict.TryGetValue(type, out SpreadMethodDataSO data))
        {
            return data;
        }
        else
        {
            Debug.LogError($"Spread method data not found for type {type}");
            return null;
        }
    }

    public void Init()
    {
        CreateDicts();
        CreateSpreadMethods();
    }

    public void CreateSpreadMethods()
    {
        spreadMethods.Clear();

        Grid<GridCell> grid = MapManager.Instance.GetGrid();

        foreach (SpreadMethodDataSO methodData in spreadMethodDatas)
        {
            ISpreadMethod method = methodData.CreateMethod(grid);
            method.Initialize();
            spreadMethods.Add(method);
        }

        spreadMethods.Sort((a, b) =>
            a.GetConfig().methodOrder.CompareTo(b.GetConfig().methodOrder));
    }

    public void StartMethods()
    {
        foreach (ISpreadMethod method in spreadMethods)
        {
            method.Start();
        }
    }
}
