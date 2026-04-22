
using UnityEngine;

public class CashierZone : MonoBehaviour
{
    private GameObject reservedBy;

    public bool IsAvailable => reservedBy == null;

    public void Reserve(GameObject npc)
    {
        reservedBy = npc;
        Debug.Log($"{name} RESERVED by {npc.name}");
    }

    public void Release()
    {
        Debug.Log($"{name} released.");
        reservedBy = null;
    }

    public bool IsReservedBy(GameObject npc)
    {
        return reservedBy == npc;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC") && other.gameObject == reservedBy)
        {
            Debug.Log($"{name} is now OCCUPIED by {other.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC") && other.gameObject == reservedBy)
        {
            Debug.Log($"{name} is now AVAILABLE after {other.name} left");
            Release();
        }
    }
}
