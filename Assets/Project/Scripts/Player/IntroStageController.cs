using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class IntroStageController : MonoBehaviour
{
    private enum TutorialAction
    {
        None,
        Rotation,
        Movement,
        Teleport
    }

    [Serializable]
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
    private Transform playerTransform;
    private const string PlayerTag = "Player";

    [Header("Locomotion Settings")]
    [SerializeField] private float rotationThresholdDegrees = 45f;
    [SerializeField] private float movementDistanceThreshold = 0.5f;
    [SerializeField] private float teleportDistancePerFrameThreshold = 1.25f;

    [Header("Input")]
    [SerializeField] private InputActionProperty nextPageAction;
    [SerializeField] private InputActionProperty previousPageAction;

    private readonly List<TutorialPage> _pages = new List<TutorialPage>();

    private int _currentPageIndex;
    private TutorialAction _currentAction = TutorialAction.None;
    private bool _popupClosed;
    private int _instructionLength = Enum.GetValues(typeof(TutorialAction)).Length - 1; // count how many pages we have for instructions

    private Vector3 _rotationReferenceForward;
    private Vector3 _movementReferencePosition;
    private Vector3 _previousRigPosition;
    
    private GameStateManager _gameStateManager;
    private bool _isFinished = false;
    

    private void Awake()
    {
        BuildPages();
        
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

    private void OnEnable()
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

    private void OnDisable()
    {
        if (nextPageAction.reference != null)
        {
            nextPageAction.action.performed -= OnNextActionPerformed;
        }
        
        if (previousPageAction.reference != null)
        {
            previousPageAction.action.performed -= OnPreviousActionPerformed;
        }
        
        TutorialPopup.Instance?.HideHint(transform);
    }

    private void Start()
    {
        if (_gameStateManager != null &&
            _gameStateManager.CurrentStage != GameStateManager.GameStage.Intro)
        {
            _gameStateManager.SetStage(GameStateManager.GameStage.Intro);
        }
        
        var player = GameObject.FindGameObjectWithTag(PlayerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }
        _previousRigPosition = GetPlayerPosition();

        ShowPage(0);
    }
    
    private void OnStageChanged(GameStateManager.GameStage newStage)
    {
        bool shouldEnable = newStage == GameStateManager.GameStage.Intro;
        if (enabled != shouldEnable)
        {
            enabled = shouldEnable;
        }
    }

    private void Update()
    {
        EvaluateCurrentAction();
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
            "Push the right joystick forward, aim the arc, and release to teleport.",
            TutorialAction.Teleport));
    }

    private void OnNextActionPerformed(InputAction.CallbackContext context)
    {
        if (!enabled)
        {
            return;
        }

        if (context.performed)
        {   
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

        if (context.performed && _currentPageIndex > _instructionLength)
        {
            ShowPage(_currentPageIndex - 1);
        }
    }

    private void ShowPage(int index)
    {
        if (_pages.Count == 0 || _isFinished)
        {
            return;
        }
        
        _currentPageIndex = Mathf.Clamp(index, 0, _pages.Count - 1);
        var page = _pages[_currentPageIndex];
        _currentAction = page.action;

        String footerText = page.action == TutorialAction.None ? "Press A to continue.\nPress B to go back." : "Follow the prompt to continue.";
        
        TutorialPopup.Instance?.ShowHint(page.title, page.body, footerText, transform);

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

    private void AdvancePage() {
        var nextIndex = _currentPageIndex + 1;
        if (nextIndex < _pages.Count) {
            ShowPage(nextIndex);
        }
        else {
            _isFinished = true;
            TutorialPopup.Instance?.HideHint(transform);
            
            
            if (_gameStateManager != null) {
                _gameStateManager.SetStage(GameStateManager.GameStage.WalkWithDog);
            }
        }
    }

    private void EvaluateCurrentAction()
    {
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
}
