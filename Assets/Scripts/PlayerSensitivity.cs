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
        sensitivity = GameSettings.Sensitivity * 2; // saved look sensitivity
        firstPersonController.RotationSpeed = sensitivity; // Apply the sensitivity value to the FirstPersonController
    }
}
