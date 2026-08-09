using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class EndScreenCard : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI hintText;
    [SerializeField]
    public Sprite[] cardImages;
    public void SetCard(string catagory, string hint, bool isCorrect)
    {
        TextMeshProUGUI categoryText = gameObject.transform.Find("Catagory").GetComponent<TextMeshProUGUI>();
        categoryText.text = catagory;
        TextMeshProUGUI hintText = gameObject.transform.Find("HintText").GetComponent<TextMeshProUGUI>();
        hintText.text = hint; // Set the hint text as needed
        Image image = gameObject.transform.Find("Logo").GetComponent<Image>();
        if (isCorrect)
        {
            image.sprite = cardImages[0];
        }
        else
        {
            image.sprite = cardImages[1];
        }
    }
}
