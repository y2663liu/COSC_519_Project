using UnityEngine;

public class InteractableBase : MonoBehaviour {
    private GameStateManager _gameStateManager;
    private GameStateManager.GameStage currentStage = GameStateManager.GameStage.Intro;
    
    private void Awake()
    {
        _gameStateManager = GameStateManager.Instance;
        _gameStateManager.OnStageChanged.AddListener(OnStageChanged);
    }

    protected virtual void OnDisable() {
        _gameStateManager.OnStageChanged.RemoveListener(OnStageChanged);
    }

    protected virtual void OnStageChanged(GameStateManager.GameStage stage) {
        if (stage == GameStateManager.GameStage.Search) {
            enabled = true;
        }
        else {
            enabled = false;
        }
    }
}
