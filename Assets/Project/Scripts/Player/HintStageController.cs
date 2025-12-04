using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The clue tutorial page controller used during the Search stage.
/// Usage:
/// 1. Attach this script to a GameObject in the scene
/// 2. In the Inspector, assign nextPageAction / previousPageAction (typically the controller’s A / B buttons or left/right).
/// 3. When the game stage switches to Search, the clue tutorial UI will automatically appear.
/// </summary>
public class HintStageController : MonoBehaviour
{
    [Serializable]
    private struct HintPage
    {
        public string title;
        public string body;

        public HintPage(string title, string body)
        {
            this.title = title;
            this.body = body;
        }
    }

    [Header("Input")]
    [SerializeField] private InputActionProperty nextPageAction;
    [SerializeField] private InputActionProperty previousPageAction;

    private readonly List<HintPage> _pages = new List<HintPage>();
    private int _currentPageIndex;

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
            Debug.LogWarning("HintStageController: GameStateManager not found in the scene.");
        }
    }

    private void OnEnable()
    {
        if (nextPageAction.reference != null)
        {
            nextPageAction.action.performed += OnNextActionPerformed;
            if (!nextPageAction.action.enabled)
            {
                nextPageAction.action.Enable();
            }
        }

        if (previousPageAction.reference != null)
        {
            previousPageAction.action.performed += OnPreviousActionPerformed;
            if (!previousPageAction.action.enabled)
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
            _gameStateManager.CurrentStage == GameStateManager.GameStage.Search)
        {
            ShowPage(0);
        }
    }

    private void OnStageChanged(GameStateManager.GameStage newStage)
    {
        // Enable only during the Search stage
        bool shouldEnable = newStage == GameStateManager.GameStage.Search;
        if (enabled != shouldEnable)
        {
            enabled = shouldEnable;
        }

        if (shouldEnable)
        {
            // Each time the Search stage begins, start by showing the first tutorial page
            ShowPage(0);
        }
        else
        {
            TutorialPopup.Instance?.HideHint(transform);
        }
    }

    #region Input Callbacks

    private void OnNextActionPerformed(InputAction.CallbackContext context)
    {
        if (!enabled)
        {
            return;
        }

        if (context.performed)
        {
            var nextIndex = _currentPageIndex + 1;
            if (nextIndex < _pages.Count)
            {
                ShowPage(nextIndex);
            }
            else
            {
                TutorialPopup.Instance?.HideHint(transform);
                _isFinished = true;
            }
        }
    }

    private void OnPreviousActionPerformed(InputAction.CallbackContext context)
    {
        if (!enabled)
        {
            return;
        }

        if (context.performed && _currentPageIndex > 0)
        {
            ShowPage(_currentPageIndex - 1);
        }
    }

    #endregion

    #region Pages

    private void BuildPages()
    {
        _pages.Clear();

        _pages.Add(new HintPage(
            "Search Objective",
            "Now I’ll guide you through interacting with clues to help you find the dog."
        ));
        
        _pages.Add(new HintPage(
            "Map Interaction",
            "Use the LEFT controller trigger to emit a ray at the campus map. Hover over each building icon to see where it is located."));
        _pages.Add(new HintPage(
            "Building Interaction",
            "When you are near a building, use the LEFT controller trigger to point at it. It will show the building’s name and basic information."));
        _pages.Add(new HintPage(
            "Paw Prints Clues",
            "Watch for dog paw clues on the ground. Walk toward highlighted dog paws to reveal hints about where to go next."));
        _pages.Add(new HintPage(
            "Talk to NPCs",
            "Some characters can share useful information. Go to NPC to hear what they have seen."));
        
        _pages.Add(new HintPage(
            "Ready to Search",
            "You now know how to use clues to search:\n\n" +
            "Begin your journey and start looking for the dog!"
        ));
    }

    private void ShowPage(int index)
    {
        if (_pages.Count == 0 || _isFinished)
        {
            return;
        }

        _currentPageIndex = Mathf.Clamp(index, 0, _pages.Count - 1);
        var page = _pages[_currentPageIndex];

        string footerText = "Press A to continue.\nPress B to go back.";

        TutorialPopup.Instance?.ShowHint(page.title, page.body, footerText, transform);
    }
    
    public void RedisplayCurrentPage()
    {
       
        
        ShowPage(_currentPageIndex);
    }

    #endregion
}
