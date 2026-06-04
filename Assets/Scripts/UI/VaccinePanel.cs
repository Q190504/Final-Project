using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VaccinePanel : MonoBehaviour
{
    [SerializeField] private Slider vaccineProgressSlider;
    [SerializeField] private TMP_Text vaccineProgressText;
    [SerializeField] private Image statusImage;

    [Header("Refs")]
    [SerializeField] private Sprite increaseStatusSprite;
    [SerializeField] private Color increaseStatusSpriteColor;
    [SerializeField] private Sprite decreaseStatusSprite;
    [SerializeField] private Color decreaseStatusSpriteColor;


    private void Start()
    {
        vaccineProgressText.SetText("0%");
        vaccineProgressSlider.value = 0;
    }

    public void UpdateData(float progress, bool isIncreasing)
    {
        vaccineProgressSlider.value = progress;
        vaccineProgressText.SetText($"{Mathf.FloorToInt(progress * 100)}%");

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        StartCoroutine(ShowStatusIcon(isIncreasing));
    }

    IEnumerator ShowStatusIcon(bool isIncreasing)
    {
        if (isIncreasing)
        {
            statusImage.sprite = increaseStatusSprite;
            statusImage.color = increaseStatusSpriteColor;
        }
        else
        {
            statusImage.sprite = decreaseStatusSprite;
            statusImage.color = decreaseStatusSpriteColor;
        }

        statusImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        statusImage.gameObject.SetActive(false);
    }
}
