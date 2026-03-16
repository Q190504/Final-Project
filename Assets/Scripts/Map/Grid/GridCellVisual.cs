using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridCellVisual : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private SpriteRenderer cellStageBorderRenderer;
    [SerializeField] private SpriteRenderer environmentRenderer;
    [SerializeField] private SpriteRenderer lockdownOverlayRenderer;
    [SerializeField] private SpriteRenderer affectedByStructureOverlayRenderer;
    [SerializeField] private SpriteRenderer affectedBySkillOverlayRenderer;

    [Header("Icons")]
    [SerializeField] private SpriteRenderer detectedIcon;
    [SerializeField] private SpriteRenderer structureIcon;
    [SerializeField] private SpriteRenderer carrierIcon;

    [Header("Icons")]
    [SerializeField] private GameObject cellFocusVFX;

    [Header("Debug")]
    [SerializeField] private TMP_Text text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lockdownOverlayRenderer.gameObject.SetActive(false);
        affectedByStructureOverlayRenderer.gameObject.SetActive(false);
        affectedBySkillOverlayRenderer.gameObject.SetActive(false);
        cellFocusVFX.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ApplyEnvironmentRenderer(EnvironmentData env)
    {
        environmentRenderer.sprite = env.sprite;
    }

    public void UpdateVisual(GridCell gridCell)
    {
        CellStats cellStats = gridCell.Stats;

        EnvironmentData envData = CellPropertyManager.Instance.GetEnvironmentData(cellStats.environment.currentEnvironmentType);
        if (envData != null)
            environmentRenderer.sprite = envData.sprite;

        StructureData strucData = CellPropertyManager.Instance.GetStructureData(cellStats.structure.type);
        if (strucData != null)
            structureIcon.sprite = strucData.sprite;

        CellStageData cellStageData = CellPropertyManager.Instance.GetCellStageData(cellStats.stage.type);
        if (cellStageData != null)
            cellStageBorderRenderer.sprite = cellStageData.sprite;

        //text.text = $"Popu: {cellStats.population.type},\nTem: {cellStats.tempurature.type}";
    }

    public void OnCellSelected(int x, int y)
    {
        cellFocusVFX.SetActive(true);
    }
}
