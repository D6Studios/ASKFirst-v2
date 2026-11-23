using UnityEngine;

public enum NPCAnimation { Idle, Walking, Suspicious, HoldingItem, InteractedWith, Happy, Upset }

// Drives the NPC Animator Controller. Looping states are bools; one-off reactions are triggers.
public class NPCAnimator : MonoBehaviour
{
    private Animator animator;

    [Header("Footsteps")]
    [SerializeField] float footstepVolume = 0.6f;
    [SerializeField] float footstepMaxDistance = 12f;
    static AudioClip[] footstepClips;
    AudioSource footstepSource;
    Transform leftFoot, rightFoot;
    bool leftFootLower;
    float nextStepTime;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (footstepClips == null) footstepClips = Resources.LoadAll<AudioClip>("SFX/Footsteps");
        leftFoot = FindFoot(HumanBodyBones.LeftFoot, "LeftFoot");
        rightFoot = FindFoot(HumanBodyBones.RightFoot, "RightFoot");
        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.playOnAwake = false;
        footstepSource.spatialBlend = 1f;
        footstepSource.rolloffMode = AudioRolloffMode.Linear;
        footstepSource.maxDistance = footstepMaxDistance;
    }

    void Start() // SoundManager may Awake after us when it lives in the same scene
    {
        if (SoundManager.Instance != null) footstepSource.outputAudioMixerGroup = SoundManager.Instance.SfxGroup;
    }

    // Humanoid rigs expose the bone directly; generic (Mixamo) rigs are found by name.
    Transform FindFoot(HumanBodyBones bone, string boneName)
    {
        if (animator.isHuman) return animator.GetBoneTransform(bone);
        foreach (Transform t in animator.GetComponentsInChildren<Transform>())
            if (t.name.EndsWith(boneName)) return t;
        return null;
    }

    // A step lands when the other foot becomes the lower one, so the sound follows the walk cycle itself.
    void LateUpdate()
    {
        if (current != NPCAnimation.Walking || leftFoot == null || rightFoot == null) return;
        bool lower = leftFoot.position.y < rightFoot.position.y;
        if (lower != leftFootLower && Time.time >= nextStepTime && footstepClips.Length > 0)
        {
            nextStepTime = Time.time + 0.2f; // both feet level during double-support can flicker; ignore that

            footstepSource.pitch = Random.Range(0.9f, 1.1f);
            footstepSource.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)], footstepVolume * Random.Range(0.8f, 1f));
        }
        leftFootLower = lower;
    }

    private NPCAnimation current = NPCAnimation.Idle;

    public void Animate(NPCAnimation anim)
    {
        current = anim;
        if (anim == NPCAnimation.InteractedWith) animator.SetTrigger("InteractedWith");
        if (anim == NPCAnimation.Happy) animator.SetTrigger("IsHappy");
        if (anim == NPCAnimation.Upset) animator.SetTrigger("IsUpset");
    }

    // Re-applied every frame so the bools survive the Animator being disabled and re-enabled.
    void Update()
    {
        animator.SetBool("IsWalking", current == NPCAnimation.Walking);
        animator.SetBool("IsSuspicious", current == NPCAnimation.Suspicious);
        animator.SetBool("IsHoldingItem", current == NPCAnimation.HoldingItem);
    }
}
