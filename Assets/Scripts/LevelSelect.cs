using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    GameObject[] levels;
    public int currentLevel = 0;
    public Vector3 offset = new Vector3(0, 2, 0);
    [SerializeField] Sprite[] images;
    Image levelImage;
    GameObject startButton;
    void Start()
    {
        levels = new GameObject[transform.childCount];
        levelImage = GameObject.Find("LevelImage").GetComponent<Image>();
        startButton = GameObject.Find("StartButton");
        startButton.SetActive(false);
        for (int i = 0; i < transform.childCount; i++)
        {
            levels[i] = transform.GetChild(i).gameObject;
            levels[i].GetComponent<Animator>().SetBool("IsSelected", false);
        }
    }
    public void SelectLevel(int levelIndex)
    {
        startButton.SetActive(true);
        levelImage.sprite = images[levelIndex];
        levels[currentLevel].GetComponent<Animator>().SetBool("IsSelected", false);
        currentLevel = levelIndex;
        levels[currentLevel].GetComponent<Animator>().SetBool("IsSelected", true);
    }
    public void StartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level" + (currentLevel + 1));
    }
}
