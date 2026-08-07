using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
public class Transition : MonoBehaviour
{
    public Canvas mainMenu;
    public Canvas levelSelect;
    public Canvas videoMenu;
    public GameObject pauseOverlay;
    public Canvas formSGUI;
    [SerializeField] private LevelSelect levelSelectScript;

    void Start()
    {
        videoMenu.enabled = false;
        mainMenu.enabled = true;
        levelSelect.enabled = false;
        formSGUI.enabled = false;
        GameManager.Instance.HideAll();
        videoMenu.transform.GetChild(1).GetComponent<VideoPlayer>().loopPointReached += OnVideoFinished;
        if (GameManager.Instance.levelUnlocked == 5)
        {
            formSGUI.enabled = true;
        }

    }
    public void SwitchToLevelSelect()
    {
        levelSelectScript.UpdateLevelUnlock(GameManager.Instance.levelUnlocked);
        if (!GameManager.Instance.viewedTrainingVideo)
        {
            videoMenu.enabled = true;
            mainMenu.enabled = false;
            levelSelect.enabled = false;
            videoMenu.transform.GetChild(1).GetComponent<VideoPlayer>().Play();
            pauseOverlay.SetActive(false);
            GameManager.Instance.viewedTrainingVideo = true;
        }
        else
        {
            mainMenu.enabled = false;
            levelSelect.enabled = true;
        }
        mainMenu.enabled = false;
        levelSelect.enabled = true;
    }
    public void PauseVideo()
    {
        if (videoMenu.transform.GetChild(1).GetComponent<VideoPlayer>().isPlaying)
        {
            videoMenu.transform.GetChild(1).GetComponent<VideoPlayer>().Pause();
            pauseOverlay.SetActive(true);
        }
        else
        {
            videoMenu.transform.GetChild(1).GetComponent<VideoPlayer>().Play();
            pauseOverlay.SetActive(false);
        }
    }
    public void skipVideo()
    {
        videoMenu.enabled = false;
        videoMenu.transform.GetChild(1).GetComponent<VideoPlayer>().Stop();

    }
    public void OptionsMenu()
    {
        GameManager.Instance.PauseGame();
    }
    public void OnVideoFinished(VideoPlayer vp)
    {
        videoMenu.enabled = false;
        levelSelect.enabled = true;
        videoMenu.transform.GetChild(1).GetComponent<VideoPlayer>().loopPointReached -= OnVideoFinished;
    }
}
