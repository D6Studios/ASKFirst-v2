
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BuyZone : MonoBehaviour
{
    private GameObject occupyingShopper;
    private GameObject assignedShopper;

    public bool IsOccupied => occupyingShopper != null;

    public bool IsOccupiedBy(GameObject shopper) => occupyingShopper == shopper;

    public void AssignShopper(GameObject shopper)
    {
        assignedShopper = shopper;
    }

    public bool IsAssignedTo(GameObject shopper) => assignedShopper == shopper;

    public void ClearAssignment()
    {
        assignedShopper = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Shopper") && other.gameObject == assignedShopper)
        {
            occupyingShopper = other.gameObject;
            Debug.Log($"{name}: Shopper '{other.name}' ENTERED zone (self-assigned).");
        }
        else
        {
            Debug.Log($"{name}: ❌ Unexpected shopper '{other.name}' entered.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Shopper") && occupyingShopper == other.gameObject)
        {
            Debug.Log($"{name}: Shopper '{other.name}' EXITED zone.");
            occupyingShopper = null;
            assignedShopper = null;
        }
    }
}
