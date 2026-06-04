using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpreadMethodUI : MonoBehaviour
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
            if (cooldown > 1)
                cooldownText.text = $"{Mathf.CeilToInt(cooldown)}";
            else
                cooldownText.text = cooldown.ToString("0.#");
            cooldownText.gameObject.SetActive(true);
        }
        else if (cooldown <= 0)
        {
            cooldownText.gameObject.SetActive(false);
        }
    }
}
