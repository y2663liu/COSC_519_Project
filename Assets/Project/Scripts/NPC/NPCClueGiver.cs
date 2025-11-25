using UnityEngine;

public class NPCClueGiver : ProximityInteractableBase
{
    [Header("Hint Content")]
    public string hintTitle = "Hello!";
    public string hintBody = "I think your dog ran that way.";
    public string hintFooter = "Follow where I'm pointing!";
    public Transform targetLocation;

    [Header("References")]
    public Animator animator;
    public NPCPointAtTarget pointer;

    private bool hasActivated = false;

    protected override void OnPlayerEnteredRange()
    {
        if (hasActivated) return;

        HintPopup.Instance?.ShowHint(hintTitle, hintBody, hintFooter, transform);

        animator?.SetBool("isTalking", true);

        pointer.target = targetLocation;
        pointer.isActive = true;

        hasActivated = true;
    }

    protected override void OnPlayerExitedRange()
    {
        HintPopup.Instance?.HideHint(transform);
        animator?.SetBool("isTalking", false);

        pointer.isActive = false;
        pointer.ResetArm();
    }
}
