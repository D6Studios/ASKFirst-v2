using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RemoteLoader : MonoBehaviour
{
    private string fileUrl = "https://b8888fb0-9a9a-4559-9af8-3749a8683cd3.client-api.unity3dusercontent.com/client_api/v1/environments/production/buckets/7ce1c4ee-c517-498e-873a-6b0f88b9eb03/release_by_badge/latest/entry_by_path/content/?path=ASKFirstDialogueFinal.txt";
    public string fileContent;
    public string mistakesUrl = "https://b8888fb0-9a9a-4559-9af8-3749a8683cd3.client-api.unity3dusercontent.com/client_api/v1/environments/production/buckets/7ce1c4ee-c517-498e-873a-6b0f88b9eb03/release_by_badge/latest/entry_by_path/content/?path=ASKFirstMistakes.txt";
    public string mistakesContent;
    public string formSGLinksUrl = "https://b8888fb0-9a9a-4559-9af8-3749a8683cd3.client-api.unity3dusercontent.com/client_api/v1/environments/production/buckets/7ce1c4ee-c517-498e-873a-6b0f88b9eb03/release_by_badge/latest/entry_by_path/content/?path=formsglinks.txt";
    public string formSGLinksContent;
    public static RemoteLoader Instance { get; private set; }
    void Start()
    {
        if (Instance == null || Instance == this)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
#if UNITY_EDITOR
        if (preloadedInEditor) return; // Content was already fetched synchronously by RemoteLoaderEditorBootstrap
#endif
        StartCoroutine(DownloadNPCLines(fileUrl));
        StartCoroutine(DownloadMistakes(mistakesUrl));
        StartCoroutine(DownloadFormSGLinks(formSGLinksUrl));

    }

#if UNITY_EDITOR
    // Editor-only: lets a level be played directly without going through the LoadingScreen scene.
    // Compiled out of every player build (including mobile WebGL).
    bool preloadedInEditor;

    public void PreloadBlockingForEditor()
    {
        Instance = this;
        preloadedInEditor = true;
        fileContent = FetchBlocking(fileUrl, "ASKFirstDialogueFinal.txt");
        mistakesContent = FetchBlocking(mistakesUrl, "ASKFirstMistakes.txt");
        formSGLinksContent = FetchBlocking(formSGLinksUrl, "formsglinks.txt");
    }

    static string FetchBlocking(string url, string cacheName)
    {
        string cacheDir = System.IO.Path.Combine(Application.dataPath, "..", "Library", "RemoteLoaderCache");
        string cachePath = System.IO.Path.Combine(cacheDir, cacheName);
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.timeout = 10;
            www.SendWebRequest();
            double start = UnityEditor.EditorApplication.timeSinceStartup;
            while (!www.isDone && UnityEditor.EditorApplication.timeSinceStartup - start < 12) System.Threading.Thread.Sleep(10);
            if (www.isDone && www.result == UnityWebRequest.Result.Success)
            {
                System.IO.Directory.CreateDirectory(cacheDir);
                System.IO.File.WriteAllText(cachePath, www.downloadHandler.text);
                return www.downloadHandler.text;
            }
            Debug.LogWarning($"[RemoteLoader] Editor preload of {cacheName} failed ({www.error}); trying cache.");
        }
        if (System.IO.File.Exists(cachePath)) return System.IO.File.ReadAllText(cachePath);
        Debug.LogError($"[RemoteLoader] No network and no cached copy of {cacheName}.");
        return "";
    }
#endif

    IEnumerator DownloadNPCLines(string url)
    {
        Debug.Log("Waiting for response...");

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to download: " + www.error);
        }
        else
        {
            Debug.Log("Response received!");

            fileContent = www.downloadHandler.text;
            Debug.Log(fileContent);
        }
    }
    IEnumerator DownloadMistakes(string url)
    {
        Debug.Log("Waiting for response...");

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to download: " + www.error);
        }
        else
        {
            Debug.Log("Response received!");

            mistakesContent = www.downloadHandler.text;
            Debug.Log(mistakesContent);
        }
    }
    IEnumerator DownloadFormSGLinks(string url)
    {
        Debug.Log("Waiting for response...");

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to download: " + www.error);
        }
        else
        {
            Debug.Log("Response received!");

            formSGLinksContent = www.downloadHandler.text;
            Debug.Log(formSGLinksContent);
        }
    }
}