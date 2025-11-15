using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class WalkWithDogStageController : MonoBehaviour {
    [Header("Player References")]
    [SerializeField] private Transform playerTransform;
    private const string PlayerTag = "Player";
    
    [Header("Map Marker Settings")]
    [SerializeField] private GameObject sceneMarker;
    [SerializeField] private float markerArrivalDistance = 0.75f;
    private bool _markerActive;
    
    [Header("Input")]
    [SerializeField] private InputActionProperty closePageAction;
    
    private GameStateManager _gameStateManager;
    
    private void Awake()
    {
        _gameStateManager = GameStateManager.Instance;
        if (_gameStateManager != null)
        {
            _gameStateManager.OnStageChanged.AddListener(OnStageChanged);
            OnStageChanged(_gameStateManager.CurrentStage);
        }
        else
        {
            Debug.LogWarning("IntroStageController: GameStateManager not found in the scene.");
        }
    }

    private void OnEnable() {
        if (closePageAction.reference != null) {
            closePageAction.action.performed += OnCloseActionPerformed;
            if (!closePageAction.action.enabled) // TODO: test if we can remove them
            {
                closePageAction.action.Enable();
            }
        }
    }
    
    private void Start() {
        if (sceneMarker != null) {
            sceneMarker.SetActive(false);
            _markerActive = sceneMarker.activeSelf;
        }
        else {
            Debug.LogWarning("IntroScene: No scene marker assigned for the walking tutorial.");
        }
        
        ShowFinalInstructions();
    }
    
    private void Update() {
        TrackMarkerProgress();
    }
    
    private void OnDisable()
    {
        if (closePageAction.reference != null)
        {
            closePageAction.action.performed -= OnCloseActionPerformed;
        }
        
        HintPopup.Instance?.HideHint(transform);
    }
    
    private void OnStageChanged(GameStateManager.GameStage newStage)
    {
        bool shouldEnable = newStage == GameStateManager.GameStage.WalkWithDog;
        if (enabled != shouldEnable)
        {
            enabled = shouldEnable;
        }
    }
    
    private void OnCloseActionPerformed(InputAction.CallbackContext context)
    {
        if (!enabled)
        {
            return;
        }

        if (context.performed)
        {
            HintPopup.Instance?.HideHint(transform);
        }
    }

    private void ShowFinalInstructions()
    {
        // Active Map Marker
        sceneMarker.SetActive(true);
        _markerActive = true;
        
        HintPopup.Instance?.ShowHint("Time for a Walk",
            "A glowing marker in the courtyard is highlighting where to go. Walk over to that spot to keep up with your pup.",
            "Press A to close this window.",
            transform);
    }

    private void TrackMarkerProgress()
    {
        if (!_markerActive || sceneMarker == null)
        {
            return;
        }

        var playerPosition = GetPlayerPosition();
        var targetPosition = sceneMarker.transform.position;
        var distance = Vector2.Distance(new Vector2(playerPosition.x, playerPosition.z),
            new Vector2(targetPosition.x, targetPosition.z));
        if (distance <= markerArrivalDistance)
        {
            _markerActive = false;
            sceneMarker.SetActive(false);

            var gameState = GameStateManager.Instance;
            if (gameState != null)
            {
                gameState.SetStage(GameStateManager.GameStage.DogRanAway);
            }
        }
    }
    
    private Vector3 GetPlayerPosition()
    {
        if (playerTransform != null)
        {
            return playerTransform.position;
        }

        return transform.position;
    }
}
