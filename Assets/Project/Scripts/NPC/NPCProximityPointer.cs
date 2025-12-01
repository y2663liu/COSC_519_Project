using UnityEngine;

public class NPCProximityPointer : ProximityInteractableBase
{
    [Header("References")]
    public NPCPointAtTarget pointer;
    public Animator animator;

    [Header("Animation Settings")]
    public bool useTalkingAnimation = true;
    public string talkingBool = "isTalking";

    //[Header("NPC Dialogue")]
    //[SerializeField] private string npcTitle = "Friendly NPC";
    //[SerializeField] [TextArea] private string npcMessage = "Hello! I saw your dog running that way!";
    //[SerializeField] private string npcFooter = "Press A/X to continue";

    protected override void OnPlayerEnteredRange()
    {
        Debug.Log("NPC: Player entered range");

        if (pointer != null)
            pointer.isActive = true;

        if (animator != null && useTalkingAnimation)
            animator.SetBool(talkingBool, true);

        // Show hint popup
        //HintPopup.Instance?.ShowHint(npcTitle, npcMessage, npcFooter, transform);
    }

    protected override void OnPlayerExitedRange()
    {
        Debug.Log("NPC: Player exited range");

        if (pointer != null)
            pointer.isActive = false;

        if (animator != null && useTalkingAnimation)
            animator.SetBool(talkingBool, false);

        // Hide hint popup
        HintPopup.Instance?.HideHint(transform);
    }
}