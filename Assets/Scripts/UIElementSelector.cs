using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIElementCycler : MonoBehaviour
{
    [Header("UI Elements")]
    public List<GameObject> uiElements = new List<GameObject>();

    [Header("Control Buttons")]
    public Button nextButton;
    public Button previousButton;

    private int currentIndex = 0;

    void Start()
    {
        // Initialize - disable all elements except the first one
        InitializeElements();

        // Set up button listeners
        if (nextButton != null)
            nextButton.onClick.AddListener(GoToNext);

        if (previousButton != null)
            previousButton.onClick.AddListener(GoToPrevious);

        // Update button states
        UpdateButtonStates();
    }

    void InitializeElements()
    {
        for (int i = 0; i < uiElements.Count; i++)
        {
            if (uiElements[i] != null)
            {
                uiElements[i].SetActive(i == 0); // Only first element is active
            }
        }
    }

    public void GoToNext()
    {
        if (uiElements.Count == 0) return;

        // Disable current element
        if (uiElements[currentIndex] != null)
            uiElements[currentIndex].SetActive(false);

        // Move to next index (no wrap-around)
        currentIndex = Mathf.Min(currentIndex + 1, uiElements.Count - 1);

        // Enable new current element
        if (uiElements[currentIndex] != null)
            uiElements[currentIndex].SetActive(true);

        UpdateButtonStates();
    }

    public void GoToPrevious()
    {
        if (uiElements.Count == 0) return;

        // Disable current element
        if (uiElements[currentIndex] != null)
            uiElements[currentIndex].SetActive(false);

        // Move to previous index (no wrap-around)
        currentIndex = Mathf.Max(currentIndex - 1, 0);

        // Enable new current element
        if (uiElements[currentIndex] != null)
            uiElements[currentIndex].SetActive(true);

        UpdateButtonStates();
    }

    void UpdateButtonStates()
    {
        if (uiElements.Count <= 1)
        {
            if (nextButton != null) nextButton.interactable = false;
            if (previousButton != null) previousButton.interactable = false;
        }
        else
        {
            if (nextButton != null) nextButton.interactable = (currentIndex < uiElements.Count - 1);
            if (previousButton != null) previousButton.interactable = (currentIndex > 0);
        }
    }

    public GameObject GetCurrentElement()
    {
        if (currentIndex >= 0 && currentIndex < uiElements.Count)
            return uiElements[currentIndex];
        return null;
    }

    public void GoToElement(int index)
    {
        if (index < 0 || index >= uiElements.Count) return;

        if (uiElements[currentIndex] != null)
            uiElements[currentIndex].SetActive(false);

        currentIndex = index;

        if (uiElements[currentIndex] != null)
            uiElements[currentIndex].SetActive(true);

        UpdateButtonStates();
    }
}