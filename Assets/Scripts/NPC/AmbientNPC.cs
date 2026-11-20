using UnityEngine;

// Non-objective "background" customer. The NPC stays silent until the player picks an approach line,
// then gives a short reply. No mood, mistakes or objectives are affected.
// Add to any NPC with NPCBehavior (leave isShoplifter off); NPCDialogue is not needed.
public class AmbientNPC : MonoBehaviour
{
    [System.Serializable]
    public class Approach
    {
        public string playerLine;
        public string npcReply;
    }

    [Tooltip("First two are shown as options.")]
    public Approach[] approaches =
    {
        new Approach { playerLine = "Good morning, how can I help you?", npcReply = "It's okay, thank you for asking." },
        new Approach { playerLine = "Hi there, are you finding everything okay?", npcReply = "Yes, I'm fine. Thanks!" },
    };
    public string closingLine = "No worries, let me know if you need any assistance.";
    public string promptText = "(Choose how to approach the customer)";
}
