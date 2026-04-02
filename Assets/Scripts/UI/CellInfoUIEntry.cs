using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellInfoUIEntry : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text textField;

    private void Reset()
    {
        textField = GetComponentInChildren<TMP_Text>();
    }

    public void Setup(string text, Sprite icon = null)
    {
        if (icon != null)
            image.sprite = icon;
        textField.SetText(text);
    }
}
