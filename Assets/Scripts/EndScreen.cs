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
    public void UpdateCards(Mistake[] mistakes)
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject currentCard = Instantiate(cardPrefab, cardParent);

            switch (i)
            {
                case 0: //A card
                    if (GameManager.Instance.mistakesMade[0] != null)
                        currentCard.GetComponent<EndScreenCard>().SetCard("a", GameManager.Instance.mistakesMade[0].description, GameManager.Instance.mistakesMade[0].positive);
                    else
                    {
                        currentCard.GetComponent<EndScreenCard>().SetCard("a", "", true);
                    }
                    break;
                case 1://S card
                    if (GameManager.Instance.mistakesMade[1] != null)
                        currentCard.GetComponent<EndScreenCard>().SetCard("s", GameManager.Instance.mistakesMade[1].description, GameManager.Instance.mistakesMade[1].positive);
                    else if (GameManager.Instance.mistakesMade[1] == null && GameManager.Instance.mistakesMade[0] == null && GameManager.Instance.mistakesMade[2] == null)
                    {
                        currentCard.GetComponent<EndScreenCard>().SetCard("s", "Good job! By implementing the A.S.K protocol and engaging the customer politely, maintaining safety coverage and discreetly monitoring their actions, you have successfully deterred potential theft without confrontation or disruption to the store.", true);
                    }
                    else
                    {
                        currentCard.GetComponent<EndScreenCard>().SetCard("s", "", true);
                    }
                    break;
                case 2://K card
                    if (GameManager.Instance.mistakesMade[2] != null)
                        currentCard.GetComponent<EndScreenCard>().SetCard("k", GameManager.Instance.mistakesMade[2].description, GameManager.Instance.mistakesMade[2].positive);
                    else
                    {
                        currentCard.GetComponent<EndScreenCard>().SetCard("k", "", true);
                    }
                    break;
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
