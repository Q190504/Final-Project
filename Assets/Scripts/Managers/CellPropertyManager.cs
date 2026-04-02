using System.Collections.Generic;
using UnityEngine;

public class CellPropertyManager : MonoBehaviour
{
    public static CellPropertyManager Instance { get; private set; }

    [Header("Cell Property Data")]
    [SerializeField] private List<PopulationData> populationDatas;
    [SerializeField] private List<TempuratureData> tempuratureDatas;
    [SerializeField] private List<EnvironmentData> environmentDatas;
    [SerializeField] private List<StructureData> structureDatas;
    [SerializeField] private List<CellStageData> cellStageDatas;

    private Dictionary<PopulationType, PopulationData> populationDict;
    private Dictionary<TemperatureType, TempuratureData> tempuratureDict;
    private Dictionary<EnvironmentType, EnvironmentData> environmentDict;
    private Dictionary<StructureType, StructureData> structureDict;
    private Dictionary<CellStageType, CellStageData> cellStageDict;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        Init();
    }

    //// Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    private void Init()
    {
        populationDict = new();
        tempuratureDict = new();
        environmentDict = new();
        structureDict = new();
        cellStageDict = new();

        foreach (var entry in populationDatas)
            populationDict[entry.type] = entry;
        foreach (var entry in tempuratureDatas)
            tempuratureDict[entry.type] = entry;
        foreach (var entry in environmentDatas)
            environmentDict[entry.type] = entry;
        foreach (var entry in structureDatas)
            structureDict[entry.type] = entry;
        foreach (var entry in cellStageDatas)
            cellStageDict[entry.type] = entry;
    }

    public List<PopulationData> GetPopulationDatas() { return populationDatas; }

    public List<TempuratureData> GetTempuratureDatas() { return tempuratureDatas; }

    public List<EnvironmentData> GetEnvironmentDatas() { return environmentDatas; }

    public List<StructureData> GetStructureDatas() { return structureDatas; }

    public List<CellStageData> GetCellStageDatas() { return cellStageDatas; }

    public PopulationData GetPopulationData(PopulationType type)
    {
        return populationDict.TryGetValue(type, out var data) ? data : null;
    }

    public TempuratureData GetTempuratureData(TemperatureType type)
    {
        return tempuratureDict.TryGetValue(type, out var data) ? data : null;
    }

    public EnvironmentData GetEnvironmentData(EnvironmentType type)
    {
        return environmentDict.TryGetValue(type, out var data) ? data : null;
    }

    public StructureData GetStructureData(StructureType type)
    {
        return structureDict.TryGetValue(type, out var data) ? data : null;
    }

    public CellStageData GetCellStageData(CellStageType type)
    {
        return cellStageDict.TryGetValue(type, out var data) ? data : null;
    }
}
