using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor-only. When you press Play on a scene other than LoadingScreen, the RemoteLoader
/// (which normally lives in the LoadingScreen scene) doesn't exist, so dialogue/mistakes/FormSG
/// text is missing. This creates one and fetches the text before the scene loads.
/// Lives in an Editor folder, so it is never part of a player build (mobile WebGL unaffected).
/// </summary>
static class RemoteLoaderEditorBootstrap
{
    const string PrefKey = "ASKFirst.PreloadRemoteInEditor";
    const string MenuPath = "Tools/ASKFirst/Preload Remote Data When Playing Levels";

    static bool Enabled
    {
        get => EditorPrefs.GetBool(PrefKey, true);
        set => EditorPrefs.SetBool(PrefKey, value);
    }

    [MenuItem(MenuPath)]
    static void Toggle() => Enabled = !Enabled;

    [MenuItem(MenuPath, true)]
    static bool ToggleValidate()
    {
        Menu.SetChecked(MenuPath, Enabled);
        return true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Preload()
    {
        if (!Enabled || RemoteLoader.Instance != null) return;

        // The LoadingScreen scene has its own RemoteLoader that downloads asynchronously.
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "LoadingScreen") return;

        var go = new GameObject("RemoteLoader (Editor Preload)");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<RemoteLoader>().PreloadBlockingForEditor();
        Debug.Log($"[RemoteLoader] Preloaded remote data for editor play of '{sceneName}'.");
    }
}
