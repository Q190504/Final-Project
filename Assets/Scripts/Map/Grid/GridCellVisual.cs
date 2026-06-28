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
    [SerializeField] private SpriteRenderer targetedBySkillOverlayRenderer;

    [Header("Icons")]
    [SerializeField] private SpriteRenderer detectedIcon;
    [SerializeField] private SpriteRenderer structureIcon;
    [SerializeField] private SpriteRenderer carrierIcon;

    [Header("VFXs")]
    [SerializeField] private GameObject inspectingVFX;
    [SerializeField] private GameObject startingCellFocusVFX;

    [Header("Debug")]
    [SerializeField] private TMP_Text text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lockdownOverlayRenderer.gameObject.SetActive(false);
        affectedByStructureOverlayRenderer.gameObject.SetActive(false);
        targetedBySkillOverlayRenderer.gameObject.SetActive(false);
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
        if (gridCell == null) return;

        CellStats cellStats = gridCell.Stats;

        EnvironmentData envData = PropertyDataManager.Instance.GetEnvironmentData(cellStats.environment.currentEnvironmentType);
        if (envData != null && environmentRenderer != null)
            environmentRenderer.sprite = envData.sprite;

        StructureDataSO strucData = PropertyDataManager.Instance.GetStructureData(cellStats.structure.type);
        if (strucData != null && structureIcon != null)
        {
            if (cellStats.structure.type == StructureType.None)
            {
                structureIcon.sprite = null;
                structureIcon.gameObject.SetActive(false);
            }
            else
            {
                structureIcon.gameObject.SetActive(true);
                structureIcon.sprite = strucData.sprite;
                structureIcon.color = new Color(structureIcon.color.r, structureIcon.color.g, structureIcon.color.b,
                    cellStats.structure.isActive ? 1f : 0.5f);
            }
        }

        CellStageData cellStageData = PropertyDataManager.Instance.GetCellStageData(cellStats.stage.type);

        if (cellStageData != null && cellStageBorderRenderer != null)
            cellStageBorderRenderer.sprite = cellStageData.sprite;

        if (detectedIcon != null)
            detectedIcon.gameObject.SetActive(cellStats.isDetected);

        if (carrierIcon != null)
            carrierIcon.gameObject.SetActive(cellStats.hasCarrier);

        if (lockdownOverlayRenderer != null)
            lockdownOverlayRenderer.gameObject.SetActive(cellStats.isLockdown);

        //text.text = $"dis ur {gridCell.distanceToNearestUrban}\n dis wa {gridCell.distanceToNearestWaterRegion}\n wa sc {gridCell.nearestWaterRegionAssistScore}";
    }

    public void SetAffectedByStructureOverlayVisibility(bool state)
    {
        if (affectedByStructureOverlayRenderer != null)
            affectedByStructureOverlayRenderer.gameObject.SetActive(state);
    }

    public void SetAffectedBySkillOverlayVisibility(bool state)
    {
        if (targetedBySkillOverlayRenderer != null)
            targetedBySkillOverlayRenderer.gameObject.SetActive(state);
    }

    public void SetInspectingVFXVisibility(bool state)
    {
        if (inspectingVFX != null)
            inspectingVFX.SetActive(state);
    }

    public void SetStartingCellFocusVFXVisibility(bool state)
    {
        if (startingCellFocusVFX != null)
            startingCellFocusVFX.SetActive(state);
    }
}
