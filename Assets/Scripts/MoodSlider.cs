using UnityEngine;
using UnityEngine.UI;

public class MoodSlider : MonoBehaviour
{
    Color badColor = Color.red;
    Color goodColor = Color.green;
    Slider moodSlider;
    void Start()
    {
        moodSlider = GetComponent<Slider>();
        moodSlider.fillRect.GetComponent<Image>().color = Color.Lerp(badColor, goodColor, 0.5f);
    }
    public void ChangeColor()
    {
        if (moodSlider == null)
        {
            moodSlider = GetComponent<Slider>();
        }
        Debug.Log("Value:" + moodSlider.value/ moodSlider.maxValue);
        moodSlider.fillRect.GetComponent<Image>().color = Color.Lerp(badColor, goodColor, moodSlider.value / moodSlider.maxValue);

    }
}
