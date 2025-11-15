using UnityEngine;

public class DogFollowAI : MonoBehaviour
{
    [Header("Player Tracking")]
    [SerializeField] private Transform player;
    private const string PlayerTag = "Player";
    
    [Header("Follow Behaviour")]
    [SerializeField] private float followDistance = 1.5f;
    [SerializeField] private float followForwardDistance = 2f; // in front of player
    [SerializeField] private float followRightOffset     = 0.5f; // to the player's right

    [SerializeField] private float followHeightOffset = 0f;
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float rotationSpeed = 6f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    private string layerName = "Walk";
    private int _layerIndex = 0;
    
    private const string IsWalking = "IsWalking";
    
    private bool _isMoving;

    private void Awake()
    {
        if (player == null)
        {
            var playerObject = GameObject.FindGameObjectWithTag(PlayerTag);
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
        
        var manager = GameStateManager.Instance;
        if (manager == null)
        {
            return;
        }
        
        manager.OnStageChanged.AddListener(OnStageChanged);
        
        if (!animator) animator = GetComponent<Animator>();
    }

    private void Start() {
        for (int i = 0; i < animator.layerCount; i++) {
            animator.SetLayerWeight(i, i == _layerIndex ? 1f : 0f);
        }
    }

    private void Update()
    {
        FollowPlayer();
        animator.SetBool(IsWalking, _isMoving);
    }

    private void OnStageChanged(GameStateManager.GameStage newStage)
    {
        if (newStage == GameStateManager.GameStage.Intro || newStage == GameStateManager.GameStage.WalkWithDog) {
            enabled = true;
        }
        else {
            enabled = false;
        }
    }

    private void FollowPlayer()
    {
        if (player == null)
        {
            return;
        }
        
        // Planar forward/right (ignore pitch/roll)
        Vector3 fwd = player.forward; 
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 1e-6f) fwd = Vector3.forward;
        fwd.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, fwd); // player's right on XZ
        
        var desiredPosition = player.position
                              + fwd   * followForwardDistance 
                              + right * followRightOffset;
        desiredPosition.y = player.position.y + followHeightOffset;
        
        // Move and measure how far we moved this loop
        _isMoving = transform.position != desiredPosition;
        
        var maxStep = followSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, maxStep);
        
        var lookDirection = player.position - transform.position;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            var targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
