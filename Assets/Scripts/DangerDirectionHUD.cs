using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// On-screen arcs around the view centre, one per NPC whose safe distance you are approaching, each
// pointing at that NPC: faint white as you near their circle, red once you're inside it.
// Builds its own UI the first time an NPC reports, so no scene setup is needed.
public class DangerDirectionHUD : MonoBehaviour
{
    const float ApproachMargin = 1.5f;   // metres outside the circle where the hint starts to appear
    const float ArcFraction = 0.12f;     // share of a full circle each arc covers
    const float RingScreenHeight = 0.8f; // arc circle diameter as a share of screen height
    static readonly Color Approach = new Color(1f, 1f, 1f, 0.45f);
    static readonly Color Danger = new Color(1f, 0.15f, 0.15f, 0.9f);

    static DangerDirectionHUD instance;

    class Arc
    {
        public Image image;
        public Color current = Color.clear;
        public Vector3 position;
        public float edgeDistance; // from the edge of the NPC's circle; negative = inside
        public bool reported;
    }

    readonly Dictionary<Transform, Arc> arcs = new Dictionary<Transform, Arc>();
    readonly List<Transform> finished = new List<Transform>();
    RectTransform ringRect;
    Sprite ringSprite;
    Transform cam;
    PlayerInteraction playerInteraction;

    public static void Report(Transform npc, float playerDistance, float dangerDistance)
    {
        float edge = playerDistance - dangerDistance;
        if (edge >= ApproachMargin && (instance == null || !instance.arcs.ContainsKey(npc))) return; // nothing to show or fade
        if (instance == null) instance = Create();
        if (!instance.arcs.TryGetValue(npc, out Arc arc))
        {
            arc = new Arc { image = instance.NewArcImage(npc.name) };
            instance.arcs.Add(npc, arc);
        }
        arc.position = npc.position;
        arc.edgeDistance = edge;
        arc.reported = true;
    }

    static DangerDirectionHUD Create()
    {
        var go = new GameObject("DangerDirectionHUD", typeof(Canvas));
        go.layer = LayerMask.NameToLayer("UI");
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // above gameplay HUD, below the level intro (100)
        var hud = go.AddComponent<DangerDirectionHUD>();
        hud.ringSprite = BuildRingSprite(512, 0.93f);
        hud.ringRect = new GameObject("Ring", typeof(RectTransform)).GetComponent<RectTransform>();
        hud.ringRect.gameObject.layer = go.layer;
        hud.ringRect.SetParent(go.transform, false);
        return hud;
    }

    Image NewArcImage(string npcName)
    {
        var rt = new GameObject("Arc " + npcName, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        rt.gameObject.layer = gameObject.layer;
        rt.SetParent(ringRect, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = rt.GetComponent<Image>();
        img.sprite = ringSprite;
        img.raycastTarget = false;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillOrigin = (int)Image.Origin360.Top;
        img.fillClockwise = true;
        img.fillAmount = ArcFraction;
        img.color = Color.clear;
        return img;
    }

    // Anti-aliased white ring; inner is the hole's radius as a fraction of the outer radius.
    static Sprite BuildRingSprite(int size, float inner)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size / 2f, aa = 1.5f;
        var px = new Color32[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                float a = Mathf.Clamp01((r - d) / aa) * Mathf.Clamp01((d - r * inner) / aa);
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void LateUpdate()
    {
        if (cam == null && Camera.main != null) cam = Camera.main.transform;
        if (playerInteraction == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerInteraction = p.GetComponent<PlayerInteraction>();
        }

        float h = Screen.height * RingScreenHeight;
        ringRect.sizeDelta = new Vector2(h, h);

        // Hidden while paused (intro, options) and during conversations, where the player can't move anyway.
        bool canShow = cam != null && Time.timeScale > 0f && (playerInteraction == null || !playerInteraction.isBusy);
        float k = 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime);
        Vector3 forward = cam != null ? cam.forward : Vector3.forward; forward.y = 0f;

        foreach (var pair in arcs)
        {
            Arc arc = pair.Value;
            Color target = Color.clear;
            if (canShow && arc.reported && arc.edgeDistance < ApproachMargin)
            {
                target = arc.edgeDistance <= 0f ? Danger : Approach * new Color(1, 1, 1, 1f - arc.edgeDistance / ApproachMargin);
                // 0 = straight ahead (arc at the top), +90 = to the right.
                Vector3 toNpc = arc.position - cam.position; toNpc.y = 0f;
                float angle = Vector3.SignedAngle(forward, toNpc, Vector3.up);
                // The filled arc grows clockwise from the top, so rotate back by half its sweep to centre it.
                arc.image.rectTransform.localEulerAngles = new Vector3(0, 0, -angle + ArcFraction * 180f);
            }
            arc.current = Color.Lerp(arc.current, target, k);
            arc.image.color = arc.current;
            arc.image.enabled = arc.current.a > 0.01f;
            // Once faded out and no longer reporting (talked to, left, destroyed), drop it.
            if (!arc.reported && arc.current.a <= 0.01f) finished.Add(pair.Key);
            arc.reported = false; // collect fresh reports next frame
        }

        foreach (Transform npc in finished)
        {
            Destroy(arcs[npc].image.gameObject);
            arcs.Remove(npc);
        }
        finished.Clear();
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
