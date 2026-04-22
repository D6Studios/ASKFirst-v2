using UnityEngine;

/// <summary>
/// Represents a queueing spot in the line. Detects player or NPC presence.
/// </summary>
public class LineSpot : MonoBehaviour
{
    public bool IsOccupied { get; set; } = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("NPC"))
        {
            IsOccupied = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("NPC"))
        {
            IsOccupied = false;
        }
    }
}