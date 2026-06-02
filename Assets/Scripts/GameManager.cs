using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public float currentScore;
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

        /// Placeholder for testing, remove when implementing actual mistake tracking
        mistakes = new List<Mistake>();
        mistakes.Add(new Mistake(true, "Area for Improvement", "Do not accuse customers directly!"));
        mistakes.Add(new Mistake(false, "Correct Choice", "You handled the interaction using the ASK Framework! Good job!"));
        mistakes.Add(new Mistake(true, "Area for Improvement", "This is a placeholder! This comments have not yet been implemented."));
    }
    public void EndLevel(float moodScore, string npcName)
    {
        Debug.Log("Level Ended with a mood score of: " + moodScore + " from NPC: " + npcName); //Placeholder for level end implementation
        GameManager.Instance.currentScore = moodScore;
        SceneManager.LoadScene("Level End");
    }
}
