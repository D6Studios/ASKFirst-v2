using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LevelObjectives : MonoBehaviour
{
    List<NPCBehavior> npcObjectives;
    GameObject gameUI;
    TextMeshProUGUI objectiveText;
    Slider objectiveSlider;
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindUI();
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Level1" || scene.name == "Level2" || scene.name == "Level3" || scene.name == "Level4" || scene.name == "Level5" || scene.name == "Tutorial")
        {
            FindUI();

        }
    }
    public void FindUI()
    {
        gameUI = GameObject.FindGameObjectWithTag("GameUI");
        objectiveText = GameObject.FindGameObjectWithTag("Objective").GetComponent<TextMeshProUGUI>();
        objectiveSlider = GameObject.FindGameObjectWithTag("ObjectiveSlider").GetComponent<Slider>();
    }
    public void ResetObjectives()
    {

        npcObjectives = new List<NPCBehavior>();
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        foreach (GameObject npc in npcs)
        {
            if (npc.GetComponent<NPCBehavior>() != null)
            {
                if (npc.GetComponent<NPCBehavior>().isShoplifter)
                {
                    npcObjectives.Add(npc.GetComponent<NPCBehavior>());
                }
            }
        }
        StartCoroutine(UpdateObjectiveText(npcObjectives.Count));
        objectiveSlider.maxValue = npcObjectives.Count;
        objectiveSlider.value = 0;
    }
    public void UpdateObjectives(NPCBehavior npc, float mood)
    {
        if (npcObjectives.Contains(npc))
        {
            npcObjectives.Remove(npc);

            gameUI.GetComponent<Animator>().SetTrigger("Objective");
            StartCoroutine(UpdateObjectiveText(npcObjectives.Count));
            StartCoroutine(UpdateObjectiveSlider());
            if (npcObjectives.Count == 0)
            {
                StartCoroutine(EndLevelAfterDelay(mood, npc.name, 2f));
            }

        }
    }
    IEnumerator UpdateObjectiveText(int newObjectiveCount)
    {

        yield return new WaitForSeconds(0.5f);
        objectiveText.text = "Talk to " + newObjectiveCount + " customers";

    }
    IEnumerator UpdateObjectiveSlider()
    {
        yield return new WaitForSeconds(0.3f);
        float sliderAim = objectiveSlider.maxValue - npcObjectives.Count;
        for (float t = 0; t <= 1; t += 0.1f)
        {
            objectiveSlider.value = Mathf.Lerp(objectiveSlider.value, sliderAim, t);
            yield return new WaitForSeconds(0.05f);
        }

    }
    IEnumerator EndLevelAfterDelay(float moodScore, string npcName, float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.EndLevel(moodScore, npcName);
    }

}

