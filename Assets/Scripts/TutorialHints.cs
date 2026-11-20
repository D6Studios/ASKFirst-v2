using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

// Runtime-built tutorial visuals: an animated "finger" that demonstrates a touch gesture, and a
// spotlight (dimmed screen with a hole + pulsing ring) around a UI element. Everything is created in
// code under the TutorialCanvas so no scene wiring is needed. All animation uses unscaled time so it
// keeps playing while a popup has the game frozen. Nothing here blocks touches.
public class TutorialHints : MonoBehaviour
{
    RectTransform root;          // full-screen container, sits just above the tutorial Background
    Image finger;
    Image fingerAnchor;          // faint ring marking where to press
    RectTransform[] dimRects = new RectTransform[4];
    Image ring;
    Coroutine fingerRoutine;
    Coroutine spotlightRoutine;
    Canvas canvas;

    static Sprite circleSprite;
    static Sprite ringSprite;

    public static TutorialHints Create(GameObject tutorialCanvas)
    {
        GameObject go = new GameObject("TutorialHints", typeof(RectTransform));
        go.transform.SetParent(tutorialCanvas.transform, false);
        TutorialHints hints = go.AddComponent<TutorialHints>();
        hints.Build(tutorialCanvas);
        return hints;
    }

    void Build(GameObject tutorialCanvas)
    {
        canvas = tutorialCanvas.GetComponent<Canvas>();
        root = (RectTransform)transform;
        Stretch(root, Vector2.zero, Vector2.one);
        // Directly above the Background image so popups (instantiated later) draw on top of the hints.
        Transform bg = tutorialCanvas.transform.Find("Background");
        root.SetSiblingIndex(bg != null ? bg.GetSiblingIndex() + 1 : 0);

        if (circleSprite == null) circleSprite = MakeCircleSprite(128, 0f);
        if (ringSprite == null) ringSprite = MakeCircleSprite(128, 0.78f);

        for (int i = 0; i < 4; i++)
        {
            Image dim = NewImage("SpotlightDim" + i, null, new Color(0f, 4f / 255f, 71f / 255f, 0.6f));
            dimRects[i] = dim.rectTransform;
            dim.gameObject.SetActive(false);
        }
        ring = NewImage("SpotlightRing", ringSprite, new Color(1f, 0.85f, 0.2f, 1f));
        ring.gameObject.SetActive(false);

        fingerAnchor = NewImage("FingerAnchor", ringSprite, new Color(1f, 1f, 1f, 0.6f));
        fingerAnchor.rectTransform.sizeDelta = new Vector2(70, 70);
        fingerAnchor.gameObject.SetActive(false);
        finger = NewImage("Finger", circleSprite, new Color(1f, 1f, 1f, 0.9f));
        finger.rectTransform.sizeDelta = new Vector2(44, 44);
        // Fully qualified: QuickOutline also defines a global "Outline" (3D mesh outline).
        UnityEngine.UI.Outline outline = finger.gameObject.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.35f);
        outline.effectDistance = new Vector2(2f, -2f);
        finger.gameObject.SetActive(false);
    }

    Image NewImage(string name, Sprite sprite, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(root, false);
        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false; // never block the joystick / buttons underneath
        RectTransform rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
        return img;
    }

    static void Stretch(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    float ScaleFactor => canvas != null && canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;

    // ---------------- Finger gesture ----------------

    // Shows a finger pressing at a point and dragging along a path (screen pixels, relative to the start).
    // The path is played on a loop until HideFinger is called.
    public void ShowFingerDrag(System.Func<Vector2> screenStart, Vector2[] pathPixels)
    {
        HideFinger();
        fingerRoutine = StartCoroutine(FingerLoop(screenStart, pathPixels));
    }

    public void HideFinger()
    {
        if (fingerRoutine != null) StopCoroutine(fingerRoutine);
        fingerRoutine = null;
        if (finger != null) finger.gameObject.SetActive(false);
        if (fingerAnchor != null) fingerAnchor.gameObject.SetActive(false);
    }

    IEnumerator FingerLoop(System.Func<Vector2> screenStart, Vector2[] path)
    {
        finger.gameObject.SetActive(true);
        fingerAnchor.gameObject.SetActive(true);
        while (true)
        {
            Vector2 start = screenStart();
            fingerAnchor.rectTransform.position = start;
            float s = ScaleFactor;

            // Fade in + press down
            yield return Animate(0.35f, t =>
            {
                finger.rectTransform.position = start;
                SetAlpha(finger, Mathf.Lerp(0f, 0.9f, t));
                finger.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.3f, 1f, t);
            });
            // Drag along each segment of the path
            Vector2 from = Vector2.zero;
            foreach (Vector2 to in path)
            {
                Vector2 a = from, b = to;
                yield return Animate(0.45f, t =>
                {
                    float e = t * t * (3f - 2f * t);
                    finger.rectTransform.position = start + Vector2.Lerp(a, b, e) * s;
                });
                from = to;
            }
            // Lift + fade out
            Vector2 end = start + from * s;
            yield return Animate(0.3f, t =>
            {
                finger.rectTransform.position = end;
                SetAlpha(finger, Mathf.Lerp(0.9f, 0f, t));
                finger.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, 1.3f, t);
            });
            yield return Animate(0.4f, t => { });
        }
    }

    // ---------------- Spotlight ----------------

    // Dims the screen except around the target and pulses a ring on it. The dim is optional so the
    // ring can stay as a gentle pointer after the popup closes. Follows the target and hides itself
    // while the target is inactive.
    public void ShowSpotlight(RectTransform target, bool dim)
    {
        HideSpotlight();
        spotlightRoutine = StartCoroutine(SpotlightLoop(target, dim));
    }

    public void HideSpotlight()
    {
        if (spotlightRoutine != null) StopCoroutine(spotlightRoutine);
        spotlightRoutine = null;
        if (ring != null) ring.gameObject.SetActive(false);
        foreach (RectTransform d in dimRects) if (d != null) d.gameObject.SetActive(false);
    }

    IEnumerator SpotlightLoop(RectTransform target, bool dim)
    {
        while (true)
        {
            bool visible = target != null && target.gameObject.activeInHierarchy;
            ring.gameObject.SetActive(visible);
            foreach (RectTransform d in dimRects) d.gameObject.SetActive(visible && dim);
            if (visible)
            {
                Rect r = ScreenRect(target);
                float pad = 10f * ScaleFactor;
                r.xMin -= pad; r.yMin -= pad; r.xMax += pad; r.yMax += pad;

                if (dim) LayoutDim(r);

                float pulse = 1f + 0.12f * Mathf.Sin(Time.unscaledTime * 6f);
                float size = Mathf.Max(r.width, r.height) / ScaleFactor;
                ring.rectTransform.position = r.center;
                ring.rectTransform.sizeDelta = new Vector2(size, size);
                ring.rectTransform.localScale = Vector3.one * pulse;
            }
            yield return null;
        }
    }

    void LayoutDim(Rect hole)
    {
        float w = Screen.width, h = Screen.height;
        float x0 = Mathf.Clamp01(hole.xMin / w), x1 = Mathf.Clamp01(hole.xMax / w);
        float y0 = Mathf.Clamp01(hole.yMin / h), y1 = Mathf.Clamp01(hole.yMax / h);
        Stretch(dimRects[0], new Vector2(0f, y1), new Vector2(1f, 1f));   // above
        Stretch(dimRects[1], new Vector2(0f, 0f), new Vector2(1f, y0));   // below
        Stretch(dimRects[2], new Vector2(0f, y0), new Vector2(x0, y1));   // left
        Stretch(dimRects[3], new Vector2(x1, y0), new Vector2(1f, y1));   // right
    }

    // Screen-pixel rect of a UGUI element on any canvas.
    static Rect ScreenRect(RectTransform target)
    {
        Canvas c = target.GetComponentInParent<Canvas>();
        Camera cam = c != null && c.renderMode != RenderMode.ScreenSpaceOverlay ? c.worldCamera : null;
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);
        return Rect.MinMaxRect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y), Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
    }

    // Screen-pixel centre of a UI Toolkit element (e.g. the mobile joystick), or null if it isn't laid out.
    public static Vector2? ScreenCenterOf(VisualElement element)
    {
        if (element == null || element.panel == null) return null;
        Rect bound = element.worldBound;
        Rect panelRect = element.panel.visualTree.layout;
        if (bound.width <= 0f || panelRect.width <= 0f || panelRect.height <= 0f) return null;
        float x = bound.center.x / panelRect.width * Screen.width;
        float y = Screen.height - bound.center.y / panelRect.height * Screen.height; // UI Toolkit y is top-down
        return new Vector2(x, y);
    }

    // ---------------- helpers ----------------

    static IEnumerator Animate(float duration, System.Action<float> step)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            step(elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        step(1f);
    }

    static void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    // Anti-aliased filled circle (innerRadius 0) or ring (innerRadius as a fraction of the radius).
    static Sprite MakeCircleSprite(int size, float innerRadius)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        float r = size * 0.5f;
        Color32[] px = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r;
                float outer = Mathf.Clamp01((1f - d) * r);                       // soft outer edge
                float inner = innerRadius > 0f ? Mathf.Clamp01((d - innerRadius) * r) : 1f;
                byte a = (byte)(255f * Mathf.Min(outer, inner));
                px[y * size + x] = new Color32(255, 255, 255, a);
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
