using UnityEngine;

/// <summary>
/// A component that defines a point for the camera to focus on during interactions
/// </summary>
public class LookPoint : MonoBehaviour
{
    [Header("Look Point Settings")]
    [Tooltip("Visual representation of the look point in the scene view")]
    public bool showGizmo = true;

    [Tooltip("Color of the gizmo in scene view")]
    public Color gizmoColor = Color.yellow;

    [Tooltip("Size of the gizmo sphere")]
    public float gizmoSize = 0.1f;

    [Header("Animation")]
    [Tooltip("Should this look point have a subtle floating animation?")]
    public bool enableFloating = false;

    [Tooltip("How much the look point should float up and down")]
    public float floatAmount = 0.05f;

    [Tooltip("Speed of the floating animation")]
    public float floatSpeed = 2f;

    private Vector3 originalPosition;
    private float floatTimer;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (enableFloating)
        {
            floatTimer += Time.deltaTime * floatSpeed;
            float newY = originalPosition.y + Mathf.Sin(floatTimer) * floatAmount;
            transform.localPosition = new Vector3(originalPosition.x, newY, originalPosition.z);
        }
    }

    /// <summary>
    /// Get the world position of this look point
    /// </summary>
    public Vector3 GetLookPosition()
    {
        return transform.position;
    }

    /// <summary>
    /// Set this look point to a specific local position relative to its parent
    /// </summary>
    public void SetLocalPosition(Vector3 localPos)
    {
        transform.localPosition = localPos;
        originalPosition = localPos;
    }

    /// <summary>
    /// Quick setup for common look point positions
    /// </summary>
    public void SetToHeadHeight()
    {
        SetLocalPosition(new Vector3(0, 1.6f, 0));
    }

    public void SetToChestHeight()
    {
        SetLocalPosition(new Vector3(0, 1.2f, 0));
    }

    public void SetToFaceHeight()
    {
        SetLocalPosition(new Vector3(0, 1.7f, 0.1f)); // Slightly forward for face focus
    }

    // Draw gizmo in scene view
    void OnDrawGizmos()
    {
        if (showGizmo)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, gizmoSize);

            // Draw a small arrow pointing forward
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * gizmoSize * 2);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (showGizmo)
        {
            // Highlight when selected
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(transform.position, gizmoSize);

            // Show connection to parent
            if (transform.parent != null)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(transform.position, transform.parent.position);
            }
        }
    }
}