using UnityEngine;

public class DogPawsVisibility : MonoBehaviour
{
    [Header("显示脚印的阶段：Search 和 Reunited")]
    private GameStateManager _gameStateManager;

    private Renderer[] _renderers;

    private void Awake()
    {
        // Find all Renderers on this object and its children (MeshRenderer / SpriteRenderer, etc.)
        _renderers = GetComponentsInChildren<Renderer>(true);

        _gameStateManager = GameStateManager.Instance;
        if (_gameStateManager != null)
        {
            _gameStateManager.OnStageChanged.AddListener(OnStageChanged);
            // Refresh visibility once on scene load
            OnStageChanged(_gameStateManager.CurrentStage);
        }
        else
        {
            Debug.LogWarning("DogPawsVisibility: GameStateManager.Instance is null.");
        }
    }

    private void OnDestroy()
    {
        if (_gameStateManager != null)
        {
            _gameStateManager.OnStageChanged.RemoveListener(OnStageChanged);
        }
    }

    private void OnStageChanged(GameStateManager.GameStage stage)
    {
        // Show footprints in Search and Reunited stages
        bool visible = (stage == GameStateManager.GameStage.Search ||
                        stage == GameStateManager.GameStage.Reunited);

        foreach (var r in _renderers)
        {
            if (r != null)
                r.enabled = visible;   // Only toggle rendering, keep GameObjects active
        }
    }
}