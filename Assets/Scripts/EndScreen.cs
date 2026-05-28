using UnityEngine;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
     public Image[] stars;

    public Sprite fullStar;
    public Sprite halfStar;
    public Sprite emptyStar;

    void Start()
    {
        UpdateStars(GameManager.Instance.currentScore);
    }

    public void UpdateStars(float value)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            int starValue = (i * 2) + 2;

            if (value >= starValue)
            {
                stars[i].sprite = fullStar;
            }
            else if (value == starValue - 1)
            {
                stars[i].sprite = halfStar;
            }
            else
            {
                stars[i].sprite = emptyStar;
            }
        }
    }
    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
