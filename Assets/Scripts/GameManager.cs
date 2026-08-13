using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.AddressableAssets;
public class GameManager : MonoBehaviour
{
    public int currentLevel = 0;
    public bool viewedTrainingVideo = false;
    public int levelUnlocked = 5;
    public static GameManager Instance { get; private set; }
    public float currentScore;
    public Mistake[] mistakesMade;
    public List<Mistake> mistakes;
    private Coroutine levelTimerCoroutine;
    public int levelTimer;
    public GameObject player;
    Canvas dialogueUI;
    Canvas optionsUI;
    Canvas gameUI;
    Animator optionsMenu;
    private MobileControls mobileControls;
    public float Sensitivity = 1.0f;
    private int maxTime = 60; // Maximum time for the level in seconds

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

        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single); // Call OnSceneLoaded for the initial scene
        mistakes = new List<Mistake>();
        TextAsset mistakeList = Resources.Load<TextAsset>("ASKFirstMistakes");
        string[] allLines = mistakeList.text.Split('\n');
        for (int i = 1; i < allLines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(allLines[i]))
            {
                continue; //Skip empty lines
            }
            // Debug.Log("Parsing mistake line: " + allLines[i]);
            string[] splitLine = allLines[i].Split('\t');

            mistakes.Add(new Mistake(
                splitLine[0],
                splitLine[1],
                bool.Parse(splitLine[2]),
                int.Parse(splitLine[3])));
        }
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
        if (scene.name == "MainMenu")
        {
            if (optionsMenu == null) Debug.LogWarning("Check1");

            HideAll();
        }
        if (scene.name == "Level1" || scene.name == "Level2" || scene.name == "Level3" || scene.name == "Level4" || scene.name == "Level5")
        {
            StartLevel();
            LevelObjectives levelObjectives = gameObject.GetComponent<LevelObjectives>();
            levelObjectives.FindUI();
        }
        if (scene.name == "Tutorial")
        {
            StartTutorial();
        }



    }

    public void StartLevel()
    {
        SoundManager.Instance.PlayBGM(Resources.Load<AudioClip>("BGM"));
        mistakesMade = new Mistake[3];
        for (int i = 0; i < mistakesMade.Length; i++)
        {
            mistakesMade[i] = new Mistake("", "", true, -1); // Initialize with default values
        }
        DisplayNormalUI();
        //levelTimerCoroutine = StartCoroutine(LevelTimer());
        gameObject.GetComponent<LevelObjectives>().ResetObjectives();
        player = GameObject.FindWithTag("Player");
    }
    public void StartTutorial()
    {
        SoundManager.Instance.PlayBGM(Resources.Load<AudioClip>("BGM"));
        mistakesMade = new Mistake[3];
        DisplayNormalUI();
        //levelTimerCoroutine = StartCoroutine(LevelTimer());
        player = GameObject.FindWithTag("Player");
        gameObject.GetComponent<LevelObjectives>().ResetObjectives();
        GameObject.Find("TutorialManager").GetComponent<TutorialManager>().StartTutorial();

    }
    public void EndLevel(float moodScore, string npcName)
    {
        SoundManager.Instance.StopBGM();
        Debug.Log("Level Ended with a mood score of: " + moodScore + " from NPC: " + npcName); //Placeholder for level end implementation
        GameManager.Instance.currentScore = moodScore;
        //StopCoroutine(levelTimerCoroutine);
        StartCoroutine(LoadScene("Assets/Scenes/Level End.unity"));
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
    IEnumerator LevelTimer()
    {
        levelTimer = maxTime;
        TextMeshProUGUI timerText = gameUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        GameObject timerObject = gameUI.transform.GetChild(2).gameObject;
        timerText.text = levelTimer.ToString("D2") + "s"; // Display time in seconds with leading zero

        while (true)
        {
            timerText.text = levelTimer.ToString("D2") + "s"; // Display time in seconds with leading zero
            timerObject.transform.rotation = Quaternion.Euler(0, 0, (360 / maxTime * levelTimer));
            yield return new WaitForSeconds(1f);
            if (!player.GetComponent<PlayerInteraction>().isBusy)
            {
                levelTimer--;
            }
            if (levelTimer <= 0) // Example: End level after 120 seconds
            {

                EndLevel(player.GetComponent<PlayerDialogue>().mood, "");
                yield break;
            }

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

