using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HumanActionUI : MonoBehaviour
{
    public HumanActionType actionType;
    [SerializeField] private Image mainIcon;
    [SerializeField] private Image smallIcon;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private GameObject executedIcon;

    private Color mainIconOriginalColor;
    private Color smallIconOriginalColor;

    private Color mainIconDisableColor;
    private Color smallIconDisableColor;

    public void Start()
    {
        mainIconOriginalColor = mainIcon.color;
        smallIconOriginalColor = smallIcon.color;

        mainIconDisableColor = new Color(mainIconOriginalColor.r, mainIconOriginalColor.g, mainIconOriginalColor.b, mainIconOriginalColor.a / 2);
        smallIconDisableColor = new Color(smallIconOriginalColor.r, smallIconOriginalColor.g, smallIconOriginalColor.b, smallIconOriginalColor.a / 2);

        cooldownText.gameObject.SetActive(false);
        executedIcon.SetActive(false);
    }

    public void SetMainIcon(Sprite sprite)
    {
        mainIcon.sprite = sprite;
    }

    public void SetSmallIcon(Sprite sprite)
    {
        smallIcon.sprite = sprite;
    }

    public void SetCooldown(float cooldown)
    {
        executedIcon.SetActive(false);

        if (cooldown > 0)
        {
            cooldownText.text = Mathf.Ceil(cooldown).ToString();
            cooldownText.gameObject.SetActive(true);
        }
        else if (cooldown <= 0)
        {
            cooldownText.gameObject.SetActive(false);
        }
    }

    public void SetExecutedIconVisibility(bool isExecutedOnThisTick)
    {
        executedIcon.SetActive(isExecutedOnThisTick);
        cooldownText.gameObject.SetActive(!isExecutedOnThisTick);
    }

    public void SetVisibility(bool isShown)
    {
        if (!isShown)
        {
            mainIcon.color = mainIconDisableColor;
            smallIcon.color = smallIconDisableColor;
        }
        else
        {
            mainIcon.color = mainIconOriginalColor;
            smallIcon.color = smallIconOriginalColor;
        }
    }
}
