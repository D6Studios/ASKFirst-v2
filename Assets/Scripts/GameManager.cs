using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public float currentScore;
    public List<Mistake> mistakesMade;
    public List<Mistake> mistakes;
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
    public void StartLevel()
    {
        mistakesMade.Clear();
    }
    public void EndLevel(float moodScore, string npcName)
    {
        Debug.Log("Level Ended with a mood score of: " + moodScore + " from NPC: " + npcName); //Placeholder for level end implementation
        GameManager.Instance.currentScore = moodScore;
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
}
