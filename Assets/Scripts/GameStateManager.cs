using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-1000)] // Make sure this script will run first
public class GameStateManager : MonoBehaviour {
    public enum GameStage {
        Intro,
        WalkingDog,
        DogRanAway,
        Searching,
        Reunited
    }

    private static GameStateManager _instance;

    [Header("Initial State")]
    [SerializeField] private GameStage startingStage = GameStage.Intro;
    private bool playerMovementEnabledAtStart = true;

    public static GameStateManager Instance{
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameStateManager>();
            }

            return _instance;
        }
    }

    public GameStage CurrentStage { get; private set; }
    public bool CanPlayerMove { get; private set; }

    public UnityEvent<GameStage> OnStageChanged = new UnityEvent<GameStage>();
    public UnityEvent<bool> OnPlayerMovementStateChanged = new UnityEvent<bool>();

    private void Awake() {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        SetStage(startingStage, true);
        SetPlayerMovementEnabled(playerMovementEnabledAtStart, true);
    }

    public void SetStage(GameStage newStage) {
        SetStage(newStage, false);
    }

    public void SetPlayerMovementEnabled(bool canMove) {
        SetPlayerMovementEnabled(canMove, false);
    }

    private void SetStage(GameStage newStage, bool isInitialising) {
        if (!isInitialising && newStage == CurrentStage) {
            return;
        }

        CurrentStage = newStage;
        OnStageChanged.Invoke(CurrentStage);
    }

    private void SetPlayerMovementEnabled(bool canMove, bool isInitialising) {
        if (!isInitialising && canMove == CanPlayerMove) {
            return;
        }

        CanPlayerMove = canMove;
        OnPlayerMovementStateChanged.Invoke(CanPlayerMove);
    }
}
