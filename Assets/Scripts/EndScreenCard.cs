using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class EndScreenCard : MonoBehaviour
{
    public  TextMeshProUGUI title;
    public TextMeshProUGUI hintText;
    public Image[] cardImages;
    void Awake()
    {
        title = gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        hintText = gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        cardImages = new Image[2];
        cardImages[0] = gameObject.transform.GetChild(2).GetComponent<Image>();
        cardImages[1] = gameObject.transform.GetChild(3).GetComponent<Image>();

    }
    public void SetCard(string titleText, string hint, Sprite cardSprite)
    {
        title.text = titleText;
        hintText.text = hint;
        foreach (Image img in cardImages)
        {
            img.sprite = cardSprite;
        }
    }
}
