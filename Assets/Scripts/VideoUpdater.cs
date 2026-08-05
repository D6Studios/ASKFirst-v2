using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;
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
        if (video.isPlaying && !isDragging)
        {
            videoSlider.SetValueWithoutNotify((float)video.time);
        }
        if (videoSlider.value >= videoSlider.maxValue)
        {
            video.transform.parent.gameObject.SetActive(false);
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
    bool isDragging;

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        video.time = videoSlider.value;
    }
}
