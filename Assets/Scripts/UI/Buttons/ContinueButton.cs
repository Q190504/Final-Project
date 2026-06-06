using UnityEngine;

public class ContinueButton : BaseButton
{
    [SerializeField] private BoolPublisherSO setSettingPanelSO;

    public void OnButtonPressed()
    {
        PlayClickSound();
        setSettingPanelSO.RaiseEvent(false);
    }
}
