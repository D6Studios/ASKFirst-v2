using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SafeDistanceIndicator : MonoBehaviour
{
    [SerializeField] int segments = 48;
    [SerializeField] float yOffset = 0.03f;
    [SerializeField] Color normalColor = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] Color breachedColor = new Color(1f, 0.15f, 0.15f, 0.8f);

    LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.loop = true;
        line.useWorldSpace = false;
        SetColor(normalColor);
    }

    public void SetRadius(float radius)
    {
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
        SetColor(breached ? breachedColor : normalColor);
    }

    void SetColor(Color color)
    {
        line.startColor = color;
        line.endColor = color;
    }
}
