using UnityEngine;

public class OpenConfirmExitGamePanelButton : BaseButton
{
    [SerializeField] private BoolPublisherSO openConfirmExitGamePanel;

    public void OnButtonPressed()
    {
        PlayClickSound();
        openConfirmExitGamePanel.RaiseEvent(true);
    }
}