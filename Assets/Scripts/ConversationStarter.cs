using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueEditor;
using System;

public class ConversationStarter : MonoBehaviour
{
    [SerializeField] private NPCConversation myConversation;


    public void StartConvo()
    {
        ConversationManager.Instance.StartConversation(myConversation);
    }
}
