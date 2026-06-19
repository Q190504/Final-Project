using System.Collections.Generic;
using UnityEngine;

public class PropertyDataManager : MonoBehaviour
{
    public static PropertyDataManager Instance { get; private set; }

    [Header("Cell Property Data")]
    [SerializeField] private List<PopulationData> populationDatas;
    [SerializeField] private List<TemperatureData> tempuratureDatas;
    [SerializeField] private List<EnvironmentData> environmentDatas;
    [SerializeField] private List<StructureDataSO> structureDatas;
    [SerializeField] private List<CellStageData> cellStageDatas;
    [SerializeField] private List<ThreatTierSO> threatTierDatas;

    private Dictionary<PopulationType, PopulationData> populationDict;
    private Dictionary<TemperatureType, TemperatureData> tempuratureDict;
    private Dictionary<EnvironmentType, EnvironmentData> environmentDict;
    private Dictionary<StructureType, StructureDataSO> structureDict;
    private Dictionary<CellStageType, CellStageData> cellStageDict;
    private Dictionary<ThreatTier, ThreatTierSO> threatTierDict;

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

        CreateThreatTierDictAndCreateActions();
    }

    private void CreateThreatTierDictAndCreateActions()
    {
        threatTierDict = new Dictionary<ThreatTier, ThreatTierSO>();

        foreach (ThreatTierSO tier in threatTierDatas)
        {
            threatTierDict[tier.tierType] = tier;
        }
    }

    public List<PopulationData> GetPopulationDatas() { return populationDatas; }

    public List<TemperatureData> GetTempuratureDatas() { return tempuratureDatas; }

    public List<EnvironmentData> GetEnvironmentDatas() { return environmentDatas; }

    public List<StructureDataSO> GetStructureDatas() { return structureDatas; }

    public List<CellStageData> GetCellStageDatas() { return cellStageDatas; }

    public List<ThreatTierSO> GetThreatTierSOs() { return threatTierDatas; }

    public PopulationData GetPopulationData(PopulationType type)
    {
        return populationDict.TryGetValue(type, out var data) ? data : null;
    }

    public TemperatureData GetTempuratureData(TemperatureType type)
    {
        return tempuratureDict.TryGetValue(type, out var data) ? data : null;
    }

    public EnvironmentData GetEnvironmentData(EnvironmentType type)
    {
        return environmentDict.TryGetValue(type, out var data) ? data : null;
    }

    public StructureDataSO GetStructureData(StructureType type)
    {
        return structureDict.TryGetValue(type, out var data) ? data : null;
    }

    public CellStageData GetCellStageData(CellStageType type)
    {
        return cellStageDict.TryGetValue(type, out var data) ? data : null;
    }

    public ThreatTierSO GetThreatTierData(ThreatTier tier)
    {
        return threatTierDict.TryGetValue(tier, out var data) ? data : null;
    }
}
