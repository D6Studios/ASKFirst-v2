using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// Renders the open scene's "Photoshoot Camera" to a transparent, supersampled PNG
/// in Assets/Scenes/PhotoshootScenes/Renders.
public static class PhotoshootCapture
{
    const string CameraName = "Photoshoot Camera";
    const string OutDir = "Assets/Scenes/PhotoshootScenes/Renders";
    // 2048 out (poster circle needs ~1650px at 300dpi). 8192 internal crashed the editor: post-processing buffers ran out of GPU memory.
    const int Size = 2048, Supersample = 2;

    [MenuItem("Tools/Photoshoot/Render Photoshoot Camera %#&r")]
    static void RenderMenu()
    {
        var cam = FindCamera();
        if (!cam) { EditorUtility.DisplayDialog("Photoshoot", $"No camera named \"{CameraName}\" in the open scene.", "OK"); return; }
        Directory.CreateDirectory(OutDir);
        string path = $"{OutDir}/{cam.gameObject.scene.name}_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        Render(cam, path);
        AssetDatabase.Refresh();
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Texture2D>(path));
        Debug.Log("Photoshoot render saved: " + path);
    }

    /// Batch mode: Unity.exe -batchmode -projectPath . -executeMethod PhotoshootCapture.RenderBatch -scene <path> -out <png> -quit
    public static void RenderBatch()
    {
        var args = System.Environment.GetCommandLineArgs();
        string Arg(string n) { int i = System.Array.IndexOf(args, n); return i >= 0 ? args[i + 1] : null; }
        EditorSceneManager.OpenScene(Arg("-scene"));
        Render(FindCamera(), Arg("-out"));
    }

    static Camera FindCamera()
    {
        foreach (var c in Object.FindObjectsByType<Camera>())
            if (c.name == CameraName) return c;
        return null;
    }

    static void Render(Camera cam, string path)
    {
        int big = Size * Supersample;
        var hi = new RenderTexture(big, big, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        var lo = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        var prevTarget = cam.targetTexture; var prevActive = RenderTexture.active;
        try
        {
            cam.targetTexture = hi;
            cam.Render(); // warm-up: textures still loading on the first render after a scene opens come out grey/white
            cam.Render();
            Graphics.Blit(hi, lo); // exact 2x bilinear = 2x2 box filter
            RenderTexture.active = lo;
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);

            // Edges were blended against transparent black, so colour is premultiplied; undo it to avoid dark fringes.
            var px = tex.GetPixels32();
            for (int i = 0; i < px.Length; i++)
            {
                var p = px[i];
                if (p.a == 0 || p.a == 255) continue;
                float k = 255f / p.a;
                px[i] = new Color32((byte)Mathf.Min(255, p.r * k), (byte)Mathf.Min(255, p.g * k), (byte)Mathf.Min(255, p.b * k), p.a);
            }
            tex.SetPixels32(px);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }
        finally
        {
            cam.targetTexture = prevTarget; RenderTexture.active = prevActive;
            hi.Release(); lo.Release();
        }
    }
}
