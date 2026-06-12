using TMPro;
using UnityEngine;

public class SpreadMethodInfoEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text textField;

    private void Reset()
    {
        textField = GetComponentInChildren<TMP_Text>();
    }

    public void Setup(string text)
    {
        textField.SetText(text);
    }
}
