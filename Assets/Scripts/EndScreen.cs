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
    public int starScore = 0;
    int currentLevel;
    void Start()
    {
        UpdateCards(GameManager.Instance.mistakesMade);
        currentLevel = GameManager.Instance.currentLevel;
    }

    public void UpdateStars()
    {
        StartCoroutine(IncrementStars(starScore));
    }
    IEnumerator IncrementStars(float targetScore)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].sprite = emptyStar;
        }
        for (int i = 0; i < stars.Length; i++)
        {
            if (i < targetScore)
            {
                stars[i].sprite = fullStar;
            }
            else
            {
                stars[i].sprite = emptyStar;
            }
            yield return new WaitForSeconds(starWaitTime);
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
                    if (GameManager.Instance.mistakesMade[0].id != -1)
                        currentCard.GetComponent<EndScreenCard>().SetCard("a", GameManager.Instance.mistakesMade[0].description, GameManager.Instance.mistakesMade[0].positive);
                    else
                    {
                        currentCard.GetComponent<EndScreenCard>().SetCard("a", "", true);
                        starScore += 1;
                    }
                    break;
                case 1://S card
                    if (GameManager.Instance.mistakesMade[1].id != -1)
                        currentCard.GetComponent<EndScreenCard>().SetCard("s", GameManager.Instance.mistakesMade[1].description, GameManager.Instance.mistakesMade[1].positive);
                    else if (GameManager.Instance.mistakesMade[1].id == -1 && GameManager.Instance.mistakesMade[0].id == -1 && GameManager.Instance.mistakesMade[2].id == -1)
                    {
                        currentCard.GetComponent<EndScreenCard>().SetCard("s", "Good job! By implementing the A.S.K protocol and engaging the customer politely, maintaining safety coverage and discreetly monitoring their actions, you have successfully deterred potential theft without confrontation or disruption to the store.", true);
                        starScore += 1;

                    }
                    else
                    {
                        currentCard.GetComponent<EndScreenCard>().SetCard("s", "", true);
                        starScore += 1;
                    }
                    break;
                case 2://K card
                    if (GameManager.Instance.mistakesMade[2].id != -1)
                        currentCard.GetComponent<EndScreenCard>().SetCard("k", GameManager.Instance.mistakesMade[2].description, GameManager.Instance.mistakesMade[2].positive);
                    else
                    {
                        currentCard.GetComponent<EndScreenCard>().SetCard("k", "", true);
                        starScore += 1;
                    }
                    break;
            }

        }
        UpdateStars();
        if (starScore == 3)
        {
            UpdateWin();
        }
        else
        {
            UpdateLose();
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
    void UpdateWin()
    {
        restartButton.SetActive(false); // Hide the restart button on win
        if (GameManager.Instance.levelUnlocked > GameManager.Instance.currentLevel)
        {
            return;
        }
        else
        {
            GameManager.Instance.levelUnlocked = GameManager.Instance.currentLevel + 1;
        }
    }
    void UpdateLose()
    {
        restartButton.SetActive(true); // Show the restart button on lose
        // Implement lose logic here if needed
    }
}
