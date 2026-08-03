using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
public class VideoUpdater : MonoBehaviour
{
    [SerializeField]
    Slider videoSlider;
    VideoPlayer video;
    bool isPrepared;

    void Start()
    {
        video = GetComponent<VideoPlayer>();
        video.prepareCompleted += OnPrepareCompleted;
        isPrepared = false;

    }
    void Update()
    {
        if (!isPrepared) return;
        if (video.isPlaying)
        {
            videoSlider.value = (float)video.time;
        }
    }
    public void UpdateVideoTime()
    {
        video.time = videoSlider.value;
    }
    void OnPrepareCompleted(VideoPlayer vp)
    {
        videoSlider.maxValue = (float)vp.length;
        isPrepared = true;
    }
}
