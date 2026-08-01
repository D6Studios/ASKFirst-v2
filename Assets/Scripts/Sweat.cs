using UnityEngine;

public class Sweat : MonoBehaviour
{
    [SerializeField]
    private
    GameObject SweatParticleSystem;
    [SerializeField]
    Transform playerHeadTransform;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        SweatParticleSystem.GetComponent<Transform>().position = new Vector3(playerHeadTransform.position.x, playerHeadTransform.position.y + 0.3f, playerHeadTransform.position.z);
        SweatParticleSystem.GetComponent<Transform>().rotation = new Quaternion(playerHeadTransform.rotation.x, playerHeadTransform.rotation.y, playerHeadTransform.rotation.z, playerHeadTransform.rotation.w);
    }
}
