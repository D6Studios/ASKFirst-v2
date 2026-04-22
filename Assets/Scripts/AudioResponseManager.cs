using UnityEngine;
using System.Collections.Generic;


public class AudioResponseManager : MonoBehaviour
{
    [Header("Good Audio Clips")]
    [SerializeField] private List<AudioClip> goodAudios = new List<AudioClip>();

    [Header("Bad Audio Clips")]
    [SerializeField] private List<AudioClip> badAudios = new List<AudioClip>();

    private AudioSource audioSource;

    void Awake()
    {
        // Get the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("AudioResponseManager: No AudioSource component found on this GameObject!");
        }
    }

    // Private method to get random AudioClip from list
    private AudioClip GetRandomFromList(List<AudioClip> audioList)
    {
        if (audioList == null || audioList.Count == 0)
        {
            Debug.LogWarning("AudioResponseManager: Audio list is empty or null");
            return null;
        }

        // Filter out null clips
        var validClips = new List<AudioClip>();
        foreach (var clip in audioList)
        {
            if (clip != null)
                validClips.Add(clip);
        }

        if (validClips.Count == 0)
        {
            Debug.LogWarning("AudioResponseManager: No valid audio clips found in list");
            return null;
        }

        int randomIndex = Random.Range(0, validClips.Count);
        return validClips[randomIndex];
    }

    // Private method to play an AudioClip
    private void PlayAudioClip(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("AudioResponseManager: Cannot play null audio clip");
            return;
        }

        if (audioSource == null)
        {
            Debug.LogError("AudioResponseManager: AudioSource is null");
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();

        Debug.Log($"AudioResponseManager: Playing {clip.name}");
    }

    // Public function to play a random good audio clip
    public void GoodResponse()
    {
        AudioClip randomGoodClip = GetRandomFromList(goodAudios);
        if (randomGoodClip != null)
        {
            Debug.Log("AudioResponseManager: Playing good response");
            PlayAudioClip(randomGoodClip);
        }
    }

    // Public function to play a random bad audio clip
    public void BadResponse()
    {
        AudioClip randomBadClip = GetRandomFromList(badAudios);
        if (randomBadClip != null)
        {
            Debug.Log("AudioResponseManager: Playing bad response");
            PlayAudioClip(randomBadClip);
        }
    }

    // Helper methods to manage audio lists at runtime
    public void AddGoodAudio(AudioClip clip)
    {
        if (clip != null)
            goodAudios.Add(clip);
    }

    public void AddBadAudio(AudioClip clip)
    {
        if (clip != null)
            badAudios.Add(clip);
    }

    public void RemoveGoodAudio(AudioClip clip)
    {
        goodAudios.Remove(clip);
    }

    public void RemoveBadAudio(AudioClip clip)
    {
        badAudios.Remove(clip);
    }

    // Get current audio lists (returns copies for safety)
    public List<AudioClip> GetGoodAudios()
    {
        return new List<AudioClip>(goodAudios);
    }

    public List<AudioClip> GetBadAudios()
    {
        return new List<AudioClip>(badAudios);
    }

    // Optional: Play clips using PlayOneShot (doesn't interrupt current audio)
    public void GoodResponseOneShot()
    {
        AudioClip randomGoodClip = GetRandomFromList(goodAudios);
        if (randomGoodClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(randomGoodClip);
            Debug.Log($"AudioResponseManager: Playing good response one-shot: {randomGoodClip.name}");
        }
    }

    public void BadResponseOneShot()
    {
        AudioClip randomBadClip = GetRandomFromList(badAudios);
        if (randomBadClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(randomBadClip);
            Debug.Log($"AudioResponseManager: Playing bad response one-shot: {randomBadClip.name}");
        }
    }
}