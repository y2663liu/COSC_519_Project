using UnityEngine;

/// <summary>
/// When the player enters this trigger, notify the HiddenDogController to make the dog appear
/// </summary>
[RequireComponent(typeof(Collider))]
public class DogRevealTrigger : MonoBehaviour
{
    public HiddenDogApproachPlayerController hiddenDog;
    public bool oneShot = true;   // Trigger only once

    private bool _triggered = false;

    private void Reset()
    {
        // Ensure this collider is set as a trigger
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (oneShot && _triggered) return;

        // Allow the player’s child objects to have colliders, as long as the root object has the Player tag
        bool isPlayer =
            other.CompareTag("Player") ||
            other.transform.root.CompareTag("Player");

        if (!isPlayer) return;

        _triggered = true;

        if (hiddenDog != null)
        {
            hiddenDog.StartReveal();
        }
    }
}