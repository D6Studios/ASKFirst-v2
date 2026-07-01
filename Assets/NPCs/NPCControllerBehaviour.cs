using UnityEngine;

public class NPCControllerBehaviour : MonoBehaviour
{
    [SerializeField] bool IsWalking;
    [SerializeField] bool IsSuspicious;
    [SerializeField] bool IsHoldingItem;

    //Temp
    [SerializeField] bool IsInteractedWith;
    [SerializeField] bool IsHappy;
    [SerializeField] bool IsUpset;
    //
    private Animator NPCAnimator;

    void Start()
    {
        NPCAnimator = GetComponent<Animator>();
        Debug.Log("NPCControllerBehaviour: Animator component found and assigned.");
    }

    void Update()
    {
        // Update the animator parameters based on the current state
        Walking(IsWalking);
        Suspicious(IsSuspicious);
        HoldingItem(IsHoldingItem);
        InteractedWith();
        Happy();
        Upset();
    }
    public void Walking(bool IsWalking)
    {
        NPCAnimator.SetBool("IsWalking", IsWalking);
    }

    public void Suspicious(bool IsSuspicious)
    {
        NPCAnimator.SetBool("IsSuspicious", IsSuspicious);
    }

    public void HoldingItem(bool IsHoldingItem)
    {
        NPCAnimator.SetBool("IsHoldingItem", IsHoldingItem);
    }

    public void InteractedWith()
    {
        if (IsInteractedWith) //to be changed
        {
            NPCAnimator.SetTrigger("InteractedWith");
        }
    }

    public void Happy()
    {
        if (IsHappy) //to be changed
        {
            NPCAnimator.SetTrigger("IsHappy");
        }
    }

    public void Upset()
    {
        if (IsUpset) //to be changed
        {
            NPCAnimator.SetTrigger("IsUpset");
        }
        
    }
}
