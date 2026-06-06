using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("Found more than one Scene Manager in the scene. Destroying the newest one");
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Description:
    /// Loads a scene by name
    /// Input:
    /// string sceneName
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="sceneName">The name of the scene to be loaded</param>
    public static void LoadScene(string sceneName)
    {
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(sceneName);
    }

    public void OnGameExit()
    {
        Application.Quit();
    }
}
