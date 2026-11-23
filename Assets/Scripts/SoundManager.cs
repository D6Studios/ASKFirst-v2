using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    AudioSource sfxAudioSource;
    AudioSource pitchedAudioSource; // own source so a pitch change doesn't bend other one-shots
    AudioSource bgmAudioSource;
    public AudioMixerGroup SfxGroup => sfxAudioSource.outputAudioMixerGroup;
    public static SoundManager Instance { get; private set; }
    void Awake() // Awake, not Start: GameManager.Start reads Instance
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        bgmAudioSource = transform.GetChild(0).GetComponent<AudioSource>();
        sfxAudioSource = transform.GetChild(1).GetComponent<AudioSource>();
        pitchedAudioSource = sfxAudioSource.gameObject.AddComponent<AudioSource>();
        pitchedAudioSource.playOnAwake = false;
        pitchedAudioSource.outputAudioMixerGroup = sfxAudioSource.outputAudioMixerGroup;
        //PlayBGM(Resources.Load<AudioClip>("bgm"));
    }
    public void PlaySound(AudioClip clip, float pitch = 1f)
    {
        if (pitch == 1f) { sfxAudioSource.PlayOneShot(clip); return; }
        pitchedAudioSource.pitch = pitch;
        pitchedAudioSource.PlayOneShot(clip);
    }
    public void PlayRandom(AudioClip[] clips, float pitchJitter = 0f)
    {
        if (clips.Length > 0) PlaySound(clips[Random.Range(0, clips.Length)], 1f + Random.Range(-pitchJitter, pitchJitter));
    }
    public void PlayBGM(AudioClip clip)
    {
        bgmAudioSource.clip = clip;
        bgmAudioSource.Play();
    }
    public void StopBGM()
    {
        bgmAudioSource.Stop();
    }
    public void PauseBGM()
    {
        bgmAudioSource.Pause();
    }
    public void ResumeBGM()
    {
        bgmAudioSource.UnPause(); // no-op if BGM was never playing
    }
}
