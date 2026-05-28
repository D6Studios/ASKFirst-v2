using UnityEngine;

public class NPCBehavior : MonoBehaviour
{   
    public float playerDistance;
    [SerializeField]
    public float interactDistance = 3f;
    Outline NPCOutline;
    void Start()
    {
        NPCOutline = gameObject.GetComponentInChildren<Outline>();
        if (NPCOutline != null)
        {
            NPCOutline.enabled = false;
        }
    }
    void Update()
    {
        CheckPlayerProximity();
    }
    void CheckPlayerProximity()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerDistance = Vector3.Distance(player.transform.position, gameObject.transform.position);
        if (playerDistance <= interactDistance)
        {
            player.GetComponent<PlayerInteraction>().NPCInProximity(gameObject);
        }
    }  
    public void Interact()
    {
        //Placeholder for interaction logic
        Debug.Log("Interacted with " + gameObject.name);
    }
    public void OutlineNPC(bool outline)
    {
        if (NPCOutline != null)
        {
            NPCOutline.enabled = outline;
        }
    }
}
