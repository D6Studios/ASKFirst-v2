using UnityEngine;

public class Transition : MonoBehaviour
{
   public  Canvas mainMenu;
    public Canvas levelSelect;
    public Canvas videoMenu;
    void Start()
    {
        videoMenu.enabled = true;
        mainMenu.enabled = false;
        levelSelect.enabled = false;
    }
    public void SwitchToLevelSelect()
    {
        mainMenu.enabled = false;
        levelSelect.enabled = true;
    }
    public void skipVideo()
    {
        videoMenu.enabled = false;
        mainMenu.enabled = true;
    }
}
