using UnityEngine;
using UnityEngine.InputSystem;

public class NPCDialogueSitStand : ProximityInteractableBase
{
    [Header("Dialogue")]
    [TextArea] public string messages;   // messages

    [Header("Animator")]
    public Animator animator;              // NPC Animator (optional)
    public string sitBool = "isSitting";   // Animator bool for sitting animation

    private bool playerInside = false;
    private Transform player;

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected override void OnPlayerEnteredRange()
    {
        playerInside = true;

        // Turn toward the player
        FacePlayer();

        // Switch from sitting to standing
        if (animator != null)
            animator.SetBool(sitBool, false);

        HintPopup.Instance?.ShowHint(
            "NPC",
            messages,
            "",
            transform
        );
    }

    protected override void OnPlayerExitedRange()
    {
        playerInside = false;

        HintPopup.Instance?.HideHint(transform);

        // Sit back down
        if (animator != null)
            animator.SetBool(sitBool, true);
    }

    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;  // keep NPC upright

        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}
