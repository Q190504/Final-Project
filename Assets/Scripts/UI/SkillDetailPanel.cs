using TMPro;
using UnityEngine;

public class SkillDetailPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text skillDescriptionText;

    public void SetInfo(SkillDataSO skillData)
    {
        skillNameText.text = skillData.displayName;
        skillDescriptionText.text = skillData.description;
    }
}
