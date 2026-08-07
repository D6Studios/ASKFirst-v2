using UnityEngine;
using StarterAssets;

public class PlayerSensitivity : MonoBehaviour
{
    float sensitivity = 1.0f; // Default sensitivity value
    FirstPersonController firstPersonController;
    void Start()
    {
        firstPersonController = GetComponent<FirstPersonController>();

    }
    void Update()
    {
        sensitivity = GameManager.Instance.Sensitivity * 2; // Get the sensitivity value from GameManager
        firstPersonController.RotationSpeed = sensitivity; // Apply the sensitivity value to the FirstPersonController
    }
}
