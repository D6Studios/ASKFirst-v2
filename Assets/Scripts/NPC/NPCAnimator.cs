using UnityEngine;

public class NPCAnimator : MonoBehaviour
{
    [SerializeField] bool IsWalking;
    [SerializeField] bool IsSuspicious;
    [SerializeField] bool IsHoldingItem;

    //Temp
    [SerializeField] bool IsInteractedWith;
    [SerializeField] bool IsHappy;
    [SerializeField] bool IsUpset;
    //
    private Animator NPCAnimatorController;

    void Start()
    {
        NPCAnimatorController = GetComponentInChildren<Animator>();
        Debug.Log("NPCAnimator: Animator component found and assigned.");
    }
    public void Animate(string newState)
    {
        IsWalking = false;
        IsSuspicious = false;
        IsHoldingItem = false;
        IsInteractedWith = false;
        IsHappy = false;
        IsUpset = false;
        if (newState == "Walking")
        {
            IsWalking = true;
        }
        else if (newState == "Suspicious")
        {
            IsSuspicious = true;
        }
        else if (newState == "HoldingItem")
        {
            IsHoldingItem = true;
        }
        else if (newState == "InteractedWith")
        {
            IsInteractedWith = true;
        }
        else if (newState == "Happy")
        {
            IsHappy = true;
        }
        else if (newState == "Upset")
        {
            IsUpset = true;
        }
    }
    public void Update()
    {
        Walking(IsWalking);
        Suspicious(IsSuspicious);
        HoldingItem(IsHoldingItem);
        InteractedWith();
        Happy();
        Upset();
    }
    public void Walking(bool IsWalking)
    {
        NPCAnimatorController.SetBool("IsWalking", IsWalking);
    }

    public void Suspicious(bool IsSuspicious)
    {
        NPCAnimatorController.SetBool("IsSuspicious", IsSuspicious);
    }

    public void HoldingItem(bool IsHoldingItem)
    {
        NPCAnimatorController.SetBool("IsHoldingItem", IsHoldingItem);
    }

    public void InteractedWith()
    {
        if (IsInteractedWith) //to be changed
        {
            NPCAnimatorController.SetTrigger("InteractedWith");
            IsInteractedWith = false; // Reset the flag after triggering the animation
        }
    }

    public void Happy()
    {
        if (IsHappy) //to be changed
        {
            NPCAnimatorController.SetTrigger("IsHappy");
            IsHappy = false; // Reset the flag after triggering the animation
        }
    }

    public void Upset()
    {
        if (IsUpset) //to be changed
        {
            NPCAnimatorController.SetTrigger("IsUpset");
            IsUpset = false; // Reset the flag after triggering the animation
        }

    }
}
