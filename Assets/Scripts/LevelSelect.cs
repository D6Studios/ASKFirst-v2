using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;
public class LevelSelect : MonoBehaviour
{
    GameObject[] levels;
    public int currentLevel = 0;
    public Vector3 offset = new Vector3(0, 2, 0);
    [SerializeField] Sprite[] images;
    string[] levelNames = { "Tutorial", "Nervous Behaviour", "My Own Shopping Bag", "Looking Cool", "No Loitering" };
    string[] levelDescriptions = {
        "Learn the basics of interacting with customers and making decisions.",
        " - Look out for customers looking around frantically and excessively \n- Some may appear to be sweaty due to anxiety or stress \n- These customers may be overly alert of their surroundings",
        "- Look out for customers using their own shopping bags",
        " - Look out for customers wearing shades or overly covered clothing \n- These customers might try avoiding areas that are well surveilled by CCTVs",
        "- Look out for customers standing around in the same area for too long and aren't shopping \n- These customers may walk occasionally to change positions"
    };
    [SerializeField] Sprite lockedImage;
    [SerializeField] Sprite unlockedImage;
    Image levelImage;
    GameObject startButton;
    [SerializeField] TextMeshProUGUI levelNameText;
    [SerializeField] TextMeshProUGUI levelDescriptionText;
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
        levelNameText.text = levelNames[levelIndex];
        levelDescriptionText.text = levelDescriptions[levelIndex];
    }
    public void StartLevel()
    {
        GameManager.Instance.currentLevel = currentLevel;
        if (currentLevel == 0)
        {
            StartCoroutine(GameManager.Instance.LoadScene("Assets/Scenes/Tutorial.unity"));
        }
        StartCoroutine(GameManager.Instance.LoadScene("Assets/Scenes/Level" + (currentLevel) + ".unity"));
    }
    public void UpdateLevelUnlock(int levelIndex)
    {
        for (int i = 0; i < levels.Length; i++)
        {
            if (i <= levelIndex)
            {
                levels[i].GetComponent<Image>().sprite = unlockedImage;
                levels[i].GetComponent<Button>().interactable = true;
            }
            else
            {
                levels[i].GetComponent<Image>().sprite = lockedImage;
                levels[i].GetComponent<Button>().interactable = false;
            }
        }
    }
}
