using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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
    void Start()
    {
        UpdateStars(GameManager.Instance.currentScore);
        UpdateCards(GameManager.Instance.mistakesMade);
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
                card.GetComponent<EndScreenCard>().SetCard(mistake.title, mistake.hint, incorrectCardSprite);
            }
            else
            {
                card.GetComponent<EndScreenCard>().SetCard(mistake.title, mistake.hint, correctCardSprite);
            }
        }
    }
    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
