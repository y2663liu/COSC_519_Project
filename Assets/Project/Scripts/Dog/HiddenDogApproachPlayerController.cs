using UnityEngine;

/// <summary>
/// Move from hidePoint to revealPoint while looking at the player.
/// After reaching revealPoint, circle around it once (while looking in the movement direction).
/// </summary>
public class HiddenDogApproachPlayerController : MonoBehaviour
{
    [Header("Position Points (Transforms only used to read initial world positions)")]
    [Tooltip("The position where the dog hides inside the bushes")]
    public Transform hidePoint;        // Read world position once only
    [Tooltip("The position the dog should reach after emerging (circle center)")]
    public Transform revealPoint;      // Read world position once only

    [Header("Movement from Hide to Reveal")]
    [Tooltip("Time (seconds) required to move from hidePoint to revealPoint")]
    public float revealDuration = 1.5f;

    [Header("Circle Movement Settings at revealPoint")]
    [Tooltip("Time (seconds) needed to complete one full circle")]
    public float circleDuration = 2.5f;
    [Tooltip("Circle radius (meters), using revealPoint as the center")]
    public float circleRadius = 0.6f;
    [Tooltip("Circle clockwise? (unchecked = counter-clockwise)")]
    public bool circleClockwise = true;

    [Header("Animation")]
    [Tooltip("Dog's Animator (DogAnimatorController)")]
    public Animator animator;
    [Tooltip("Bool parameter name that controls walking animation")]
    public string isWalkingParamName = "IsWalking";

    [Header("Audio")]
    public AudioSource barkSource;
    public AudioClip barkClip;

    // Private state
    private bool _hasStarted;          // Whether the full sequence has started
    private bool _reachedReveal;       // Whether the dog has reached revealPoint
    private bool _isCircling;          // Whether the dog is currently circling

    private float _revealTimer;
    private float _circleTimer;
    private float _circleAngle;        // Current angle (radians) during circling

    // Cached world positions for hide/reveal
    private Vector3 _hideWorldPos;
    private Vector3 _revealWorldPos;

    // Player reference — dog looks at player during phase 1
    private Transform _player;

    private void Awake()
    {
        // Automatically find Animator (if not manually assigned in Inspector)
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Cache world positions (avoid parent movement shifting target points)
        _hideWorldPos = hidePoint != null ? hidePoint.position : transform.position;
        _revealWorldPos = revealPoint != null ? revealPoint.position : transform.position;

        // Place dog at hidePoint initially
        transform.position = _hideWorldPos;

        // Find player (XR Rig root must have Tag: "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }

        // Start in Idle animation
        SetIsWalking(false);
    }

    /// <summary>
    /// Called externally: begin the sequence “walk out from the bushes + circle at revealPoint”.
    /// Example usage in trigger: hiddenDog.StartReveal();
    /// </summary>
    public void StartReveal()
    {
        if (_hasStarted) return;

        _hasStarted = true;
        _revealTimer = 0f;
        _circleTimer = 0f;
        _reachedReveal = false;
        _isCircling = false;

        // Begin movement → Walking animation
        SetIsWalking(true);

        // Play bark sound (optional)
        if (barkSource != null && barkClip != null)
        {
            barkSource.clip = barkClip;
            barkSource.Play();
        }
    }

    private void Update()
    {
        if (!_hasStarted) return;

        // Phase 1: move from hidePoint to revealPoint
        if (!_reachedReveal)
        {
            UpdateMoveToReveal();
        }
        // Phase 2: circle around revealPoint
        else if (_isCircling)
        {
            UpdateCircleAroundReveal();
        }
    }

    /// <summary>
    /// Phase 1: Linearly move from hidePoint to revealPoint while facing the player
    /// </summary>
    private void UpdateMoveToReveal()
    {
        _revealTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_revealTimer / revealDuration);

        // Interpolate position
        transform.position = Vector3.Lerp(_hideWorldPos, _revealWorldPos, t);

        // Always look at the player
        LookAtPlayerOnPlane();

        if (t >= 1f)
        {
            // Reached revealPoint
            _reachedReveal = true;
            StartCircleAtReveal();
        }
    }

    /// <summary>
    /// Begin circling phase: dog starts circling around revealPoint
    /// </summary>
    private void StartCircleAtReveal()
    {
        _isCircling = true;
        _circleTimer = 0f;

        // Determine starting angle based on XZ plane direction
        // Start at circleRadius distance to the right of center
        Vector3 startOffset;

        if (_player != null)
        {
            // Use player's right direction as starting offset → more natural
            Vector3 right = _player.right;
            right.y = 0f;
            if (right.sqrMagnitude < 0.0001f)
                right = Vector3.right;

            right.Normalize();
            startOffset = right * circleRadius;
        }
        else
        {
            startOffset = Vector3.right * circleRadius;
        }

        // Place dog on the circle perimeter
        transform.position = _revealWorldPos + startOffset;

        // Convert offset to angle
        Vector3 flatOffset = startOffset;
        flatOffset.y = 0f;
        if (flatOffset.sqrMagnitude < 0.0001f)
        {
            flatOffset = Vector3.right * circleRadius;
        }

        _circleAngle = Mathf.Atan2(flatOffset.z, flatOffset.x);

        // Ensure walking animation continues during circling
        SetIsWalking(true);
    }

    /// <summary>
    /// Phase 2: Circle around revealPoint, look in the movement direction (tangent)
    /// </summary>
    private void UpdateCircleAroundReveal()
    {
        if (circleDuration <= 0f)
        {
            FinishSequence();
            return;
        }

        _circleTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(_circleTimer / circleDuration);

        // Full circle = 2π
        float fullCircle = Mathf.PI * 2f;
        float dirSign = circleClockwise ? -1f : 1f;

        // Angle increment per frame
        float deltaAngle = dirSign * fullCircle * (Time.deltaTime / circleDuration);
        _circleAngle += deltaAngle;

        // Compute new position on the circle (XZ only)
        Vector3 offset = new Vector3(
            Mathf.Cos(_circleAngle),
            0f,
            Mathf.Sin(_circleAngle)
        ) * circleRadius;

        Vector3 newPos = _revealWorldPos + offset;
        newPos.y = transform.position.y;
        transform.position = newPos;

        // Look in tangent direction (forward direction of circle path)
        Vector3 forward = new Vector3(
            -Mathf.Sin(_circleAngle) * dirSign,
            0f,
            Mathf.Cos(_circleAngle) * dirSign
        );

        if (forward.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(forward);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * 5f
            );
        }

        // Completed full circle
        if (progress >= 1f)
        {
            FinishSequence();
        }
    }

    /// <summary>
    /// Entire sequence finished → stop movement and return to Idle
    /// </summary>
    private void FinishSequence()
    {
        _hasStarted = false;
        _isCircling = false;

        // Return to Idle
        SetIsWalking(false);

        // Optional: hook into GameStateManager or UI hint system
        // GameStateManager.Instance?.SetStage(GameStage.Reunited);
        // HintPopup.Instance?.ShowHint(...);

        enabled = false;
    }

    /// <summary>
    /// During phase 1, rotate dog to face the player on XZ plane
    /// </summary>
    private void LookAtPlayerOnPlane()
    {
        if (_player == null) return;

        Vector3 lookPos = _player.position;
        lookPos.y = transform.position.y;

        Vector3 dir = lookPos - transform.position;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * 5f   // Adjust rotation speed if needed
        );
    }

    private void SetIsWalking(bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(isWalkingParamName))
        {
            animator.SetBool(isWalkingParamName, value);
        }
    }
}
