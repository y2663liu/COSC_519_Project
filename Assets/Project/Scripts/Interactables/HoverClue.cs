using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HoverClue : InteractableBase
{
    [SerializeField] string title;
    [SerializeField] string clues;
    [SerializeField] string funFacts;

    private string HintMessage;

    [SerializeField] XRBaseInteractable rayInteractable;

    private AudioSource audioSource;         
    [SerializeField] private AudioClip audioClip;

    protected void Start()
    {
        rayInteractable.hoverEntered.AddListener(OnHoverEntered);
        rayInteractable.hoverExited.AddListener(OnHoverExited);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        HintPopup.Instance?.ShowHint(title, clues, funFacts, transform);

        // Play sound effect
        if (audioSource != null && audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }

    public void OnHoverExited(HoverExitEventArgs args)
    {
        HintPopup.Instance?.HideHint(transform);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (HintPopup.Instance != null)
        {
            HintPopup.Instance.HideHint(transform);
        }
    }
}