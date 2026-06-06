using UnityEngine;

public class LoadSceneButton : BaseButton
{
    [SerializeField] private StringPublisherSO loadLevelSO;
    [SerializeField] private VoidPublisherSO setBGMWhenLoadLevelSO;

    public void OnButtonPressed(string levelName)
    {
        PlayClickSound();
        setBGMWhenLoadLevelSO.RaiseEvent();
        loadLevelSO.RaiseEvent(levelName);
    }
}
