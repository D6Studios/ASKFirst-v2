using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// Batch-mode helper: renders a scene's overlay canvases to a PNG and dumps the UI hierarchy,
/// so UI can be reviewed without opening the Editor.
/// Usage: Unity.exe -batchmode -projectPath . -executeMethod UICapture.Run -scene <path> -out <png> [-only <canvasName>] [-quit]
public static class UICapture
{
    public static void Run()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        string scene = Arg(args, "-scene"), outPath = Arg(args, "-out"), only = Arg(args, "-only");
        int w = int.Parse(Arg(args, "-w") ?? "1920"), h = int.Parse(Arg(args, "-h") ?? "1080");

        EditorSceneManager.OpenScene(scene);
        Capture(outPath, only, w, h);
    }

    public static void Capture(string outPath, string only, int w, int h)
    {
        var camGo = new GameObject("__CaptureCam");
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.cullingMask = 1 << 5; // UI layer only
        var rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;

        var dump = new StringBuilder();
        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!canvas.isRootCanvas) continue;
            bool show = only == null ? canvas.enabled && canvas.gameObject.activeInHierarchy : canvas.name == only;
            canvas.gameObject.SetActive(show);
            canvas.enabled = show;
            if (!show) continue;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1;
            Dump(canvas.transform, 0, dump);
        }

        Canvas.ForceUpdateCanvases();
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        File.WriteAllBytes(outPath, tex.EncodeToPNG());
        File.WriteAllText(Path.ChangeExtension(outPath, ".txt"), dump.ToString());
    }

    static void Dump(Transform t, int depth, StringBuilder sb)
    {
        var rt = t as RectTransform;
        sb.Append(' ', depth * 2).Append(t.name).Append(t.gameObject.activeSelf ? "" : " (inactive)");
        if (rt) sb.Append($"  anchor {rt.anchorMin}-{rt.anchorMax} pos {rt.anchoredPosition} size {rt.sizeDelta}");
        foreach (var c in t.GetComponents<Component>())
            if (!(c is Transform)) sb.Append(" [").Append(c.GetType().Name).Append(']');
        sb.AppendLine();
        foreach (Transform child in t) Dump(child, depth + 1, sb);
    }

    static string Arg(string[] args, string name)
    {
        int i = System.Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }
}
