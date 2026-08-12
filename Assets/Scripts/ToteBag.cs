using UnityEngine;

public class ToteBag : MonoBehaviour
{
    [SerializeField] private GameObject toteBag;
    private Transform toteBagTransform;
    [SerializeField] private Quaternion toteBagWalkRotation = Quaternion.Euler(0f, 0f, 0f);
    [SerializeField] private Quaternion toteBagIdleRotation = Quaternion.Euler(0f, 0f, 0f);
    [SerializeField] private Vector3 toteBagWalkPosition = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 toteBagIdlePosition = new Vector3(0f, 0f, 0f);

    private NPCBehavior npcBehavior;
    void Start()
    {
        npcBehavior = GetComponent<NPCBehavior>();
        toteBagTransform = toteBag.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (npcBehavior.currentState == "Walking")
        {
            toteBagTransform.localRotation = toteBagWalkRotation;
            toteBagTransform.localPosition = toteBagWalkPosition;
        }
        else
        {
            toteBagTransform.localRotation = toteBagIdleRotation;
            toteBagTransform.localPosition = toteBagIdlePosition;
        }
    }
}
