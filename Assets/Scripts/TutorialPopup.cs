using UnityEngine;

public class TutorialPopup : MonoBehaviour
{
    public void PopupClick()
    {
        GameObject.Find("TutorialManager").GetComponent<TutorialManager>().ClosePopup();
    }
}
