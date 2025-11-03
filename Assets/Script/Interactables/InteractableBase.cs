using UnityEngine;

public class InteractableBase : MonoBehaviour {
    private GameStateManager.GameStage currentStage = GameStateManager.GameStage.Searching; // TODO: change to Intro after debugging

    protected bool IsEnabled => CheckInteractable();

    protected virtual void Start() {
        var manager = GameStateManager.Instance;
        if (manager != null) {
            manager.OnStageChanged.AddListener(HandleStageChanged);
            manager.OnPlayerMovementStateChanged.AddListener(HandleMovementStateChanged);
        }
    }

    protected virtual void OnDisable() {
        var manager = GameStateManager.Instance;
        if (manager == null) {
            return;
        }

        manager.OnStageChanged.RemoveListener(HandleStageChanged);
        manager.OnPlayerMovementStateChanged.RemoveListener(HandleMovementStateChanged);
    }

    protected virtual void HandleStageChanged(GameStateManager.GameStage stage) {
        currentStage = stage;
    }

    protected virtual void HandleMovementStateChanged(bool canMove) {
    }

    protected bool CheckInteractable() {
        return currentStage == GameStateManager.GameStage.Searching;
    }
}
