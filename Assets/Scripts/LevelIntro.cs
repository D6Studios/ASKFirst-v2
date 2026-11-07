using System;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class LevelIntro : MonoBehaviour
{
    [SerializeField] string levelName;
    [TextArea][SerializeField] string levelDescription;

    [SerializeField] Canvas introCanvas;
    [SerializeField] TextMeshProUGUI levelNameText;
    [SerializeField] TextMeshProUGUI levelDescriptionText;
    [SerializeField] Button skipButton;

    [SerializeField] CinemachineCamera introCamera;
    [SerializeField] int introCameraPriority = 20;
    [SerializeField] Transform[] panWaypoints;
    [SerializeField] float panDuration = 6f;
    [SerializeField] float holdAfterPan = 1f;

    bool skipRequested;

    void Awake()
    {
        if (introCanvas != null) introCanvas.enabled = false;
        if (skipButton != null) skipButton.onClick.AddListener(() => skipRequested = true);
    }

    public void PlayIntro(Action onComplete)
    {
        StartCoroutine(RunIntro(onComplete));
    }

    IEnumerator RunIntro(Action onComplete)
    {
        skipRequested = false;
        float previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (levelNameText != null) levelNameText.text = levelName;
        if (levelDescriptionText != null) levelDescriptionText.text = levelDescription;
        if (introCanvas != null) introCanvas.enabled = true;

        CursorLockMode previousLockState = Cursor.lockState;
        bool previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Renderer[] playerRenderers = null;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerRenderers = player.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in playerRenderers) r.enabled = false;
        }

        int previousPriority = 0;
        if (introCamera != null)
        {
            previousPriority = introCamera.Priority;
            introCamera.Priority = introCameraPriority;
        }

        if (introCamera != null && panWaypoints != null && panWaypoints.Length > 1)
        {
            yield return PanCamera();
        }
        else
        {
            yield return WaitUnscaled(panDuration);
        }

        yield return WaitUnscaled(holdAfterPan);

        if (introCanvas != null) introCanvas.enabled = false;
        if (introCamera != null) introCamera.Priority = previousPriority;
        Time.timeScale = previousTimeScale;

        if (playerRenderers != null)
        {
            foreach (Renderer r in playerRenderers)
            {
                if (r != null) r.enabled = true;
            }
        }
        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisible;

        onComplete?.Invoke();
    }

    IEnumerator WaitUnscaled(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && !skipRequested)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    IEnumerator PanCamera()
    {
        Transform camTransform = introCamera.transform;
        float segmentDuration = panDuration / (panWaypoints.Length - 1);
        camTransform.SetPositionAndRotation(panWaypoints[0].position, panWaypoints[0].rotation);

        for (int i = 0; i < panWaypoints.Length - 1 && !skipRequested; i++)
        {
            Vector3 startPos = panWaypoints[i].position;
            Quaternion startRot = panWaypoints[i].rotation;
            Vector3 endPos = panWaypoints[i + 1].position;
            Quaternion endRot = panWaypoints[i + 1].rotation;

            float elapsed = 0f;
            while (elapsed < segmentDuration && !skipRequested)
            {
                float t = elapsed / segmentDuration;
                camTransform.SetPositionAndRotation(Vector3.Lerp(startPos, endPos, t), Quaternion.Slerp(startRot, endRot, t));
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            camTransform.SetPositionAndRotation(endPos, endRot);
        }
    }
}
