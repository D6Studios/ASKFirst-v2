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
        SoundManager.Instance.PlaySound(Resources.Load<AudioClip>("SFX/levelselect"), 1f + 0.1f * levelIndex); // each level a step higher
        startButton.SetActive(true);
        levelImage.sprite = images[levelIndex];
        levels[currentLevel].GetComponent<Animator>().SetBool("IsSelected", false);
        currentLevel = levelIndex;
        levels[currentLevel].GetComponent<Animator>().SetBool("IsSelected", true);
        levelNameText.text = GameLevels.All[levelIndex].Title;
        levelDescriptionText.text = GameLevels.All[levelIndex].Description;
    }
    public void StartLevel()
    {
        GameManager.Instance.currentLevel = currentLevel;
        StartCoroutine(GameManager.Instance.LoadScene(GameLevels.ScenePath(currentLevel)));
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
