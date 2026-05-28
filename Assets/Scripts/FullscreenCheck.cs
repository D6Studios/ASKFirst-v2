using UnityEngine;
using UnityEngine.EventSystems;
public class FullscreenCheck : MonoBehaviour
{
    void Update()
    {
        if (Screen.fullScreen)
        {
            gameObject.SetActive(false);
            
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Object clicked!");
        Screen.fullScreen = true;
    }
}

