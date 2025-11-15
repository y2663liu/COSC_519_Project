using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HoverClue : InteractableBase {
    [SerializeField] string title;
    [SerializeField] string clues;
    [SerializeField] string funFacts;

    private string HintMessage;
    
    [SerializeField] XRBaseInteractable rayInteractable;

    protected void Start() {
        rayInteractable.hoverEntered.AddListener(OnHoverEntered);
        rayInteractable.hoverExited.AddListener(OnHoverExited);
    }

    public void OnHoverEntered(HoverEnterEventArgs args) {
        HintPopup.Instance?.ShowHint(title, clues, funFacts, transform);
    }

    public void OnHoverExited(HoverExitEventArgs args) {
        HintPopup.Instance?.HideHint(transform);
    }

    protected override void OnDisable() {
        base.OnDisable();
        if (HintPopup.Instance != null)
        {
            HintPopup.Instance.HideHint(transform);
        }
    }
}
