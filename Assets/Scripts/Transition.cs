using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
public class Transition : MonoBehaviour
{
    public Canvas mainMenu;
    public Canvas levelSelect;
    public Canvas videoMenu;
    void Start()
    {
        videoMenu.enabled = true;
        mainMenu.enabled = false;
        levelSelect.enabled = false;
    }
    public void SwitchToLevelSelect()
    {
        mainMenu.enabled = false;
        levelSelect.enabled = true;
        SceneManager.LoadScene("Level1_Scene");
    }
    public void skipVideo()
    {
        videoMenu.enabled = false;
        videoMenu.transform.GetChild(1).GetComponent<VideoPlayer>().Stop();
        mainMenu.enabled = true;

    }
}
