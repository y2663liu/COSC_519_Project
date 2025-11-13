using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class IntroScene : MonoBehaviour
{
    private enum TutorialAction
    {
        None,
        Rotation,
        Movement,
        Teleport
    }

    [System.Serializable]
    private struct TutorialPage
    {
        public string title;
        [TextArea]
        public string body;
        public TutorialAction action;

        public TutorialPage(string title, string body, TutorialAction action)
        {
            this.title = title;
            this.body = body;
            this.action = action;
        }
    }
    
    [Header("Player References")]
    [SerializeField] private Transform playerTransform;

    [Header("Locomotion Settings")]
    [SerializeField] private float rotationThresholdDegrees = 45f;
    [SerializeField] private float movementDistanceThreshold = 0.5f;
    [SerializeField] private float teleportDistancePerFrameThreshold = 1.25f;

    [Header("Marker Settings")]
    [SerializeField] private GameObject sceneMarker;
    [SerializeField] private float markerArrivalDistance = 0.75f;

    [Header("Input (optional)")]
    [SerializeField] private InputActionProperty nextPageAction;
    [SerializeField] private InputActionProperty previousPageAction;

    private readonly List<TutorialPage> _pages = new List<TutorialPage>();

    private int _currentPageIndex;
    private TutorialAction _currentAction = TutorialAction.None;
    private bool _isFinalInstructionShown;
    private bool _popupClosed;
    private int _instructionLength = Enum.GetValues(typeof(TutorialAction)).Length - 1; // count how many pages we have for instructions

    private Vector3 _rotationReferenceForward;
    private Vector3 _movementReferencePosition;
    private Vector3 _previousRigPosition;
    
    private bool _markerActive;

    private Camera _cachedCamera;
    
    private string _playerTag = "Player";

    private void Awake()
    {
        BuildPages();
        CacheCamera();
    }

    private void OnEnable()
    {
        SubscribeInput();
    }

    private void OnDisable()
    {
        UnsubscribeInput();
        HideCanvas();
    }

    private void Start()
    {
        _previousRigPosition = GetPlayerPosition();
        var gameState = GameStateManager.Instance;
        if (gameState != null)
        {
            gameState.SetStage(GameStateManager.GameStage.Intro);
        }
        var player = GameObject.FindGameObjectWithTag(_playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }
        if (sceneMarker != null)
        {
            sceneMarker.SetActive(false);
            _markerActive = sceneMarker.activeSelf;
        }
        else {
            Debug.LogWarning("IntroScene: No scene marker assigned for the walking tutorial.");
        }

        ShowPage(0);
    }

    private void Update()
    {
        CacheCamera();
        EvaluateCurrentAction();
        TrackMarkerProgress();

        _previousRigPosition = GetPlayerPosition();
    }

    private void BuildPages()
    {
        _pages.Clear();
        _pages.Add(new TutorialPage(
            "Welcome",
            "Welcome to the courtyard! Try snapping the right joystick left or right to rotate.",
            TutorialAction.Rotation));
        _pages.Add(new TutorialPage(
            "Take a Step",
            "Use the left joystick to walk a short distance. Keep an eye on your surroundings!",
            TutorialAction.Movement));
        _pages.Add(new TutorialPage(
            "Teleport Trial",
            "Press and hold the teleport button, aim the arc, and release to teleport.",
            TutorialAction.Teleport));
        _pages.Add(new TutorialPage(
            "Courtyard Stories",
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum posuere erat in dui consequat.",
            TutorialAction.None));
        _pages.Add(new TutorialPage(
            "Old Memories",
            "Suspendisse potenti. Vivamus et nibh et nisl volutpat tincidunt. Cras vitae nibh non sem pharetra tempor.",
            TutorialAction.None));
        _pages.Add(new TutorialPage(
            "A Calm Afternoon",
            "Integer efficitur, metus sed malesuada egestas, justo sapien gravida nisl, id mattis erat nisl in augue.",
            TutorialAction.None));
    }

    private void SubscribeInput()
    {   
        if (nextPageAction.reference != null)
        {
            nextPageAction.action.performed += OnNextActionPerformed;
            if (!nextPageAction.action.enabled) // TODO: test if we can remove them
            {
                nextPageAction.action.Enable();
            }
        }
        
        if (previousPageAction.reference != null)
        {
            previousPageAction.action.performed += OnPreviousActionPerformed;
            if (!previousPageAction.action.enabled) // TODO: test if we can remove them
            {
                previousPageAction.action.Enable();
            }
        }
    }

    private void UnsubscribeInput()
    {
        if (nextPageAction.reference != null)
        {
            nextPageAction.action.performed -= OnNextActionPerformed;
        }
        
        if (previousPageAction.reference != null)
        {
            previousPageAction.action.performed -= OnPreviousActionPerformed;
        }
    }

    private void OnNextActionPerformed(InputAction.CallbackContext context)
    {
        if (!enabled || _popupClosed)
        {
            return;
        }

        if (context.performed)
        {
            if (_isFinalInstructionShown)
            {
                ClosePopup();
                return;
            }

            if (_currentPageIndex >= _instructionLength)
            {
                AdvancePage();
            }
        }
    }

    private void OnPreviousActionPerformed(InputAction.CallbackContext context)
    {
        if (!enabled || _popupClosed)
        {
            return;
        }

        if (_isFinalInstructionShown)
        {
            return;
        }

        if (context.performed && _currentPageIndex > _instructionLength)
        {
            ShowPage(_currentPageIndex - 1);
        }
    }

    private void ShowPage(int index)
    {
        if (_pages.Count == 0)
        {
            return;
        }
        
        _currentPageIndex = Mathf.Clamp(index, 0, _pages.Count - 1);
        var page = _pages[_currentPageIndex];
        _currentAction = page.action;

        String footerText = page.action == TutorialAction.None ? "Press A to continue.\nPress B to go back." : "Follow the prompt to continue.";
        
        HintPopup.Instance?.ShowHint(page.title, page.body, footerText, transform);

        var currentPosition = GetPlayerPosition();
        _previousRigPosition = currentPosition;

        switch (_currentAction)
        {
            case TutorialAction.Rotation:
                _rotationReferenceForward = GetHorizontalForward();
                break;
            case TutorialAction.Movement:
                _movementReferencePosition = currentPosition;
                break;
            case TutorialAction.Teleport:
                // Reset previous position so the next large jump is counted correctly
                _previousRigPosition = currentPosition;
                break;
        }
    }

    private void AdvancePage()
    {
        if (_isFinalInstructionShown)
        {
            return;
        }

        var nextIndex = _currentPageIndex + 1;
        if (nextIndex < _pages.Count)
        {
            ShowPage(nextIndex);
        }
        else
        {
            ShowFinalInstructions();
        }
    }

    private void ShowFinalInstructions()
    {
        _isFinalInstructionShown = true;
        var anchor = playerTransform;
        if (anchor == null)
        {
            anchor = transform;
        }
        
        // Active Map Marker
        sceneMarker.SetActive(true);
        _markerActive = true;
        
        HintPopup.Instance?.ShowHint("Time for a Walk",
            "A glowing marker in the courtyard is highlighting where to go. Walk over to that spot to keep up with your pup.",
            "Press A to close this window.",
            transform);

        var gameState = GameStateManager.Instance;
        if (gameState != null)
        {
            gameState.SetStage(GameStateManager.GameStage.WalkingDog);
        }
    }

    private void ClosePopup()
    {
        if (_popupClosed)
        {
            return;
        }

        _popupClosed = true;
        HideCanvas();
    }

    private void HideCanvas()
    {
        HintPopup.Instance?.HideHint(transform);
    }

    private void EvaluateCurrentAction()
    {
        if (_isFinalInstructionShown)
        {
            return;
        }

        switch (_currentAction)
        {
            case TutorialAction.Rotation:
                EvaluateRotation();
                break;
            case TutorialAction.Movement:
                EvaluateMovement();
                break;
            case TutorialAction.Teleport:
                EvaluateTeleport();
                break;
        }
    }

    private void EvaluateRotation()
    {
        var currentForward = GetHorizontalForward();
        if (currentForward.sqrMagnitude < Mathf.Epsilon || _rotationReferenceForward.sqrMagnitude < Mathf.Epsilon)
        {
            return;
        }

        var angle = Vector3.Angle(_rotationReferenceForward, currentForward);
        if (angle >= rotationThresholdDegrees)
        {
            AdvancePage();
        }
    }

    private void EvaluateMovement()
    {
        var currentPosition = GetPlayerPosition();
        var distance = Vector2.Distance(new Vector2(currentPosition.x, currentPosition.z),
            new Vector2(_movementReferencePosition.x, _movementReferencePosition.z));
        if (distance >= movementDistanceThreshold)
        {
            AdvancePage();
        }
    }

    private void EvaluateTeleport()
    {
        var currentPosition = GetPlayerPosition();
        var frameDistance = Vector2.Distance(new Vector2(currentPosition.x, currentPosition.z),
            new Vector2(_previousRigPosition.x, _previousRigPosition.z));
        if (frameDistance >= teleportDistancePerFrameThreshold)
        {
            AdvancePage();
        }
    }

    private void ActivateMarker()
    {
        sceneMarker.SetActive(true);
        _markerActive = true;
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

    private Vector3 GetHorizontalForward() {
        Transform source = playerTransform;
        if (source == null)
        {
            return Vector3.forward;
        }

        var forward = source.forward;
        forward.y = 0f;
        return forward.normalized;
    }

    private void CacheCamera()
    {
        if (_cachedCamera != null)
        {
            return;
        }

        if (Camera.main != null)
        {
            _cachedCamera = Camera.main;
        }
    }
}
