using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MobileControls : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Move joystick magnitude is in [-1;1] range, this multiply it before sending it to move event")]
    public float MoveMagnitudeMultiplier = 1.0f;
    [Tooltip("Look joystick magnitude is in [-1;1] range, this multiply it before sending it to move event")]
    public float LookMagnitudeMultiplier = 1.0f;
    public bool InvertLookY;

    [Header("Events")]
    public UnityEvent<Vector2> MoveEvent;
    public UnityEvent<Vector2> LookEvent;
    public UnityEvent<bool> InteractEvent;
    // Removed controls from original layout
    // Left here to serve as reference for future inputs
    /*
    public UnityEvent<bool> JumpEvent;
    public UnityEvent<bool> SprintEvent;
    */

    private UIDocument m_Document;

    private VirtualJoystick m_MoveJoystick;
    private VirtualJoystick m_LookJoystick;
    public float sensitivityMultiplier => GameManager.Instance.Sensitivity;
    private int cameraFingerId = -1;
    private VisualElement root;

    private void Awake()
    {
        m_Document = GetComponent<UIDocument>();

        var safeArea = Screen.safeArea;

        root = m_Document.rootVisualElement;

        root.style.position = Position.Absolute;
        root.style.left = safeArea.xMin;
        root.style.right = Screen.width - safeArea.xMax;
        root.style.top = Screen.height - safeArea.yMax;
        root.style.bottom = safeArea.yMin;
    }

    private void Start()
    {
        var joystickMove = m_Document.rootVisualElement.Q<VisualElement>("JoystickMove");
        var joystickLook = m_Document.rootVisualElement.Q<VisualElement>("JoystickLook");

        m_MoveJoystick = new VirtualJoystick(joystickMove);
        m_MoveJoystick.JoystickEvent.AddListener(mov =>
        {
            MoveEvent.Invoke(mov * MoveMagnitudeMultiplier);
        }); ;
        /*
            m_LookJoystick = new VirtualJoystick(joystickLook);
            m_LookJoystick.JoystickEvent.AddListener(mov =>
            {
                if (InvertLookY)
                    mov.y *= -1;

                LookEvent.Invoke(mov * LookMagnitudeMultiplier * sensitivityMultiplier);
            });

            var interactButton = m_Document.rootVisualElement.Q<VisualElement>("ButtonInteract");
            interactButton.RegisterCallback<PointerEnterEvent>(evt => { InteractEvent.Invoke(true); });
            interactButton.RegisterCallback<PointerLeaveEvent>(evt => { InteractEvent.Invoke(false); });

            // Removed controls from original layout
            // Left here to serve as reference for future inputs
            /*
            var jumpButton = m_Document.rootVisualElement.Q<VisualElement>("ButtonJump");
            jumpButton.RegisterCallback<PointerEnterEvent>(evt => { JumpEvent.Invoke(true); });
            jumpButton.RegisterCallback<PointerLeaveEvent>(evt => { JumpEvent.Invoke(false); });

            var sprintButton = m_Document.rootVisualElement.Q<VisualElement>("ButtonSprint");
            sprintButton.RegisterCallback<PointerEnterEvent>(evt => { SprintEvent.Invoke(true); });
            sprintButton.RegisterCallback<PointerLeaveEvent>(evt => { SprintEvent.Invoke(false); });
            */
    }

    private void Update()
    {
        if (Touchscreen.current == null)
            return;

        // Find a new camera finger
        foreach (var touch in Touchscreen.current.touches)
        {
            if (!touch.press.wasPressedThisFrame)
                continue;

            int id = touch.touchId.ReadValue();
            Vector2 position = touch.position.ReadValue();

            if (cameraFingerId == -1 &&
                position.x > Screen.width * 0.5f)
            {
                cameraFingerId = id;
            }
        }

        // Process camera finger
        if (cameraFingerId != -1)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.touchId.ReadValue() != cameraFingerId)
                    continue;

                // Finger released this frame
                if (touch.press.wasReleasedThisFrame)
                {
                    cameraFingerId = -1;

                    // Tell camera to stop looking
                    LookEvent.Invoke(Vector2.zero);

                    break;
                }

                // Finger is still held
                if (touch.press.isPressed)
                {
                    Vector2 delta = touch.delta.ReadValue();
                    if (delta.magnitude < 0.5f)
                        delta = Vector2.zero;

                    delta = Vector2.Lerp(
                        Vector2.zero,
                        delta,
                        0.5f
                    );
                    if (InvertLookY)
                        delta.y *= -1;

                    LookEvent.Invoke(
                        delta * LookMagnitudeMultiplier * sensitivityMultiplier
                    );
                }

                break;
            }
        }
    }
}
