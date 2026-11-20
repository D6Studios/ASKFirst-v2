using UnityEngine;

// Ring drawn around an NPC's feet showing the safe distance. It fades in as the player
// approaches (fully invisible beyond DangerDistance + fadeStartMargin, fully visible
// within DangerDistance + fullVisibleMargin) instead of popping in.
[RequireComponent(typeof(LineRenderer))]
public class SafeDistanceIndicator : MonoBehaviour
{
    [SerializeField] int segments = 48;
    [SerializeField] float yOffset = 0.03f;
    [SerializeField] Color normalColor = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] Color breachedColor = new Color(1f, 0.15f, 0.15f, 0.8f);

    [Header("Distance fade")]
    [Tooltip("Distance beyond the circle's edge where the ring starts fading in.")]
    [SerializeField] float fadeStartMargin = 3f;
    [Tooltip("Distance beyond the circle's edge where the ring is fully visible.")]
    [SerializeField] float fullVisibleMargin = 1f;
    [Tooltip("How fast visibility catches up to its target (per second). Smooths dismissal and colour changes.")]
    [SerializeField] float smoothing = 6f;

    LineRenderer line;
    Transform player;
    float radius;
    float visibility;      // current 0..1
    bool dismissed;
    Color currentColor;
    Color targetColor;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.loop = true;
        line.useWorldSpace = false;
        currentColor = targetColor = normalColor;
        ApplyColor();
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        float target = 0f;
        if (!dismissed && player != null)
        {
            Vector3 flat = player.position - transform.position;
            flat.y = 0f;
            float distFromEdge = flat.magnitude - radius;
            target = 1f - Mathf.InverseLerp(fullVisibleMargin, fadeStartMargin, distFromEdge);
            target = target * target * (3f - 2f * target); // smoothstep for a softer ramp
        }

        float k = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
        visibility = Mathf.Lerp(visibility, target, k);
        currentColor = Color.Lerp(currentColor, targetColor, k);

        if (dismissed && visibility < 0.01f)
        {
            gameObject.SetActive(false);
            return;
        }
        ApplyColor();
    }

    public void SetRadius(float newRadius)
    {
        radius = newRadius;
        if (line == null) line = GetComponent<LineRenderer>();
        line.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = 2 * Mathf.PI * i / segments;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, yOffset, Mathf.Sin(angle) * radius));
        }
    }

    public void SetBreached(bool breached)
    {
        targetColor = breached ? breachedColor : normalColor;
    }

    // Fades the ring out and deactivates it (used once the encounter is over).
    public void Dismiss()
    {
        dismissed = true;
        if (!isActiveAndEnabled) gameObject.SetActive(false);
    }

    void ApplyColor()
    {
        Color c = currentColor;
        c.a *= visibility;
        line.startColor = c;
        line.endColor = c;
        line.enabled = c.a > 0.002f;
    }
}
