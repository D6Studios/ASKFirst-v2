using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// Rebuilds the look of the loading screen and the Form SG "training complete" screen in the
/// main-menu style (navy/purple/orange, Lexend Deca, UiButtonWhite/UIButtonBlue sprites).
/// Restyles the existing objects in place so button OnClick wiring is kept; safe to re-run.
public static class UIRestyle
{
    static readonly Color Navy = new Color32(0x13, 0x00, 0x71, 0xFF);
    static readonly Color DeepNavy = new Color32(0x0B, 0x00, 0x45, 0xFF);
    static readonly Color Orange = new Color32(0xF0, 0xAD, 0x55, 0xFF);

    const string PillPath = "Assets/Sprites/UI/Pill.png";
    const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LexendDeca-VariableFont_wght SDF 1.asset";
    const string PlainFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LexendDeca-VariableFont_wght SDF.asset";

    [MenuItem("Tools/ASK First/Restyle Loading + Form SG Screens")]
    public static void RunAll()
    {
        EnsurePillSprite();
        RestyleLoading();
        RestyleFormSG();
    }

    // ---------- Loading screen ----------

    static void RestyleLoading()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/LoadingScreen.unity");
        var loader = Object.FindFirstObjectByType<LoadingScreen>(FindObjectsInactive.Include);
        var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        SetScaler(canvas);
        var root = canvas.transform;

        var panel = root.Find("Panel").GetComponent<Image>();
        panel.sprite = null;
        panel.color = DeepNavy;

        var shape = Img(root, "LogoBackground", Sprite("Assets/Sprites/MainMenu/LogoBackground.png"), Color.white);
        Place(shape, new Vector2(30, 150), new Vector2(760, 760));
        shape.transform.SetSiblingIndex(1);

        var logo = Img(root, "Logo", Sprite("Assets/Sprites/MainMenu/ASKFirstLogoShadow.png"), Color.white);
        Place(logo, new Vector2(0, 170), new Vector2(490, 484));
        logo.preserveAspect = true;
        logo.transform.SetSiblingIndex(2);

        // Progress bar: rounded track + orange fill
        var slider = root.Find("Slider").GetComponent<Slider>();
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        slider.minValue = 0; slider.maxValue = 1; slider.value = 0.6f;
        Place(slider, new Vector2(0, -160), new Vector2(900, 30));
        var bg = slider.transform.Find("Background").GetComponent<Image>();
        Stretch(bg.rectTransform);
        Pill(bg, new Color(1, 1, 1, 0.15f), 30);
        var fillArea = (RectTransform)slider.transform.Find("Fill Area");
        Stretch(fillArea);
        var fill = slider.fillRect.GetComponent<Image>();
        fill.rectTransform.sizeDelta = Vector2.zero;
        Pill(fill, Orange, 30);

        var status = Txt(root, "Status", "LOADING", 30, Color.white, TextAlignmentOptions.Left);
        status.characterSpacing = 8;
        Plain(status);
        Place(status, new Vector2(-225, -215), new Vector2(450, 50));
        var progress = root.Find("Progress").GetComponent<TextMeshProUGUI>();
        Style(progress, "60%", 30, Orange, TextAlignmentOptions.Right);
        Plain(progress);
        Place(progress, new Vector2(225, -215), new Vector2(450, 50));

        // Start button: same white/navy box as the main menu START
        var start = root.Find("Start").GetComponent<Image>();
        ButtonBox(start, "Assets/Sprites/MainMenu/UiButtonWhite.png", 1.5f);
        Place(start, new Vector2(0, -350), new Vector2(560, 120));
        Style(start.GetComponentInChildren<TextMeshProUGUI>(), "TAP TO START", 52, Navy, TextAlignmentOptions.Center);

        var np = Img(root, "NPLogo", Sprite("Assets/Sprites/MainMenu/NPLogo.png"), Color.white);
        np.preserveAspect = true;
        Corner(np.rectTransform, new Vector2(1, 0), new Vector2(-40, 40), new Vector2(240, 80));

        var so = new SerializedObject(loader);
        so.FindProperty("progressText").objectReferenceValue = progress;
        so.FindProperty("statusText").objectReferenceValue = status;
        so.FindProperty("progressBar").objectReferenceValue = slider;
        so.FindProperty("startButton").objectReferenceValue = start.GetComponent<Button>();
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ---------- Form SG "training complete" screen (in MainMenu) ----------

    static void RestyleFormSG()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var canvasGo = FindInScene("FormSGRedirect");
        var canvas = canvasGo.GetComponent<Canvas>();
        SetScaler(canvas);
        var root = canvas.transform;

        var panel = root.Find("Panel").GetComponent<Image>();
        Stretch(panel.rectTransform);
        panel.sprite = null;
        panel.color = new Color(DeepNavy.r, DeepNavy.g, DeepNavy.b, 0.94f);

