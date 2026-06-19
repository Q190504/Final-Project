using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionTreePanel : MonoBehaviour
{
    [SerializeField] private EvolutionUpgradeNodeUI nodePrefab;
    [SerializeField] private GridLayoutGroup nodeContainer;
    [SerializeField] private TMP_Text methodName;
    [SerializeField] private Image methodIcon;
    [SerializeField] private Image nodeIconImage;
    [SerializeField] private TMP_Text nodeNameText;
    [SerializeField] private TMP_Text nodeDescriptionText;

    private List<EvolutionUpgradeNodeSO> nodeDatas;

    private EvolutionUpgradeNodeUI hoveredNode;
    private bool isInitialized = false;

    public void Initialize(List<EvolutionUpgradeNodeSO> nodeDatas, string methodName, Sprite methodIcon)
    {
        ClearNodeUI();

        this.methodName.SetText(methodName);
        this.methodIcon.sprite = methodIcon;

        this.nodeDatas = new List<EvolutionUpgradeNodeSO>(nodeDatas);
        for (int i = 0; i < nodeDatas.Count; i++)
        {
            EvolutionUpgradeNodeUI nodeUI = Instantiate(nodePrefab, nodeContainer.transform);
            nodeUI.Initialize(nodeDatas[i], this, i);
        }

        SetNodeInfoUIElementsVisibility(false);

        isInitialized = true;
    }

    public void SetPanelVisibility(bool isVisible, List<EvolutionTierInfo> tierInfos)
    {
        SetNodeInfoUIElementsVisibility(false);

        if (isVisible && !isInitialized)
        {
            Debug.LogError("EvolutionTreePanel is not initialized.");
            return;
        }

        if (isVisible && tierInfos != null)
        {
            // Ignore tier 0 (select method tier)
            for (int tier = 1; tier < tierInfos.Count; tier++)
            {
                EvolutionTierInfo tierInfo = tierInfos[tier];

                // every tier has 2 nodes
                int nodeStartIndex = (tier - 1) * 2;

                for (int nodeOffset = 0; nodeOffset < 2; nodeOffset++)
                {
                    int nodeIndex = nodeStartIndex + nodeOffset;

                    if (nodeIndex >= nodeContainer.transform.childCount || nodeIndex >= nodeDatas.Count)
                    {
                        continue;
                    }

                    if (nodeContainer.transform.GetChild(nodeIndex).TryGetComponent<EvolutionUpgradeNodeUI>(out var ui))
                    {
                        ui.SetState(nodeDatas[nodeIndex], tierInfo.selectedNode == nodeDatas[nodeIndex]);
                    }
                }
            }
        }
    }

    private void OnNodeSelected(int index, bool showInfo)
    {
        if (showInfo)
        {
            SetNodeInfoUIElementsVisibility(true);
            EvolutionUpgradeNodeSO nodeData = nodeDatas[index];
            nodeIconImage.sprite = nodeData.icon;
            nodeNameText.text = nodeData.upgradeName;
            nodeDescriptionText.text = nodeData.description;
        }
        else
            SetNodeInfoUIElementsVisibility(false);
    }

    private void OnNodeDeselected()
    {
        SetNodeInfoUIElementsVisibility(false);
    }

    public void OnNodeHovered(EvolutionUpgradeNodeUI node, int index)
    {
        // unhover previous node
        if (hoveredNode != null)
            hoveredNode.SetSelected(false);

        hoveredNode = node;
        hoveredNode.SetSelected(true);

        SetNodeInfoUIElementsVisibility(true);
        OnNodeSelected(index, hoveredNode.IsUnlocked());
    }

    public void OnNodeUnhovered(EvolutionUpgradeNodeUI node)
    {
        // only remove if this is current hovered node
        if (hoveredNode == node)
        {
            hoveredNode.SetSelected(false);
            hoveredNode = null;
            OnNodeDeselected();
        }
    }

    private void SetNodeInfoUIElementsVisibility(bool visible)
    {
        //nodeIconImage.gameObject.SetActive(visible);
        nodeNameText.gameObject.SetActive(visible);
        nodeDescriptionText.gameObject.SetActive(visible);
    }

    private void ClearNodeUI()
    {
        foreach (Transform child in nodeContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public bool IsInitialized()
    {
        return isInitialized;
    }
}
