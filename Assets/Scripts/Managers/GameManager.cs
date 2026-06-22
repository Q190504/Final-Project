using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("Found more than one Game Manager in the scene. Destroying the newest one");
            Destroy(gameObject);
        }
    }

    public void OnGameExit()
    {
        Application.Quit();
    }
}
