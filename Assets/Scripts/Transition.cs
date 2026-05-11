using UnityEngine;

public class Transition : MonoBehaviour
{
   public  Canvas mainMenu;
    public Canvas levelSelect;
    void Start()
    {
        mainMenu.enabled = true;
        levelSelect.enabled = false;
    }
    public void SwitchToLevelSelect()
    {
        mainMenu.enabled = false;
        levelSelect.enabled = true;
    }
}
