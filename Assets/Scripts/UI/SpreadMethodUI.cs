using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpreadMethodUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SpreadMethodType methodType;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text cooldownText;

    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }

    public void SetCooldown(float cooldown)
    {
        if (cooldown > 0)
        {
            cooldownText.text = cooldown.ToString("0.#");
            cooldownText.gameObject.SetActive(true);
        }
        else if (cooldown <= 0)
        {
            cooldownText.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.ShowSpreadMethodDetailPanel(methodType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HideSpreadMethodDetailPanel();
    }
}
