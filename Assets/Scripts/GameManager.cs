using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public float currentScore;
    public List<Mistake> mistakesMade;
    public List<Mistake> mistakes;
    private Coroutine levelTimerCoroutine;
    public int levelTimer;
    public GameObject player;
    Canvas dialogueUI;
    Canvas optionsUI;
    Canvas gameUI;
    GameObject optionsMenu;

    void Start()
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
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single); // Call OnSceneLoaded for the initial scene
        mistakes = new List<Mistake>();
        TextAsset mistakeList = Resources.Load<TextAsset>("ASKFirstMistakeList");
        string[] allLines = mistakeList.text.Split('\n');
        for (int i = 1; i < allLines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(allLines[i]))
            {
                continue; //Skip empty lines
            }
            Debug.Log("Parsing mistake line: " + allLines[i]);
            string[] splitLine = allLines[i].Split(',');
            mistakes.Add(new Mistake(int.Parse(splitLine[0]), splitLine[1], splitLine[2].Replace("|||", ","), bool.Parse(splitLine[3])));
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        dialogueUI = GameObject.FindGameObjectWithTag("DialogueUI").GetComponent<Canvas>();
        optionsUI = GameObject.FindGameObjectWithTag("OptionsUI").GetComponent<Canvas>();
        gameUI = GameObject.FindGameObjectWithTag("GameUI").GetComponent<Canvas>();
        optionsMenu = GameObject.FindGameObjectWithTag("OptionsMenu");
        if (scene.name == "MainMenu")
        {
            HideAll();
        }
        if (scene.name == "Level1" || scene.name == "Level2" || scene.name == "Level3")
        {
            StartLevel();
        }

    }

    public void StartLevel()
    {
        mistakesMade.Clear();
        DisplayNormalUI();
        levelTimerCoroutine = StartCoroutine(LevelTimer());

        player = GameObject.FindWithTag("Player");
    }
    public void EndLevel(float moodScore, string npcName)
    {
        Debug.Log("Level Ended with a mood score of: " + moodScore + " from NPC: " + npcName); //Placeholder for level end implementation
        GameManager.Instance.currentScore = moodScore;
        StopCoroutine(levelTimerCoroutine);
        SceneManager.LoadScene("Level End");
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
            mistakesMade.Add(mistake);
            Debug.Log("Added mistake: " + mistake.title);
        }
        else
        {
            Debug.LogWarning("Mistake with ID " + mistakeId + " not found.");
        }
    }
    IEnumerator LevelTimer()
    {
        levelTimer = 60;
        TextMeshProUGUI timerText = gameUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        timerText.text = levelTimer.ToString("D2") + "s"; // Display time in seconds with leading zero

        while (true)
        {
            timerText.text = levelTimer.ToString("D2") + "s"; // Display time in seconds with leading zero
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
        optionsMenu.SetActive(false);
    }

    public void PauseGame()
    {
        gameUI.enabled = false;
        Time.timeScale = 0f; // Pause the game
        optionsMenu.SetActive(true); // Show the options menu
    }
    public void ResumeGame()
    {
        gameUI.enabled = true;
        Time.timeScale = 1f; // Resume the game
        optionsMenu.SetActive(false); // Hide the options menu
    }
    public void HideAll()
    {
        dialogueUI.enabled = false;
        optionsUI.enabled = false;
        gameUI.enabled = false;
        optionsMenu.SetActive(false);
    }
}
