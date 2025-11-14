using System;
using UnityEngine;

public class DogRunAwayAI : MonoBehaviour
{
    [Header("Player Tracking")]
    [SerializeField] private Transform player;
    private const string PlayerTag = "Player";
    
    [Header("Run Away Behaviour")]
    [SerializeField] private float runAwaySpeed = 6f;
    [SerializeField] private float runAwayAcceleration = 4f;
    [SerializeField] private float despawnDelaySeconds = 5f;
    
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
        
        var manager = GameStateManager.Instance;
        if (manager == null)
        {
            return;
        }
        
        manager.OnStageChanged.AddListener(OnStageChanged);
    }
    
    private void Start() {
        for (int i = 0; i < animator.layerCount; i++) {
            animator.SetLayerWeight(i, i == _layerIndex ? 1f : 0f);
        }
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

    void OnEnable() {
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
    }


    private void Update()
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
}
