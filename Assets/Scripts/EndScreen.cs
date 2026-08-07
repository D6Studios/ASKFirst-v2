using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
public class EndScreen : MonoBehaviour
{
    public Image[] stars;

    public Sprite fullStar;
    public Sprite halfStar;
    public Sprite emptyStar;
    [SerializeField]
    private float starWaitTime = 0.1f;
    public Sprite correctCardSprite;
    public Sprite incorrectCardSprite;
    public GameObject cardPrefab;
    public Transform cardParent;
    [SerializeField] GameObject restartButton;
    void Start()
    {
        UpdateStars(GameManager.Instance.currentScore);
        UpdateCards(GameManager.Instance.mistakesMade);
        UpdateWin(GameManager.Instance.currentLevel, (int)GameManager.Instance.currentScore);
    }

    public void UpdateStars(float value)
    {
        StartCoroutine(IncrementStars(value));
    }
    IEnumerator IncrementStars(float targetScore)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].sprite = emptyStar;
        }
        for (int i = 0; i < stars.Length; i++)
        {
            int starValue = (i * 2) + 2;

            if (targetScore >= starValue)
            {
                stars[i].sprite = halfStar;
                yield return new WaitForSeconds(starWaitTime);
                stars[i].sprite = fullStar;
                yield return new WaitForSeconds(starWaitTime);
            }
            else if (targetScore == starValue - 1)
            {
                stars[i].sprite = halfStar;
                yield return new WaitForSeconds(starWaitTime);
            }
            else
            {
                stars[i].sprite = emptyStar;
            }
        }
        yield return null;
    }
    public void UpdateCards(List<Mistake> mistakes)
    {
        foreach (Mistake mistake in mistakes)
        {
            GameObject card = Instantiate(cardPrefab, cardParent);
            Debug.Log("Creating card for mistake: " + mistake.title);
            Debug.Log("Mistake details - Positive: " + mistake.positive + ", Title: " + mistake.title + ", Hint: " + mistake.hint);
            if (!mistake.positive)
            {
                card.GetComponent<EndScreenCard>().SetCard(mistake.title, mistake.hint, incorrectCardSprite, false);
            }
            else
            {
                card.GetComponent<EndScreenCard>().SetCard(mistake.title, mistake.hint, correctCardSprite, true);
            }
        }
    }
    public void ReturnToMainMenu()
    {
        StartCoroutine(GameManager.Instance.LoadScene("Assets/Scenes/MainMenu.unity"));
    }
    public void RestartLevel()
    {
        StartCoroutine(GameManager.Instance.LoadScene("Assets/Scenes/Level" + (GameManager.Instance.currentLevel) + ".unity"));
    }
    void UpdateWin(int currentLevel, int currentScore)
    {
        if (currentScore >= 10) // replace with your actual score threshold for winning
        {
            restartButton.SetActive(false); // Hide the restart button on win
            if (!(GameManager.Instance.levelUnlocked > currentLevel)) // Check if the next level is already unlocked
            {
                GameManager.Instance.levelUnlocked = currentLevel + 1; // Unlock the next level
            }
        }
        else
        {
            UpdateLose();
        }
    }
    void UpdateLose()
    {
        restartButton.SetActive(true); // Show the restart button on lose
        // Implement lose logic here if needed
    }
}
