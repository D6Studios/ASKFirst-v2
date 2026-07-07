using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer AudioMixer;

    [SerializeField] private Slider MusicSlider;
    [SerializeField] private Slider SfxSlider;
    [SerializeField] private Slider SensSlider;
    private MobileControls mobileControls;

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        try
        {
            mobileControls = GameObject.FindGameObjectWithTag("MobileControls").GetComponent<MobileControls>();
        }
        catch (Exception e)
        {
            Debug.LogWarning("MobileControls not found in the scene: " + e.Message);
        }

    }

    // Update is called once per frame
    public void ExitToMainMenu()
    {
        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void SetMusicVolume()
    {
        // Set the music volume based on the slider value
        float MusicVolume = MusicSlider.value;
        if (MusicVolume == 0)
        {
            AudioMixer.SetFloat("Music", -80); // Set to minimum volume if slider is at 0 to avoid log10(0) error
            return;
        }
        AudioMixer.SetFloat("Music", Mathf.Log10(MusicVolume) * 20); // Using this formula so that the value on the slider matches the audio mixer's values
    }

    public void SetSFXVolume()
    {
        // Set the SFX volume based on the slider value
        float SfxVolume = SfxSlider.value;
        if (SfxVolume == 0)
        {
            AudioMixer.SetFloat("Sfx", -80); // Set to minimum volume if slider is at 0 to avoid log10(0) error
            return;
        }
        AudioMixer.SetFloat("Sfx", Mathf.Log10(SfxVolume) * 20); // Using this formula so that the value on the slider matches the audio mixer's values
    }

    public void SetLookSensitivity()
    {
        // Set the look sensitivity based on the slider value
        float Sensitivity = SensSlider.value;
        mobileControls.sensitivityMultiplier = Sensitivity; // Set the sensitivity multiplier in the MobileControls script
    }
    public void pauseGame()
    {
        GameManager.Instance.PauseGame();
    }
    public void resumeGame()
    {
        GameManager.Instance.ResumeGame();
    }
}
