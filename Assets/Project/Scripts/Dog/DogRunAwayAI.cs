using System;
using UnityEngine;

public class DogRunAwayAI : MonoBehaviour
{
    [Header("Player Tracking")]
    [SerializeField] private Transform player;
    private const string PlayerTag = "Player";
    
    [Header("Run Away Behaviour")]
    private const float RunAwaySpeed = 2f;
    private const float RunAwayAcceleration = 1f;
    private const float DespawnDelaySeconds = 5f;
    
    [SerializeField] private float rotationSpeed = 6f;
    [SerializeField] private AudioSource barkAudioSource;
    [SerializeField] private AudioClip barkClip;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    private string layerName = "Run";
    private int _layerIndex = 1;
    
    private float _runTimer;
    private float _currentRunSpeed;
    private Vector3 _runDirection = Vector3.forward;
    
    private GameStateManager _gameStateManager;
    
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
        
        _gameStateManager = GameStateManager.Instance;
        _gameStateManager.OnStageChanged.AddListener(OnStageChanged);
    }
    
    private void Start() {
        for (int i = 0; i < animator.layerCount; i++) {
            animator.SetLayerWeight(i, i == _layerIndex ? 1f : 0f);
        }
        
        _runTimer = 0f;
        _currentRunSpeed = RunAwaySpeed;

        if (player != null)
        {
            _runDirection = new Vector3(0f, 0f, -1f);
        }

        if (_runDirection.sqrMagnitude < 0.001f)
        {
            _runDirection = transform.forward;
        }

        _runDirection = _runDirection.normalized;

        PlayBark();
    }
    
    private void OnStageChanged(GameStateManager.GameStage newStage)
    {
        if (newStage == GameStateManager.GameStage.DogRanAway) {
            enabled = true;
        }
        else {
            enabled = false;
        }
    }

    private void Update()
    {
        _runTimer += Time.deltaTime;
        _currentRunSpeed += RunAwayAcceleration * Time.deltaTime;

        transform.position += _runDirection * (_currentRunSpeed * Time.deltaTime);

        if (_runDirection.sqrMagnitude > 0.001f)
        {
            var targetRotation = Quaternion.LookRotation(_runDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (_runTimer >= DespawnDelaySeconds)
        {
            if (_gameStateManager != null) {
                _gameStateManager.SetStage(GameStateManager.GameStage.Search);
            }
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
}
