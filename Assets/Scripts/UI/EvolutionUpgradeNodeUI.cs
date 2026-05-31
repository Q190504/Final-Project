using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EvolutionUpgradeNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image nodeIconImage;
    [SerializeField] private Sprite unselectedIcon;
    [SerializeField] private Image selectedBorder;

    private EvolutionTreePanel treePanel;
    private int nodeIndex;
    private bool isUnlocked;

    public void Initialize(EvolutionUpgradeNodeSO nodeData, EvolutionTreePanel treePanel, int index)
    {
        this.treePanel = treePanel;
        nodeIndex = index;

        nodeIconImage.sprite = unselectedIcon;

        SetSelected(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        treePanel.OnNodeHovered(this, nodeIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        treePanel.OnNodeUnhovered(this);
    }

    public void SetSelected(bool selected)
    {
        selectedBorder.gameObject.SetActive(selected);
    }

    public void SetState(EvolutionUpgradeNodeSO nodeData, bool isUnlocked)
    {
        this.isUnlocked = isUnlocked;
        nodeIconImage.sprite = isUnlocked ? nodeData.icon : unselectedIcon;
    }

    public bool IsUnlocked()
    {
        return isUnlocked;
    }
}