        var shape = Img(root, "LogoBackground", Sprite("Assets/Sprites/MainMenu/LogoBackground.png"), Color.white);
        Place(shape, new Vector2(-20, 110), new Vector2(1180, 1180));
        shape.transform.SetSiblingIndex(1);

        var card = Img(root, "Card", null, Color.white);
        ButtonBox(card, "Assets/Sprites/MainMenu/UiButtonWhite.png", 1.2f);
        Place(card, new Vector2(0, -30), new Vector2(1060, 600));
        card.transform.SetSiblingIndex(2);

        var badge = Img(root, "Badge", Sprite("Assets/Sprites/Circle.png"), Navy);
        Place(badge, new Vector2(0, 270), new Vector2(170, 170));
        badge.transform.SetSiblingIndex(3);
        var tick = Img(badge.transform, "Tick", Sprite("Assets/Sprites/TickLogo.png"), Color.white);
        tick.preserveAspect = true;
        Place(tick, new Vector2(0, -4), new Vector2(100, 100));

        var title = Txt(root, "Title", "TRAINING COMPLETE!", 72, Navy, TextAlignmentOptions.Center);
        Place(title, new Vector2(0, 110), new Vector2(980, 100));
        title.transform.SetSiblingIndex(4);

        var body = root.Find("Congrats").GetComponent<TextMeshProUGUI>();
        Style(body, "Congratulations on completing the A.S.K First training!\n" +
                    "Please fill out the short Form SG to finish up.", 34, Navy, TextAlignmentOptions.Center);
        body.fontStyle = FontStyles.Normal;
        Plain(body);
        body.color = new Color32(0x2A, 0x22, 0x5C, 0xFF);
        body.lineSpacing = 12;
        Place(body, new Vector2(0, -5), new Vector2(940, 130));

        var btn = root.Find("FormSGButton").GetComponent<Image>();
        ButtonBox(btn, "Assets/Sprites/MainMenu/UIButtonBlue.png", 1.5f);
        Place(btn, new Vector2(0, -190), new Vector2(560, 120));
        Style(btn.GetComponentInChildren<TextMeshProUGUI>(), "OPEN FORM SG", 46, Color.white, TextAlignmentOptions.Center);
        btn.transform.SetAsLastSibling();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ---------- Helpers ----------

    static void EnsurePillSprite()
    {
        if (File.Exists(PillPath)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(PillPath));
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(r - d))); // 1px anti-aliased edge
            }
        File.WriteAllBytes(PillPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(PillPath);
        var imp = (TextureImporter)AssetImporter.GetAtPath(PillPath);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spriteBorder = new Vector4(31, 31, 31, 31);
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.SaveAndReimport();
    }

    static void Pill(Image img, Color color, float height)
    {
        img.sprite = Sprite(PillPath);
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 62f / height; // rounded ends = half the bar height
        img.color = color;
    }

    static void ButtonBox(Image img, string spritePath, float ppuMultiplier)
    {
        img.sprite = Sprite(spritePath);
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = ppuMultiplier;
        img.color = Color.white;
    }

    static void SetScaler(Canvas canvas)
    {
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    static Image Img(Transform parent, string name, Sprite sprite, Color color)
    {
        var go = Child(parent, name);
        var img = go.GetComponent<Image>();
        if (!img) img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static TextMeshProUGUI Txt(Transform parent, string name, string text, float size, Color color, TextAlignmentOptions align)
    {
        var go = Child(parent, name);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (!t) t = go.AddComponent<TextMeshProUGUI>();
        Style(t, text, size, color, align);
        t.raycastTarget = false;
        return t;
    }

    static void Style(TextMeshProUGUI t, string text, float size, Color color, TextAlignmentOptions align)
    {
        t.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        t.fontSharedMaterial = t.font.material;
        t.text = text;
        t.fontSize = size;
        t.enableAutoSizing = false;
        t.fontStyle = FontStyles.Bold;
        t.color = color;
        t.alignment = align;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.margin = Vector4.zero;
    }

    // Main-menu font has a baked outline; body/small labels read better without it
    static void Plain(TextMeshProUGUI t)
    {
        t.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PlainFontPath);
        t.fontSharedMaterial = t.font.material;
    }

    static GameObject Child(Transform parent, string name)
    {
        var t = parent.Find(name);
        if (t) return t.gameObject;
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Place(Component c, Vector2 pos, Vector2 size)
    {
        var rt = (RectTransform)c.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
    }

    static void Corner(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        rt.localScale = Vector3.one;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.anchoredPosition = rt.sizeDelta = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

    static GameObject FindInScene(string name)
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (go.name == name && go.transform.parent == null) return go;
        throw new System.Exception("Not found: " + name);
    }
}
