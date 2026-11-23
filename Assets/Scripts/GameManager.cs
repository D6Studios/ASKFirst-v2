using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
public class GameManager : MonoBehaviour
{
    public int currentLevel = 0;
    public static GameManager Instance { get; private set; }
    public float currentScore;
    public Mistake[] mistakesMade;
    public List<Mistake> mistakes;
    public GameObject player;
    Canvas dialogueUI;
    Canvas optionsUI;
    Canvas gameUI;
    Animator optionsMenu;
    [SerializeField] private AudioMixer audioMixer;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        GameSettings.ApplyVolumes(audioMixer);
        mistakes = Mistake.ParseAll(RemoteLoader.Instance.mistakesContent);
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single); // Call OnSceneLoaded for the initial scene
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Get UI references
        //
        Time.timeScale = 1f; // Reset time scale to normal when a new scene is loaded
        Debug.Log("Scene loaded: " + scene.name);
        try
        {
            dialogueUI = GameObject.FindGameObjectWithTag("DialogueUI").GetComponent<Canvas>();

        }
        catch (System.Exception e)
        {
            Debug.LogWarning("DialogueUI not found in the scene: " + e.Message);
        }
        try
        {
            optionsUI = GameObject.FindGameObjectWithTag("OptionsUI").GetComponent<Canvas>();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("OptionsUI not found in the scene: " + e.Message);
        }
        try
        {
            gameUI = GameObject.FindGameObjectWithTag("GameUI").GetComponent<Canvas>();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("GameUI not found in the scene: " + e.Message);
        }
        try
        {
            optionsMenu = GameObject.FindGameObjectWithTag("OptionsUI").GetComponent<Animator>();
            Debug.Log("OptionsMenu found and assigned in GameManager.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("OptionsMenu not found in the scene: " + e.Message);
        }
        if (scene.name == GameLevels.MainMenu)
        {
            if (optionsMenu == null) Debug.LogWarning("Check1");

            HideAll();
        }
        if (GameLevels.IsLevel(scene.name))
        {
            LevelObjectives levelObjectives = gameObject.GetComponent<LevelObjectives>();
            LevelIntro levelIntro = FindObjectOfType<LevelIntro>();
            if (levelIntro != null)
            {
                levelIntro.PlayIntro(() =>
                {
                    StartLevel();
                    levelObjectives.FindUI();
                });
            }
            else
            {
                StartLevel();
                levelObjectives.FindUI();
            }
        }
        if (scene.name == GameLevels.Tutorial)
        {
            StartTutorial();
        }



    }

    public void StartLevel()
    {
        SoundManager.Instance.PlayBGM(Resources.Load<AudioClip>("BGM"));
        ResetMistakes();
        DisplayNormalUI();
        gameObject.GetComponent<LevelObjectives>().ResetObjectives();
        player = GameObject.FindWithTag("Player");
    }
    public void StartTutorial()
    {
        SoundManager.Instance.PlayBGM(Resources.Load<AudioClip>("BGM"));
        ResetMistakes();
        DisplayNormalUI();
        player = GameObject.FindWithTag("Player");
        gameObject.GetComponent<LevelObjectives>().ResetObjectives();
        GameObject.Find("TutorialManager").GetComponent<TutorialManager>().StartTutorial();

    }
    public void EndLevel(float moodScore, string npcName)
    {
        SoundManager.Instance.StopBGM();
        Debug.Log("Level Ended with a mood score of: " + moodScore + " from NPC: " + npcName); //Placeholder for level end implementation
        GameManager.Instance.currentScore = moodScore;
        StartCoroutine(LoadScene(GameLevels.ScenePath(GameLevels.LevelEnd)));
    }
    // One slot per category (a, s, k); id -1 = no mistake recorded.
    public void ResetMistakes()
    {
        mistakesMade = new Mistake[3];
        for (int i = 0; i < mistakesMade.Length; i++)
            mistakesMade[i] = new Mistake("", "", true, -1);
    }
    public void AddMistake(int mistakeId)
    {
        if (mistakeId == -1)
        {
            Debug.Log("No mistake for this option.");
            return; //No mistake for this option
        }
        Mistake mistake = mistakes.Find(m => m.id == mistakeId);
        if (mistake != null)
        {
            if (mistake.catagory == "a")
            {
                mistakesMade[0] = mistake;
            }
            else if (mistake.catagory == "s")
            {
                mistakesMade[1] = mistake;
            }
            else if (mistake.catagory == "k")
            {
                mistakesMade[2] = mistake;
            }

        }
        else
        {
            Debug.LogWarning("Mistake with ID " + mistakeId + " not found.");
        }
    }
    public void DisplayDialogueUI()
    {
        dialogueUI.enabled = true;
        optionsUI.enabled = false;
        gameUI.enabled = false;
    }
    public void DisplayNormalUI()
    {
        dialogueUI.enabled = false;
        optionsUI.enabled = true;
        gameUI.enabled = true;
        optionsMenu.SetBool("IsOpen", false);
    }

    public void PauseGame()
    {
        gameUI.enabled = false;
        Time.timeScale = 0f; // Pause the game
        optionsUI.enabled = true;
        optionsMenu.SetBool("IsOpen", true); // Show the options menu
    }
    public void ResumeGame()
    {
        gameUI.enabled = true;
        Time.timeScale = 1f; // Resume the game
        optionsMenu.SetBool("IsOpen", false); // Hide the options menu
    }
    public void HideAll()
    {
        dialogueUI.enabled = false;
        optionsUI.enabled = false;
        gameUI.enabled = false;
        if (optionsMenu == null) Debug.LogWarning("OptionsMenu is null in HideAll.");
        optionsMenu.SetBool("IsOpen", false);
    }
    public IEnumerator LoadScene(string sceneName)
    {
        var asyncLoad = Addressables.LoadSceneAsync(sceneName);
        while (!asyncLoad.IsDone)
        {
            yield return null;
        }
    }
}

