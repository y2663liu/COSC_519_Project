using UnityEngine;

public class NPCSimpleMessage : ProximityInteractableBase
{
    [Header("NPC Message")]
    [SerializeField] private string title = "NPC";
    [SerializeField] [TextArea] private string message = "Hello! I saw something interesting over there.";
    [SerializeField] private string footer = "Press A/X to continue";

    protected override void OnPlayerEnteredRange()
    {
        Debug.Log("NPC: Player entered range");
        HintPopup.Instance?.ShowHint(title, message, footer, transform);
    }

    protected override void OnPlayerExitedRange()
    {
        Debug.Log("NPC: Player exited range");
        HintPopup.Instance?.HideHint(transform);
    }
}
