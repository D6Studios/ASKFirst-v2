using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    GameObject[] levels;
    public int currentLevel = 0;
    public Vector3 offset = new Vector3(0, 2, 0);
    void Start()
    {
        levels = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            levels[i] = transform.GetChild(i).gameObject;
            levels[i].GetComponent<Animator>().SetBool("IsSelected", false);
        }
    }
    public void SelectLevel(int levelIndex)
    {
        currentLevel = levelIndex;
        levels[levelIndex].GetComponent<Animator>().SetBool("IsSelected", true);
        levels[levelIndex].transform.GetChild(0).GetComponent<Animator>().SetBool("IsSelected", true);
        levels[levelIndex].transform.GetChild(0).GetComponent<Image>().enabled = true;
        for (int i = 0; i < levels.Length; i++)
        {
            if (i != levelIndex)
            {
                levels[i].GetComponent<Animator>().SetBool("IsSelected", false);
                levels[i].transform.GetChild(0).GetComponent<Animator>().SetBool("IsSelected", false);
            }
        }
    }
}
