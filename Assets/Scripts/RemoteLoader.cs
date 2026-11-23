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
            return; // duplicate: don't download again
        }
#if UNITY_EDITOR
        if (preloadedInEditor) return; // Content was already fetched synchronously by RemoteLoaderEditorBootstrap
#endif
        // Bundled copies first, so the game still works offline or if a download fails;
        // downloads overwrite them with the latest text when they arrive.
        fileContent = Fallback("ASKFirstDialogueFinal");
        mistakesContent = Fallback("ASKFirstMistakes");
        formSGLinksContent = Fallback("formsglinks");
        StartCoroutine(Download(fileUrl, text => fileContent = text));
        StartCoroutine(Download(mistakesUrl, text => mistakesContent = text));
        StartCoroutine(Download(formSGLinksUrl, text => formSGLinksContent = text));
    }

    // Snapshot in Assets/Resources/RemoteFallback, refreshed automatically whenever a level is played in the editor.
    const string FallbackFolder = "RemoteFallback/";
    static string Fallback(string name)
    {
        TextAsset asset = Resources.Load<TextAsset>(FallbackFolder + name);
        if (asset == null) Debug.LogError("[RemoteLoader] Missing bundled fallback " + name);
        return asset != null ? asset.text : "";
    }

#if UNITY_EDITOR
    // Editor-only: lets a level be played directly without going through the LoadingScreen scene.
    // Compiled out of every player build (including mobile WebGL).
    bool preloadedInEditor;

    public void PreloadBlockingForEditor()
    {
        Instance = this;
        preloadedInEditor = true;
        fileContent = FetchBlocking(fileUrl, "ASKFirstDialogueFinal");
        mistakesContent = FetchBlocking(mistakesUrl, "ASKFirstMistakes");
        formSGLinksContent = FetchBlocking(formSGLinksUrl, "formsglinks");
    }

    static string FetchBlocking(string url, string name)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.timeout = 10;
            www.SendWebRequest();
            double start = UnityEditor.EditorApplication.timeSinceStartup;
            while (!www.isDone && UnityEditor.EditorApplication.timeSinceStartup - start < 12) System.Threading.Thread.Sleep(10);
            if (www.isDone && www.result == UnityWebRequest.Result.Success)
            {
                // Keep the bundled fallback in sync with the live text.
                string path = System.IO.Path.Combine(Application.dataPath, "Resources", FallbackFolder + name + ".txt");
                if (!System.IO.File.Exists(path) || System.IO.File.ReadAllText(path) != www.downloadHandler.text)
                    System.IO.File.WriteAllText(path, www.downloadHandler.text);
                return www.downloadHandler.text;
            }
            Debug.LogWarning($"[RemoteLoader] Editor preload of {name} failed ({www.error}); using bundled fallback.");
        }
        return Fallback(name);
    }
#endif

    IEnumerator Download(string url, System.Action<string> onSuccess)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError("[RemoteLoader] Failed to download " + url + ": " + www.error);
            else
                onSuccess(www.downloadHandler.text);
        }
    }
}
