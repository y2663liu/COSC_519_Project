using System;
using UnityEngine;

public abstract class ProximityInteractableBase : InteractableBase
{
    [Header("Proximity Settings")]
    [SerializeField] private float activationRadius = 2.5f;
    [SerializeField] private float deactivateRadius = 3.2f;
    
    private string PlayerTag = "Player";
    private Transform playerTransform = null;

    private bool _isPlayerInside;

    protected virtual void Start() {
        var player = GameObject.FindGameObjectWithTag(PlayerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
    protected virtual void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        var distance = Vector3.Distance(playerTransform.position, transform.position);
        if (!_isPlayerInside && distance <= activationRadius)
        {
            _isPlayerInside = true;
            OnPlayerEnteredRange();
        }
        else if (_isPlayerInside && distance >= deactivateRadius)
        {
            _isPlayerInside = false;
            OnPlayerExitedRange();
        }
    }

    protected override void OnDisable() {
        base.OnDisable();
    }

    protected abstract void OnPlayerEnteredRange();
    protected abstract void OnPlayerExitedRange();
    
}
