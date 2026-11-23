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
        SensSlider.value = GameSettings.Sensitivity;
        MusicSlider.value = GameSettings.MusicVolume;
        SfxSlider.value = GameSettings.SfxVolume;
    }

    // Update is called once per frame
    public void ExitToMainMenu()
    {
        StartCoroutine(GameManager.Instance.LoadScene(GameLevels.ScenePath(GameLevels.MainMenu)));
    }

    public void SetMusicVolume()
    {
        AudioMixer.SetFloat("Music", GameSettings.ToDecibels(MusicSlider.value));
        GameSettings.MusicVolume = MusicSlider.value;
    }

    public void SetSFXVolume()
    {
        AudioMixer.SetFloat("Sfx", GameSettings.ToDecibels(SfxSlider.value));
        GameSettings.SfxVolume = SfxSlider.value;
    }

    public void SetLookSensitivity()
    {
        // Set the look sensitivity based on the slider value
        float Sensitivity = SensSlider.value;
        GameSettings.Sensitivity = Sensitivity;
    }
    public void pauseGame()
    {
        GameManager.Instance.PauseGame();
    }
    public void resumeGame()
    {
        GameManager.Instance.ResumeGame();
    }

    [Tooltip("\"Are you sure?\" box shown by RESET DATA. Starts hidden.")]
    [SerializeField] private GameObject resetConfirm;

    public void AskResetData()
    {
        SoundManager.Instance.PlaySound(Resources.Load<AudioClip>("click"));
        resetConfirm.SetActive(true);
    }

    public void CancelResetData()
    {
        SoundManager.Instance.PlaySound(Resources.Load<AudioClip>("click"));
        resetConfirm.SetActive(false);
    }

    // Wipes progress and settings, then reloads the main menu so every screen reads the fresh defaults.
    public void ConfirmResetData()
    {
        GameSettings.ResetAll();
        GameSettings.ApplyVolumes(AudioMixer);
        GameManager.Instance.currentLevel = 0;
        resetConfirm.SetActive(false);
        StartCoroutine(GameManager.Instance.LoadScene(GameLevels.ScenePath(GameLevels.MainMenu)));
    }
}
