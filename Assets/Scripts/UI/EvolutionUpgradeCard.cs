using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionUpgradeCard : MonoBehaviour
{
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardDescription;

    [SerializeField] private VoidPublisherSO playSoundSO;

    private EvolutionUpgradeNodeSO upgradeNodeSO;
    private SpreadMethodType spreadMethodType;

    private bool isClickable = false;

    public void SetCardInfo(EvolutionUpgradeNodeSO upgradeNode)
    {
        cardName.text = upgradeNode.upgradeName;
        if (upgradeNode.icon != null)
            cardImage.sprite = upgradeNode.icon;
        cardDescription.text = upgradeNode.description;
        upgradeNodeSO = upgradeNode;
        spreadMethodType = SpreadMethodType.None;
        DelaySelected();
    }

    public void SetCardInfo(SpreadMethodDataSO methodDataSO)
    {
        cardName.text = methodDataSO.methodName;
        if (methodDataSO.methodIcon != null)
            cardImage.sprite = methodDataSO.methodIcon;
        cardDescription.text = methodDataSO.description;
        upgradeNodeSO = null;
        spreadMethodType = methodDataSO.baseConfig.methodType;
        DelaySelected();
    }

    private void DelaySelected()
    {
        isClickable = false;
        StartCoroutine(DelaySelectedCoroutine());
    }

    private IEnumerator DelaySelectedCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        isClickable = true;
    }

    public void Select()
    {
        if (isClickable)
        {
            isClickable = false;
            if (playSoundSO != null) playSoundSO.RaiseEvent();
            if (upgradeNodeSO != null)
                EvolutionManager.Instance.SelectUpgrade(upgradeNodeSO);
            else if (spreadMethodType != SpreadMethodType.None)
                EvolutionManager.Instance.SelectSpreadMethod(spreadMethodType);
        }
    }
}