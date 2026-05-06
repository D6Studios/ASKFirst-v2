using UnityEngine;
using StarterAssets;
using Unity.Multiplayer.Center.Common.Analytics;
using System;
using NUnit.Framework;
public class PlayerInteraction : MonoBehaviour
{
    private StarterAssetsInputs _input;
    public bool isBusy = false;
    //Temporary placeholder
    public GameObject interactText;
    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
    }

    // Update is called once per frame
    void Update()
    {
        Interact();
    }
    void Interact()
    {
        if (_input.interact && !isBusy)
        {
            Debug.Log("Interacting");
            isBusy = true;
            interactText.SetActive(true);
        }
        else if(!_input.interact)
        {
            isBusy=false;
            interactText.SetActive(false);
        }
    }
}
