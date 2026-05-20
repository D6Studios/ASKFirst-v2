using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer AudioMixer;

    [SerializeField] private Slider MusicSlider;
    [SerializeField] private Slider SfxSlider;
    [SerializeField] private Slider SensSlider;
    
    void Start()
    {
        GameObject.FindWithTag("OptionsMenu").SetActive(false);
    }

    // Update is called once per frame
    public void ExitToMainMenu()
    {
        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void SetMusicVolume (Slider MusicSlider)
    {
        // Set the music volume based on the slider value
        float MusicVolume = MusicSlider.value;
        AudioMixer.SetFloat("Music", Mathf.Log10(MusicVolume)*20); // Using this formula so that the value on the slider matches the audio mixer's values
    }
    
    public void SetSFXVolume (Slider SfxSlider)
    {
        // Set the SFX volume based on the slider value
        float SfxVolume = SfxSlider.value;
        AudioMixer.SetFloat("Sfx", Mathf.Log10(SfxVolume)*20); // Using this formula so that the value on the slider matches the audio mixer's values
    }

    public void SetLookSensitivity (Slider SensSlider)
    {
        // Set the look sensitivity based on the slider value

    }
}
