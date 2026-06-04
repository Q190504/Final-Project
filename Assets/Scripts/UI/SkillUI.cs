using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image mainIcon;
    [SerializeField] private Image secondaryIcon;
    [SerializeField] private GameObject cooldownContainer;
    [SerializeField] private Image cooldownSprite;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private TMP_Text costText;

    [HideInInspector]
    public SkillType skillType;

    private void Start()
    {
        cooldownContainer.SetActive(false);
    }

    public void SetData(SkillDataSO skillData)
    {
        skillType = skillData.type;
        mainIcon.sprite = skillData.mainIcon;

        if (skillData.secondaryIcon != null)
        {
            secondaryIcon.sprite = skillData.secondaryIcon;
            secondaryIcon.color = skillData.secondaryIconColor;
            secondaryIcon.gameObject.SetActive(true);
        }
        else 
            secondaryIcon.gameObject.SetActive(false);

        costText.text = skillData.infectionPointCost.ToString();
    }

    public void SetCooldown(float remaining, float originalDuration)
    {
        if (remaining > 0)
        {
            if (remaining > 1)
                cooldownText.text = $"{Mathf.CeilToInt(remaining)}";
            else
                cooldownText.text = remaining.ToString("0.#");

            cooldownSprite.fillAmount = remaining / originalDuration;
            cooldownContainer.SetActive(true);
        }
        else if (remaining <= 0)
        {
            cooldownText.text = "0";
            cooldownSprite.fillAmount = 0;
            cooldownContainer.SetActive(false);
        }
    }

    public void OnClick()
    {
        SkillManager.Instance.TryUseSkill(skillType);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.ShowSkillDetailPanel(skillType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HideSkillDetailPanel();
    }
}
