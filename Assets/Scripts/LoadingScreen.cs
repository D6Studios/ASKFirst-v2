using UnityEngine;
using UnityEngine.AddressableAssets;
public class LoadingScreen : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Addressables.LoadSceneAsync("Assets/Scenes/MainMenu.unity");
    }

}
