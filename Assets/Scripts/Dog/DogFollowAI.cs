using UnityEngine;

public class DogFollowAI : MonoBehaviour
{
    [Header("Player Tracking")]
    [SerializeField] private Transform player;
    [SerializeField] private float followDistance = 1.5f;
    [SerializeField] private float followForwardDistance = 2f; // in front of player
    [SerializeField] private float followRightOffset     = 10f; // to the player's right

    [SerializeField] private float followHeightOffset = 0f;
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float rotationSpeed = 6f;

    [Header("Run Away Behaviour")]
    [SerializeField] private float runAwaySpeed = 6f;
    [SerializeField] private float runAwayAcceleration = 4f;
    [SerializeField] private float despawnDelaySeconds = 5f;
    [SerializeField] private AudioSource barkAudioSource;
    [SerializeField] private AudioClip barkClip;

    [Header("Animation")]
    [SerializeField] private Animator walkAnimator;
    private const string IS_WALKING = "IsWalking";
    
    [SerializeField] private Animator runAnimator;
    [SerializeField] private string runTriggerName = "Run";

    private GameStateManager _gameState;
    private GameStateManager.GameStage _currentStage;
    private bool _isRunningAway;
    private float _runTimer;
    private float _currentRunSpeed;
    private Vector3 _runDirection = Vector3.forward;
    private bool _isMoving;

    private const string PlayerTag = "Player";

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

        if (barkAudioSource == null)
        {
            barkAudioSource = GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        SubscribeToGameState();
    }

    private void OnDisable()
    {
        UnsubscribeFromGameState();
    }

    private void Update()
    {
        if (_isRunningAway)
        {
            UpdateRunAway();
            return;
        }

        if (_currentStage == GameStateManager.GameStage.WalkingDog ||
            _currentStage == GameStateManager.GameStage.Intro) {
            FollowPlayer();
            walkAnimator = GetComponent<Animator>();
            walkAnimator.SetBool(IS_WALKING, _isMoving);
        }
    }

    private void SubscribeToGameState()
    {
        _gameState = GameStateManager.Instance;
        if (_gameState == null)
        {
            return;
        }

        _currentStage = _gameState.CurrentStage;
        _gameState.OnStageChanged.AddListener(OnStageChanged);
        OnStageChanged(_currentStage);
    }

    private void UnsubscribeFromGameState()
    {
        if (_gameState == null)
        {
            return;
        }

        _gameState.OnStageChanged.RemoveListener(OnStageChanged);
        _gameState = null;
    }

    private void OnStageChanged(GameStateManager.GameStage newStage)
    {
        _currentStage = newStage;

        if (newStage == GameStateManager.GameStage.DogRanAway)
        {
            StartRunAway();
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

    private void StartRunAway()
    {
        if (_isRunningAway)
        {
            return;
        }

        _isRunningAway = true;
        _runTimer = 0f;
        _currentRunSpeed = runAwaySpeed;

        if (player != null)
        {
            _runDirection = transform.position - player.position;
            _runDirection.y = 0f;
        }

        if (_runDirection.sqrMagnitude < 0.001f)
        {
            _runDirection = transform.forward;
        }

        _runDirection = _runDirection.normalized;

        PlayBark();
        TriggerRunAnimation();
    }

    private void UpdateRunAway()
    {
        _runTimer += Time.deltaTime;
        _currentRunSpeed += runAwayAcceleration * Time.deltaTime;

        transform.position += _runDirection * (_currentRunSpeed * Time.deltaTime);

        if (_runDirection.sqrMagnitude > 0.001f)
        {
            var targetRotation = Quaternion.LookRotation(_runDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (_runTimer >= despawnDelaySeconds)
        {
            gameObject.SetActive(false);
        }
    }

    private void PlayBark()
    {
        if (barkAudioSource == null)
        {
            return;
        }

        if (barkClip != null)
        {
            barkAudioSource.PlayOneShot(barkClip);
        }
        else if (!barkAudioSource.isPlaying)
        {
            barkAudioSource.Play();
        }
    }

    private void TriggerRunAnimation()
    {
        if (runAnimator == null || string.IsNullOrEmpty(runTriggerName))
        {
            return;
        }

        runAnimator.ResetTrigger(runTriggerName);
        runAnimator.SetTrigger(runTriggerName);
    }
}
