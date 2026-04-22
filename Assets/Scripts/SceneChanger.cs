
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("Scene Names")]
    [Tooltip("Enter the exact names of your scenes as they appear in Build Settings")]
    public string scene1Name = "Scene1";
    public string scene2Name = "Scene2";
    public string scene3Name = "Scene3";

    [Header("Loading Options")]
    [Tooltip("Wait for scene to fully load before switching")]
    public bool waitForSceneLoad = true;

    // Public functions to change to each scene
    public void GoToScene1()
    {
        ChangeScene(scene1Name);
    }

    public void GoToScene2()
    {
        ChangeScene(scene2Name);
    }

    public void GoToScene3()
    {
        ChangeScene(scene3Name);
    }

    // Generic scene change function
    private void ChangeScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is empty or null!");
            return;
        }

        // Check if scene exists in build settings
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            if (waitForSceneLoad)
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                SceneManager.LoadSceneAsync(sceneName);
            }
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' not found! Make sure it's added to Build Settings.");
        }
    }

    // Optional: Change scene by build index instead of name
    public void GoToSceneByIndex(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError($"Scene index {sceneIndex} is out of range!");
        }
    }

    // Optional: Get current scene name
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    // Optional: Reload current scene
    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // Optional: Quit application (useful for main menu)
    public void QuitGame()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}