using UnityEngine;
using UnityEngine.InputSystem;

public class NPCDialogueSitStand : ProximityInteractableBase
{
    [Header("Dialogue")]
    [TextArea] public string[] messages;   // List of messages
    private int index = 0;

    [Header("Animator")]
    public Animator animator;              // NPC Animator (optional)
    public string sitBool = "isSitting";   // Animator bool for sitting animation

    [Header("Input")]
    public InputActionProperty nextMessageAction;   // B button

    private bool playerInside = false;
    private Transform player;

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void OnEnable()
    {
        nextMessageAction.action.performed += OnNextMessage;
    }

    private void OnDisable()
    {
        nextMessageAction.action.performed -= OnNextMessage;
    }

    protected override void OnPlayerEnteredRange()
    {
        playerInside = true;

        // Turn toward the player
        FacePlayer();

        // Switch from sitting to standing
        if (animator != null)
            animator.SetBool(sitBool, false);

        // Start dialogue
        index = 0;
        ShowMessage(index);
    }

    protected override void OnPlayerExitedRange()
    {
        playerInside = false;

        HintPopup.Instance?.HideHint(transform);

        // Sit back down
        if (animator != null)
            animator.SetBool(sitBool, true);
    }

    private void OnNextMessage(InputAction.CallbackContext ctx)
    {
        if (!playerInside) return;

        index++;

        // If more messages remain → show next
        if (index < messages.Length)
        {
            FacePlayer();
            ShowMessage(index);
        }
        else
        {
            // End dialogue
            HintPopup.Instance?.HideHint(transform);

            // Sit again after conversation
            if (animator != null)
                animator.SetBool(sitBool, true);
        }
    }

    private void ShowMessage(int idx)
    {
        HintPopup.Instance?.ShowHint(
            "NPC",
            messages[idx],
            "Press B to continue",
            transform
        );
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
