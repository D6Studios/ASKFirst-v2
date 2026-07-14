using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class EndScreenCard : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI hintText;
    public Image cardImages;
    void Awake()
    {
        title = gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        hintText = gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        cardImages = gameObject.transform.GetChild(2).GetComponent<Image>();

    }
    public void SetCard(string titleText, string hint, Sprite cardSprite, bool correct)
    {
        if (correct)
        {
            title.color = Color.green;
        }
        else
        {
            title.color = Color.red;
        }
        title.text = titleText;
        hintText.text = hint;

        cardImages.sprite = cardSprite;

    }
}
