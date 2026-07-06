using UnityEngine;
using Unity.Cinemachine;
public class NPCFocusCamera : MonoBehaviour
{
    CinemachineCamera virtualCamera;
    void Start()
    {
        virtualCamera = GetComponent<CinemachineCamera>();
    }
    public void FocusOnNPC(Transform npcTransform)
    {
        virtualCamera.LookAt = npcTransform;
        virtualCamera.Priority = 2;
    }
    public void ResetFocus()
    {
        virtualCamera.Priority = 0;
    }
}
