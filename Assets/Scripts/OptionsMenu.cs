using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer AudioMixer;

    [SerializeField] private Slider MusicSlider;
    [SerializeField] private Slider SfxSlider;
    [SerializeField] private Slider SensSlider;


    void Start()
    {
        SensSlider.value = GameManager.Instance.Sensitivity;
        MusicSlider.value = SoundManager.Instance.MusicVolume;
        SfxSlider.value = SoundManager.Instance.SFXVolume;
    }

    // Update is called once per frame
    public void ExitToMainMenu()
    {
        StartCoroutine(GameManager.Instance.LoadScene("Assets/Scenes/MainMenu.unity"));
    }

    public void SetMusicVolume()
    {
        SoundManager.Instance.MusicVolume = MusicSlider.value;
        float MusicVolume = SoundManager.Instance.MusicVolume;
        if (MusicVolume == 0)
        {
            AudioMixer.SetFloat("Music", -80); // Set to minimum volume if slider is at 0 to avoid log10(0) error
            return;
        }
        AudioMixer.SetFloat("Music", Mathf.Log10(MusicVolume) * 20); // Using this formula so that the value on the slider matches the audio mixer's values
    }

    public void SetSFXVolume()
    {
        SoundManager.Instance.SFXVolume = SfxSlider.value;
        float SfxVolume = SoundManager.Instance.SFXVolume;
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
        GameManager.Instance.Sensitivity = Sensitivity;
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
