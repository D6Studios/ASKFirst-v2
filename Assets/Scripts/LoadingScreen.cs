using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class LoadingScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] Slider progressBar;
    [SerializeField] Button startButton;
    private AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> assetLoadingHandle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        assetLoadingHandle = Addressables.LoadSceneAsync("Assets/Scenes/MainMenu.unity", activateOnLoad: false);
        startButton.gameObject.SetActive(false);
    }
    void Update()
    {
        if (assetLoadingHandle.IsValid())
        {
            float progress = assetLoadingHandle.PercentComplete;

            progressText.text = $"{progress * 100:F0}%";
            progressBar.value = progress;
            if (progress >= 1f && !startButton.gameObject.activeSelf)
            {
                if (statusText) statusText.text = "READY!";
                startButton.gameObject.SetActive(true);
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
