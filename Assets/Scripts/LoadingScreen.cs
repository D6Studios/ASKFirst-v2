using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class LoadingScreen : MonoBehaviour
{
    TextMeshProUGUI progressText;
    Slider progressBar;
    Button startButton;
    private AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> assetLoadingHandle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        assetLoadingHandle = Addressables.LoadSceneAsync("Assets/Scenes/MainMenu.unity", activateOnLoad: false);
        progressText = GetComponentInChildren<TextMeshProUGUI>();
        progressBar = GetComponentInChildren<Slider>();
        startButton = GetComponentInChildren<Button>();
        startButton.interactable = false;

    }
    void Update()
    {
        if (assetLoadingHandle.IsValid())
        {
            float progress = assetLoadingHandle.PercentComplete;

            progressText.text = $"{progress * 100:F2}%";
            progressBar.value = progress;
            if (progress >= 1f)
            {
                startButton.interactable = true;
            }
        }
    }
    public void StartButtonPressed()
    {
        if (assetLoadingHandle.IsValid() && assetLoadingHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Screen.fullScreen = true;
            assetLoadingHandle.Result.ActivateAsync();
        }

    }

}
