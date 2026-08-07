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

    private void Awake()
    {
        m_Document = GetComponent<UIDocument>();

        var safeArea = Screen.safeArea;

        var root = m_Document.rootVisualElement;

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
                
                var jumpButton = m_Document.rootVisualElement.Q<VisualElement>("ButtonJump");
                jumpButton.RegisterCallback<PointerEnterEvent>(evt => { JumpEvent.Invoke(true); });
                jumpButton.RegisterCallback<PointerLeaveEvent>(evt => { JumpEvent.Invoke(false); });

                var sprintButton = m_Document.rootVisualElement.Q<VisualElement>("ButtonSprint");
                sprintButton.RegisterCallback<PointerEnterEvent>(evt => { SprintEvent.Invoke(true); });
                sprintButton.RegisterCallback<PointerLeaveEvent>(evt => { SprintEvent.Invoke(false); });
                */
    }
}
