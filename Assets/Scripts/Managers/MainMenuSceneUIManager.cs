using UnityEngine;

public class MainMenuSceneUIManager : MonoBehaviour
{
    public static MainMenuSceneUIManager Instance;

    [SerializeField] private GameObject confirmExitGamePanel;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetConfirmExitGamePanelVisibility(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetConfirmExitGamePanelVisibility(bool isVisible)
    {
        confirmExitGamePanel.SetActive(isVisible);
    }
}
