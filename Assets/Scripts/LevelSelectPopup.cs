using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LevelSelectPopup : MonoBehaviour
{
    void EnableRenderer()
    {
        GetComponent<Image>().enabled = true;
    }
    void DisableRenderer()
    {
        GetComponent<Image>().enabled = false;
    }
    public void StartLevel()
    {
        string levelName = "Level" + (transform.parent.parent.GetComponent<LevelSelect>().currentLevel + 1);
        Debug.Log("Loading level: " + levelName);
        SceneManager.LoadScene(levelName);
    }
}
