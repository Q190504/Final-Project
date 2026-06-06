using UnityEngine;

public class OpenSettingPanelButton : BaseButton
{
    [SerializeField] private BoolPublisherSO setSettingPanelSO;

    public void OnButtonPressed()
    {
        PlayClickSound();
        setSettingPanelSO.RaiseEvent(true);
    }
}
